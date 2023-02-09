# This file will run after updating the Production manifest
#
# Environment variables set by the pipeline:
#     GITHUB_TOKEN              is the PAT to access the repo
#     A11yInsightsTagName       is the name of the tag (e.g., v1.1.2227.01) associated with this release
#     OctokitVersion            is the version of OctoKit we've pinned to. A previous task will install this package
#
# Objective: Return error if the release being promote if any of the follwing are true:
#  - The latest release does not exist
#  - The latest release's TagName property does not match A11yInsightsTagName
#  - The latest release's Prerelease property is true
#  - The latest release's Draft property is true
#  - The latest release's Name property does not contain the string "Production Release"
#  - The latest release's Body property does not contain the string "Production Release"
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

# Main program
$productionReleaseMarker = "Production Release"

$client = Get-Client
$latestRelease = $client.Repository.Release.GetLatest($Owner, $Repo).Result

if ($null -eq $latestRelease)
{
    Write-Error "Unable to obtain the latest release!"
    Exit
}

Write-Host "===== Begin Latest Release Dump ====="
$latestRelease
Write-Host "=====  End Latest Release Dump  ====="


if ($latestRelease.TagName -ne $env:A11yInsightsTagName)
{
    Write-Error "Target tag '($($env:A11yInsightsTagName))' does not match latest release tag of '($($latestRelease.TagName))'"
    Exit
}

if ($latestRelease.Prerelease -eq $true)
{
    Write-Error "Target release '$($latestRelease.Name)' must not be marked as prerelease"
    Exit
}

if ($latestRelease.Draft -eq $true)
{
    Write-Error "Target release '$($latestRelease.Name)' must not be marked as draft"
    Exit
}

if (-Not ($latestRelease.Name -Match $ProductionReleaseMarker))
{
    Write-Error "Release Name must contain '$($ProductionReleaseMarker)'"
    Exit
}

if (-Not ($latestRelease.Body -Match $ProductionReleaseMarker))
{
    Write-Error "Release Body must contain '$($ProductionReleaseMarker)'"
    Exit
}

Write-Host "Release Prevalidation Succeeded"
