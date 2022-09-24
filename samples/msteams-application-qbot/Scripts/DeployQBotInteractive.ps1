[CmdletBinding()]
param(
    $artifactsBaseUri = ""
)

# Helper functions for bootstrapping
function Import-ModuleGlobal {
    [CmdletBinding()]
    param (
        [string] $ModuleName,
        [string] $ModuleVersion
    )
    Import-Module $ModuleName -MinimumVersion $ModuleVersion -ErrorAction SilentlyContinue -ErrorVariable ImportModuleError
    if ($ImportModuleError) {
        Write-Verbose -Message "Checking online gallery for module $ModuleName ($ModuleVersion)"
        if (Find-Module -Name $ModuleName -RequiredVersion $ModuleVersion -ErrorAction SilentlyContinue) {
            Write-Verbose -Message "Installing module from gallery"
            Install-Module -Name $ModuleName -RequiredVersion $ModuleVersion -Force -Scope CurrentUser -AllowClobber -ErrorAction Continue -ErrorVariable InstallModuleError
            if ($InstallModuleError) {
                throw $InstallModuleError
            }
            return Import-Module $ModuleName -MinimumVersion $ModuleVersion -ErrorAction Stop
        }
        throw "Module $ModuleName ($ModuleVersion) not imported, not available and not in online gallery, exiting."
    }
}

function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    $name = [System.IO.Path]::GetRandomFileName()
    New-Item -ItemType Directory -Path (Join-Path $parent $name)
}

# Get the current working directory
$currentWorkingDirectory = Get-Location

$importModulePath = "$($currentWorkingDirectory)/../Deployment/Powershell/QBot.psd1"

# Next get the list of dependencies and install them
$qbotPsManifest=Import-PowerShellDataFile -Path $importModulePath

Clear-Host

# Install Modules
Write-Host "The following modules will be installed (if not already):"

$qbotPsManifest.RequiredModules | ForEach-Object { 
    Write-Host "  -" -NoNewline
    Write-Host $_.ModuleName
}

$confirm = $host.UI.PromptForChoice("Proceed?", "" , @("&Yes", "&No"), 0)
if ($confirm -eq 1) 
{
    exit
}

$qbotPsManifest.RequiredModules | ForEach-Object {
    Import-ModuleGlobal $_.ModuleName $_.ModuleVersion -ErrorAction Stop
}

Import-Module $importModulePath

Clear-Host

# Login to M365 account.
Write-Host "Please log in with your M365 credentials if prompted"
Connect-MgGraph -Scopes 'Application.ReadWrite.All','User.Read'

Clear-Host

# Login to Azure account for deployment.
Write-Host "Please log in with your Azure credentials"
Connect-AzAccount 3>$null

# Choose a subscription
$azSubscriptions = Get-AzSubscription 3>$null | Sort-Object -Property Name
if ($azSubscriptions.Count -gt 1)
{
    Write-Host "Please pick an Azure subscription to deploy to:"
    $azSubscriptions | ForEach-Object { 
        Write-Host "  - " -NoNewline
        Write-Host $_.Name
    }
    $azSubscriptionNames = $azSubscriptions | ForEach-Object {$_.Name}
    $defaultSubscription = $azSubscriptionNames[0]
    $subscription = $null 
    while ($true)
    {
        $subscription = Read-Host "Select Azure subscription [Default: ${defaultSubscription}]"
        if ($null -eq $subscription)
        {
            $subscription = $defaultSubscription
        }

        if ($azSubscriptionNames -contains $subscription)
        {
            break
        }
    }
    
    Set-AzContext $subscription 3>$null | Out-Null
}

# Choose region
$locationList=(Get-AzLocation | ForEach-Object {$_.Location}| Sort-Object)
$locationIndex = $host.UI.PromptForChoice("Deploy to which region?", "" , $locationList, 0)
$location = $locationList[$locationIndex]

$searchLocationList = (Get-AzLocation | Where-Object {$_.Providers -contains 'Microsoft.Search'}| ForEach-Object {$_.Location}| Sort-Object)
$searchLocation = $location
if ($searchLocationList -notcontains $searchLocation)
{
    Write-Host "Search not supported in that region, where would you like to deploy the search service"
    $locationIndex = $host.UI.PromptForChoice("Deploy to which region?", "" , $searchLocationList, 0)
    $searchLocation = $searchLocationList[$locationIndex]
}

# Organization's information to create manifest.
$websiteUrl = Read-Host "Enter the url for your website [Default: https://www.contoso.com]"
if ([String]::IsNullOrWhiteSpace($websiteUrl))
{
    $websiteUrl = "https://www.contoso.com"
}

$privacyPolicyUrl = Read-Host "Enter the url for your website [Default: https://www.contoso.com/privacy]"
if ([String]::IsNullOrWhiteSpace($privacyPolicyUrl))
{
    $privacyPolicyUrl = "https://www.contoso.com/privacy"
}

$termsOfServiceUrl = Read-Host "Enter the url for your terms of service [Default: https://www.contoso.com/tos]"
if ([String]::IsNullOrWhiteSpace($termsOfServiceUrl))
{
    $termsOfServiceUrl = "https://www.contoso.com/tos"
}

$organization = Get-MgOrganization
$defaultOrganizationName = ($organization).DisplayName
$organizationName = Read-Host "Enter your organization name: [Default: ${defaultOrganizationName}]"
if ([String]::IsNullOrWhiteSpace($organizationName))
{
    $organizationName = $defaultOrganizationName
}

# Generate resource group name
if ([String]::IsNullOrWhiteSpace($ResourceGroupName))
{
    $nonce = Get-Nonce 5
    $ResourceGroupName = "QBot-${nonce}"
}

Write-Host "Please wait while we deploy QBot, this may take up to 15 minutes"

$logfileLocation = "${currentWorkingDirectory}/deployment.log"
Write-Host "Deployment logs will be written to: ${logFileLocation}"

$oldVerbosePreference = $VerbosePreference
$VerbosePreference = "Continue"

# Start Deployment
$($result = New-QBotDeployment -verbose `
    -resourceGroupName $ResourceGroupName `
    -websiteUrl $websiteUrl `
    -companyName $organizationName `
    -privacyUrl $privacyPolicyUrl `
    -termsOfUseUrl $termsOfServiceUrl `
    -location $location `
    -searchLocation $searchLocation) *>> $logfileLocation

$VerbosePreference = $oldVerbosePreference
$result | Out-Host

# Admin consent.
$adminConsentURL="https://login.microsoftonline.com/$($organization.Id)/adminconsent?client_id=$($result.GraphAppId)"
Write-Output "Please grant consent to the application with your M365 admin credentials"
Write-Output "AAD admin consent page: $adminConsentURL" >> $logfileLocation
Start-Process $adminConsentURL

# Sign-out of the M365 account.
Disconnect-MgGraph

# Application Package
Move-Item -Path $result.ManifestLocation -Destination $currentWorkingDirectory
Write-Host "Deployment completed, please upload the manifest.zip file in the current directory to your organization's app catalog"