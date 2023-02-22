# This will run early in both Canary and Release stages
#
# Objective: Set values for A11yInsightsTagName and A11yInsightsMinProdVersionTag
#

# This assumes that the task working directory is $(Release.PrimaryArtifactSourceAlias)
Write-Host 
$unsignedManifestFile = Join-Path "manifest" "ReleaseInfo.json"
Write-Host $unsignedManifestFile
$json = Get-Content $unsignedManifestFile | Out-String
$info = $json | ConvertFrom-Json

Write-Host "Unaigned Manifest:"
Write-Host $json

$a11yInsightsTagName = "v" + $info.current_version
$a11yInsightsMinProdVersionTag = "v" + $info.production_minimum_version

# Diagnostic use only--does NOT set environment variables
Write-Host "Diagnostics values:"
Write-Host ("A11yInsightsTagName = " + $($a11yInsightsTagName))
Write-Host ("A11yInsightsMinProdVersionTag = " + $($a11yInsightsMinProdVersionTag))

# Set the environment variables
Write-Host "##vso[task.setvariable variable=A11yInsightsTagName]$($a11yInsightsTagName)"
Write-Host "##vso[task.setvariable variable=A11yInsightsMinProdVersionTag]$($a11yInsightsMinProdVersionTag)"
