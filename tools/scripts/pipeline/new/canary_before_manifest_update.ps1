# This will run before updating the Canary manifest
#
# Environment variables set by the pipeline:
#     A11yInsightsVersion       is the version (e.g., 1.1.2227.01) begin published by the pipeline
#     OctokitVersion            is the version of OctoKit we've pinned to. A previous task will install this package
#
# Objective: Return error if a more recent release (i.e., a higher version) already exists
#
# This assumes that the task is run from the default working directory

# Constants
$Owner = "Microsoft"
$Repo = "accessibility-insights-windows"

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
    Write-Host "Version       TagName         Name"
    Write-Host "-------       -------         ----"
    foreach($releaseKV in $releaseMap)
    {
        Write-Host $releaseKV.Key "  " $releaseKV.Value.TagName "  " $releaseKV.Value.Name
    }
    Write-Host "==================================="
}

# Sorts the releases based on the version number
function SortReleases($releases)
{
    $releaseMap = @{}
    foreach($release in $releases)
    {
        $releaseVersion = [System.Version](Get-ReleaseVersion $release)
        if(-not [string]::IsNullOrEmpty($releaseVersion))
        {
            $releaseMap.Add($releaseVersion,$release)
        }
    }

    $releaseMap = $releaseMap.GetEnumerator() | Sort-Object -Descending -Property Name

    Write-ReleaseMap $releaseMap

    return $releaseMap
}

# Return the newest release version
function Get-NewestReleaseVersion($releaseMap)
{
    return $releaseMap[0].Key
}

# Fail the script if this would result in a backfill
function DisallowBackfill($newestVersion)
{
    $releaseVersion = New-Object System.Version($env:A11yInsightsVersion)

    Write-Host "Comparing release version" $releaseVersion "to newest release" $newestVersion

    if ($newestVersion -gt $releaseVersion)
    {
        Write-Error "Backfill validation FAILED"
    }
    else
    {
        Write-Host "Backfill validation passed"
    }
}

# Initialize the client
function Get-Client()
{
    # Load the octokit dll
    Add-Type -Path ((Get-Location).Path + "\Octokit.$($env:OctokitVersion)\lib\netstandard2.0\Octokit.dll")

    # Get a new client
    $productHeader = [Octokit.ProductHeaderValue]::new("Canary-Pipeline-PreValidation")
    $client = [Octokit.GitHubClient]::new($productHeader)

    return $client
}

# Main program
$client = Get-Client
$releases = $client.Repository.Release.GetAll($Owner, $Repo).Result

Write-Host "Unsorted Releases:" 
$releases | Select-Object -Property Name, TagName | Format-Table

$releaseMap = SortReleases $releases
$newestReleaseVersion = Get-NewestReleaseVersion($releaseMap)
DisallowBackfill $newestReleaseVersion
