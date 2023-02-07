# This file will run after updating the Production manifest
#
# Environment variables set by the pipeline:
#     GITHUB_TOKEN              is the PAT to access the repo
#     A11yInsightsVersion       is the version (e.g., 1.1.2227.01) begin published by the pipeline
#     A11yInsightsTagName       is the name of the tag (e.g., v1.1.2227.01) associated with this release
#     OctokitVersion            is the version of OctoKit we've pinned to. A previous task will install this package
#     DeleteReleases            is true if cleanup should actually delete (if false, just report what would be cleaned up)
#
# Objective: Return error if the release being promoted fails any of the following checks:
#  - The release does not exist
#  - The release is marked as draft
#  - The release is not marked as latest
#  - The release title includes the word "Canary"
#
