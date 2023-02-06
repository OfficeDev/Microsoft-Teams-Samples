param (
    $OutFile
)

function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    $name = [System.IO.Path]::GetRandomFileName()
    New-Item -ItemType Directory -Path (Join-Path $parent $name)
}

$tmpDir = New-TemporaryDirectory
Write-Host $tmpDir

Copy-Item -Recurse -Path "$PSScriptRoot/../Deployment/*" -Destination $tmpDir
Copy-Item -Recurse -Path "$PSScriptRoot/../Manifest/" -Destination $tmpDir

Compress-Archive -Path "$tmpDir/*" -DestinationPath $OutFile