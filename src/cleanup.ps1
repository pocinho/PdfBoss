### cleanup.ps1 ###

# root path:
$rootProjectPath = ".\"

# file and directory name patterns that are safe to delete:
$fileNamePattern = @("bin", "obj", ".vs", "*.user", "*publish*", "debug", "release", "*.bak", "*.old")

# do not delete nuget downloads:
$excludePattern = @("*\packages\*")

Get-ChildItem $rootProjectPath -Recurse -Force -Include $fileNamePattern | Where {$_.FullName -NotLike $excludePattern} | Remove-Item -Recurse -Force -WhatIf

### cleanup.ps1 ###