param (
    $Company_Name,
    $Website_Url,
    $Privacy_Url,
    $Terms_Of_Use_Url,
    $Host_Name,
    $Graph_App_Id,
    $Bot_App_Id,
    $OutFile
)

function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    $name = [System.IO.Path]::GetRandomFileName()
    New-Item -ItemType Directory -Path (Join-Path $parent $name)
}

function New-QBotManifest(
    [string]$Company_Name,
    [string]$Website_Url,
    [string]$Privacy_Url,
    [string]$Terms_Of_Use_Url,
    [string]$Host_Name,
    [string]$Graph_App_Id,
    [string]$Bot_App_Id,
    [string]$OutFile
) 
{
    $parametersJson = @{
        Company_Name = $Company_Name;
        Website_Url = $Website_Url;
        Privacy_Url = $Privacy_Url;
        Terms_Of_Use_Url = $Terms_Of_Use_Url;
        Host_Name = $Host_Name;
        Graph_App_Id = $Graph_App_Id;
        Bot_App_Id = $Bot_App_Id;
        Resource_Uri = "api://$Host_Name/$Graph_App_Id";
    } | ConvertTo-Json
    $manifestDirectory = "$PSScriptRoot\..\..\Manifest\"
    $tempDirectory = New-TemporaryDirectory
    
    ConvertTo-PoshstacheTemplate `
        -InputFile (Join-Path $manifestDirectory "manifest.json") `
        -ParametersObject $parametersJson `
        | Out-File (Join-Path $tempDirectory 'manifest.json') `
        | Out-Null
    
    Copy-Item (Join-Path $manifestDirectory "color.png") (Join-Path $tempDirectory "color.png")
    Copy-Item (Join-Path $manifestDirectory "outline.png") (Join-Path $tempDirectory "outline.png")
    Compress-Archive -Path (Join-Path $tempDirectory "/*") -DestinationPath $OutFile | Out-Null
}
