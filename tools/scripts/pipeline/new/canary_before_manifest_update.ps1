# This will run before updating the Canary manifest
#
# Environment variables set by the pipeline:
#     GITHUB_TOKEN              is the PAT to access the repo
#     A11yInsightsVersion       is the version (e.g., 1.1.2227.01) begin published by the pipeline
#     A11yInsightsTagName       is the name of the tag (e.g., v1.1.2227.01) associated with this release
#     OctokitVersion            is the version of OctoKit we've pinned to. A previous task will install this package
#     DeleteReleases            is true if cleanup should actually delete (if false, just report what would be cleaned up)
#
# Objective: Return error if a more recent release (i.e., a higher version) already exists
#

# Extracts a version from the release name.
function Get-ReleaseVersion($specificRelease)
{
    $versionString = $specificRelease.Name
    if($versionString -match ".*v(\d+\.\d+\.\d+\.\d+).*"){
        return $matches[1]
    } else {
        $deleteList += $specificRelease
        return [string]::Empty
    }
}

# Sorts the releases based on the version number
function SortReleases($releases)
{
    $releaseMap = @{}
    foreach($release in $releases.Result){
        $releaseVersion = [System.Version](Get-ReleaseVersion $release)
        if(-not [string]::IsNullOrEmpty($releaseVersion)){
            $releaseMap.Add($releaseVersion,$release)
        }
    }

    $releaseMap = $releaseMap.GetEnumerator() | Sort-Object -Descending -Property Name

    Write-ReleaseMap $releaseMap

    return $releaseMap
}

# Dump the sorted releases for debugging
function Write-ReleaseMap($releaseMap)
{
    Write-Host "==================================="
    Write-Host "Sorted Releases"
    Write-Host "Version       Id          Name"
    Write-Host "-------       --          ----"
    foreach($releaseKV in $releaseMap){
        Write-Host $releaseKV.Key '  ' $releaseKV.Value.Id '  ' $releaseKV.Value.Name
    }
    Write-Host "==================================="
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

    Write-Host 'Comparing release version' $releaseVersion 'to newest release' $newestVersion

    if ($newestVersion -gt $releaseVersion) {
        Write-Error 'Backfill validation FAILED'
    } else {
        Write-Host 'Backfill validation passed'
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

    # Add credentials for authentication
    $client.Credentials = [Octokit.Credentials]::new($env:GITHUB_TOKEN)
    return $client
}

# Main program
$client = Get-Client
$repoId = $client.Repository.Get("Microsoft", "accessibility-insights-windows").Result.Id;
$releases = $client.Repository.Release.GetAll($repoId)

Write-Host "Unsorted Releases:" 
$releases.Result | Select-Object -Property Name, TagName, Id | Format-Table

$releaseMap = SortReleases $releases
$newestReleaseVersion = Get-NewestReleaseVersion($releaseMap)
DisallowBackfill $newestReleaseVersion
