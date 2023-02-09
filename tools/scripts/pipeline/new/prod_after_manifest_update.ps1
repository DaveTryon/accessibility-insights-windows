# This file will run after updating the Production manifest
#
# Environment variables set by the pipeline:
#     GITHUB_TOKEN              is the PAT to access the repo
#     OctokitVersion            is the version of OctoKit we've pinned to. A previous task will install this package
#     DeleteReleases            is true if cleanup should actually delete (if false, just report what would be cleaned up)
#     IsForcedProdUpdate        is true if cleanup should keep only 1 production release instead of 2
#
# Objective: Cleanup of unneeded releases & tags. We want to keep the following:
#  - All draft releases newer than the version being promoted to Production
#  - The 2 (or 1 if IsForcedProdUpdate is true) most recent non-draft releases
#  - The tags associated with non-draft releases that we are removing
#
# This assumes that the task is run from the default working directory

# Constants
$Owner = "Microsoft"
$Repo = "accessibility-insights-windows"

# Initialize the client
function Get-Client()
{
    if ($null -eq $env:GITHUB_TOKEN)
    {
        throw "Run this script with the GITHUB_TOKEN environment variable set to a GitHub Personal Access Token with 'repo' permissions"
    }

    # Load the octokit dll
    Add-Type -Path ((Get-Location).Path + "\Octokit.$($env:OctokitVersion)\lib\netstandard2.0\Octokit.dll")

    # Get a new client
    $productHeader = [Octokit.ProductHeaderValue]::new("Production-Pipeline-PreValidation")
    $client = [Octokit.GitHubClient]::new($productHeader)

    # Add credentials for authentication
    $client.Credentials = [Octokit.Credentials]::new($env:GITHUB_TOKEN)
    return $client
}

# Extracts a version from the release name.
function Get-ReleaseVersion($specificRelease)
{
    $versionString = $specificRelease.Name
    if($versionString -match ".*v(\d+\.\d+\.\d+\.\d+).*")
    {
        return $matches[1]
    }
    else
    {
        return [string]::Empty
    }
}

# Dump the sorted releases for debugging
function Write-ReleaseMap($releaseMap)
{
    Write-Host "==================================="
    Write-Host "Sorted Releases"
    Write-Host "Version       Name"
    Write-Host "-------       ----"
    foreach($releaseKV in $releaseMap)
    {
        Write-Host $releaseKV.Key "  " $releaseKV.Value.Name
    }
    Write-Host "==================================="
}

# Sorts the releases based on the version number
function SortReleases($releases)
{
    $releaseMap = @{}
    foreach($release in $releases.Result)
    {
        $releaseVersion = [System.Version](Get-ReleaseVersion $release)
        if(-Not [string]::IsNullOrEmpty($releaseVersion))
        {
            $releaseMap.Add($releaseVersion,$release)
        }
    }

    $releaseMap = $releaseMap.GetEnumerator() | Sort-Object -Descending -Property Name

    Write-ReleaseMap $releaseMap

    return $releaseMap
}

# Delete a release
function Remove-ReleaseAndPerhapsItsTag($client, $release, $deleteReleases)
{
    # Null checks
    if([string]::IsNullOrEmpty($release.Id))
    {
        Write-Host "No Release Id found"
        return;
    }

    $wasPrerelease = $release.Prerelease
    if($deleteReleases -eq $true)
    {
        Write-Host "Deleting release '$($release.Name)'"
        $response = $client.Repository.Release.Delete($Owner, $Repo, $release.Id);

        if (-Not ($response.Result -eq "NoContent" -And $response.IsCompleted -eq $True))
        {
            Write-Host "Failed to delete release '$($release.Name)'"
        }
        else 
        {
            if ($wasPrerelease)
            {
                Write-Host "Deleting prerelease tag '$($release.TagName)"
                #$client.Git.Reference.Delete($Owner, $Repo, "tags/$($release.TagName)").Wait()
            }
            else
            {
                Write-Host "Retaining release tag '$($release.TagName)'"
            }
        }
    }
    else
    {
        Write-Host "Would have deleted release '$($release.Name)'"
        if ($wasPrerelease)
        {
            Write-Host "Would have deleted prerelease tag '$($release.TagName)'"
        }
        else
        {
            Write-Host "Would have retained release tag '$($release.TagName)'"
        }
    }
}


# Main program
$client = Get-Client
$releases = $client.Repository.Release.GetAll($Owner, $Repo)
$deleteReleases = $env:DeleteReleases -eq "true"
$isForcedProdUpdate = $env:IsForcedProdUpdate -eq "true"

Write-Host "Releases found" 
$releases.Result | Select-Object -Property Name, TagName | Format-Table

$releaseMap = SortReleases $releases

$prodCount = 0
if ($isForcedProdUpdate -eq $true)
{
    $prodVersionsToKeep = 1
}
else
{
    $prodVersionsToKeep = 2
}

foreach($releaseKV in $releaseMap)
{
    $release = $releaseKV.Value
    if ($release.Prerelease -eq $true)
    {
        if($prodCount -gt 0)
        {
            $deleteList += $release
            continue
        }
    }
    else
    {
        if($prodCount -lt $prodVersionsToKeep)
        {
            $prodCount++
            continue
        }
        $deleteList += $release
    }
}

Write-Host "IsForcedProdUpdate = $($isForcedProdUpdate)"
Write-Host "Delete releases option is set to $($deleteReleases)" 

if($deleteList.Count -eq 0)
{
    Write-Host "No releases to delete."
}
else
{
    $deleteList | ForEach-Object { Remove-ReleaseAndPerhapsItsTag $client $_ $deleteReleases }
}
