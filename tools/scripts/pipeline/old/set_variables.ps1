# This assumes that the task working directory is $(Release.PrimaryArtifactSourceAlias)
$msiFolder = '.\msi'
$a11yInsightsVersion = Get-ChildItem -Name -Directory $($msiFolder)
$a11yInsightsTagName = 'v' + $($a11yInsightsVersion)
$a11yInsightsMsiFile = $($a11yInsightsVersion) + '\' + 'AccessibilityInsights.msi'

# Diagnostic use only--does NOT set environment variables
Write-Host 'Diagnostics values:'
Write-Host ('A11yInsightsVersion = ' + $($a11yInsightsVersion))
Write-Host ('A11yInsightsTagName = ' + $($a11yInsightsTagName))
Write-Host ('A11yInsightsMsiFile= ' + $($a11yInsightsMsiFile))

# Set the environment variables
Write-Host "##vso[task.setvariable variable=A11yInsightsVersion]$($a11yInsightsVersion)"
Write-Host "##vso[task.setvariable variable=A11yInsightsTagName]$($a11yInsightsTagName)"
Write-Host "##vso[task.setvariable variable=A11yInsightsMsiFile]$($a11yInsightsMsiFile)"
