# Constants
$owner = 'Microsoft'
$repositoryName = 'accessibility-insights-windows'
$branchHead = 'heads/Canary'
$releaseVersion = $env:A11yInsightsVersion
$filePath = 'Channels/Canary/release_info.json'
$fileMode = '100644'
$contentTemplate = '{{
  "default": {{
    "current_version": "{0}",
    "minimum_version": "{0}",
    "installer_url": "https://www.github.com/Microsoft/accessibility-insights-windows/releases/download/v{0}/AccessibilityInsights.msi",
    "release_notes_url": "https://github.com/Microsoft/accessibility-insights-windows/releases/tag/v{0}"
  }}
}}'

if ($null -eq $env:GITHUB_TOKEN) {
	throw 'Run this script with a GITHUB_TOKEN environment variable set to a GitHub Personal Access Token with "repo" permissions'
}

# Load the octokit dll
Add-Type -Path ((Get-Location).Path + "\Octokit.$(OctokitVersion)\lib\netstandard2.0\Octokit.dll")

# 1. Get a new client with an appropriate product header value
$productHeader = [Octokit.ProductHeaderValue]::new("AIWindows-CanaryUpdate")
$client = [Octokit.GitHubClient]::new($productHeader)
$client.Credentials = [Octokit.Credentials]::new($env:GITHUB_TOKEN)

# 2. Get the current state of the branch
$branchReference = $client.Git.Reference.Get($owner, $repositoryName, $branchHead).Result;
$latestCommit = $client.Git.Commit.Get($owner, $repositoryName, $branchReference.Object.Sha).Result;
$newTree = [Octokit.NewTree]::new()
$newTree.BaseTree = $latestCommit.Tree.Sha
Write-Host('Initial Sha:' + $latestCommit.Tree.Sha)

# 3. Create the blob with the new content
$content = [String]::Format($contentTemplate, $releaseVersion)
$newBlob = [Octokit.NewBlob]::new()
$newBlob.Content = $content
$newBlob.Encoding = [Octokit.EncodingType]::Utf8
$createdBlobReference = $client.Git.Blob.Create($owner, $repositoryName, $newBlob).Result
Write-Host('Sha of created blob reference: ' + $createdBlobReference.Sha)

# 4. Create a new tree with the blob
$treeItem = [Octokit.NewTreeItem]::new()
$treeItem.Path = $filePath
$treeItem.Mode = $fileMode
$treeItem.Type = [Octokit.TreeType]::Blob
$treeItem.Sha = $createdBlobReference.Sha
$newTree.Tree.Add($treeItem)
$createdTree = $client.Git.Tree.Create($owner, $repositoryName, $newTree).Result
Write-Host('Sha of created tree: ' + $createdTree.Sha)

# 5. Create the commit with the SHAs of the tree and the branch reference
$comment = [String]::Format('Releasing v{0} to Canary', $releaseVersion)
$newCommit = [Octokit.NewCommit]::new($comment, $createdTree.Sha, $branchReference.Object.Sha)
$createdCommit = $client.Git.Commit.Create($owner, $repositoryName, $newCommit).Result
Write-Host('Sha of created commit: ' + $createdCommit.Sha)

# 6. Update the branch head with the SHA of the commit
$referenceUpdate = [Octokit.ReferenceUpdate]::new($createdCommit.Sha)
$update = $client.Git.Reference.Update($owner, $repositoryName, $branchHead, $referenceUpdate.Sha).Result
Write-Host('Url of update: ' + $update.Url)

Write-Host $($update.Result.Url)