function New-QBotDeployment
{
    [CmdletBinding()]
    param (
        $resourceGroupName,
        $name = "QBot",
        $companyName = "Contoso",
        $websiteUrl = "https://contoso.com/",
        $privacyUrl = "https://contoso.com/privacy",
        $termsOfUseUrl = "https://contoso.com/terms-of-use",
        $location = "westus2",
        $searchLocation = "westus2",
        $manifestId = "97c66967-8416-4660-a22d-168a02fa5633",
        $manifestLocation = "./manifest.zip"
    )

    # Create the resource group
    Write-Verbose "Creating Resource Group"
    New-AzResourceGroup -Name $resourceGroupName -Location $location | Out-Null

    # Deploy the layer 1 arm template
    # This is broken into two stages since there are 3 resources which need manual post-processing 
    # but generate artifacts that the running application requires
    # 
    # 1) The certificates in the keyvault need to be generated
    # 2) The schema, roles & user need to be provisionined in the SQL database
    # 3) The knowledgebase needs to be created
    # 
    # additionally, the bot cannot be created w/o the AAD application registration which depends on a 
    # certificate generated in '1'
    Write-Verbose "Deploying layer 1 arm template"
    $azureContext = Get-AzContext
    $azureAccount = $azureContext.Account
    $homeAccount = $azureAccount.ExtendedProperties.HomeAccountId.Split('.')

    $armParameters = @{
        TemplateFile = "$PSScriptRoot/../arm/azuredeploy-layer1.json"
        ResourceGroupName = $resourceGroupName;
        baseResourceName = $resourceGroupName;
        location = $location;
        qnaMakerLocation = $searchLocation;
        sqlAdminUserMail = $azureAccount.Id;
        sqlAdminUserId = $homeAccount[0];
        sqlAdminTenantId = $homeAccount[1];
    }
    $armParameters | Out-Host
    Write-Verbose $armParameters.sqlAdminUserMail
    $deployment1 = New-AzResourceGroupDeployment @armParameters
    Write-Host $deployment1
    if($deployment1.ProvisioningState -eq 'Failed' -or $null -eq $deployment1)
    {
        throw "Failed deploying layer 1 arm template"
    }
    $hostname =  $deployment1.Outputs.hostname.Value


    # Create the certificates in the keyvault for both the graph & bot applications
    Write-Verbose "Generating Graph Application Certificate"
    $graphAppCert = New-QBotKeyVaultCert `
            -VaultName $deployment1.Outputs.keyVaultName.Value `
            -Name "GraphApp" `
            -HostName $hostname

    Write-Verbose "Generating Bot Application Certificate"
    $botAppCert = New-QBotKeyVaultCert `
            -VaultName $deployment1.Outputs.keyVaultName.Value `
            -Name "BotApp" `
            -HostName $hostname
    

    # Create the application registrations in the M365 tenant for the bot & graph applications
    Write-Verbose "Create Bot App Registration" 
    $botApp = New-MgApplication `
            -DisplayName "$Name [Bot]" `
            -SignInAudience "AzureADMultipleOrgs"
    Set-ApplicationKeyCredential $botApp.id $botAppCert
    
    $graphAppApi = Get-TeamsTabSSOApiApplication
    $graphAppApi.RequestedAccessTokenVersion = 2 # use version 2 tokens
    $graphAppResources = Get-GraphRequiredResourceAccess `
        -ApplicationPermissions @('TeamsActivity.Send', 'User.Read.All')

    $graphApp = New-MgApplication `
        -DisplayName "$Name [Graph]" `
        -SignInAudience "AzureADMyOrg" `
        -RequiredResourceAccess $graphAppResources
    
    # Get the SSO uri for the graph app id
    $graphAppIdentifierUri = Get-TeamsTabSSOIdentifierUri `
        -AppId $graphApp.AppId `
        -Domain $hostname
    # Set the SSO parameters for the graph app
    Update-MgApplication `
        -ApplicationId $graphApp.Id `
        -IdentifierUris @($graphAppIdentifierUri)

    Update-MgApplication `
        -ApplicationId $graphApp.Id `
        -Api @{
            Oauth2PermissionScopes = $graphAppApi.Oauth2PermissionScopes;
            RequestedAccessTokenVersion = $graphAppApi.RequestedAccessTokenVersion;
        }
    Update-MgApplication `
        -ApplicationId $graphApp.Id `
        -Api $graphAppApi

    Set-ApplicationKeyCredential $graphApp.Id $graphAppCert

    
    # Create the knowledge base in the QnAMaker & get the endpoint key(s)
    Write-Verbose "Creating knowledge base"
    $qnaMakerEndpoint = $deployment1.Outputs.qnaMakerEndpoint.Value
    $qnaMakerKey = $deployment1.Outputs.qnaMakerPrimaryKey.Value
    $kb = New-QnaMakerKB -name 'knowledgebase' -key $qnaMakerKey -endpoint $qnaMakerEndpoint
    $kbKeys = Get-QnaMakerEndpointKeys -key $qnaMakerKey -endpoint $qnaMakerEndpoint


    # Setup the user, role & schema for the database
    Write-Verbose "Configuring database"
    $DatabaseName = "qbot"
    $DatabaseServerName = $deployment1.Outputs.databaseName.Value
    
    Invoke-Azsql `
        -ResourceGroup $resourceGroupName `
        -Server $DatabaseServerName `
        -Database $DatabaseName `
        -InputFile "${PSScriptRoot}/../sql/CreateQBotRole.sql" `
        -FirewallRuleName "TEMP RULE FOR PROVISIONING PLEASE DELETE" `
        -Variable @("databaseName=qbot", "qbot_user_role=qbot_user")
    
    # Create the User
    $qbotPrincipal = $deployment1.Outputs.qbotPrincipalName.Value
    Invoke-Azsql `
        -ResourceGroup $resourceGroupName `
        -Server $DatabaseServerName `
        -Database $DatabaseName `
        -InputFile "${PSScriptRoot}/../sql/AddAdSqlUser.sql" `
        -FirewallRuleName "TEMP RULE FOR PROVISIONING PLEASE DELETE" `
        -Variable @("user_upn=${qbotPrincipal}")
    
    # Add the principal to the Role
    Invoke-Azsql `
        -ResourceGroup $resourceGroupName `
        -Server $DatabaseServerName `
        -Database $DatabaseName `
        -FirewallRuleName "TEMP RULE FOR PROVISIONING PLEASE DELETE" `
        -Query "EXEC sp_addrolemember 'qbot_user', '${qbotPrincipal}'"
    
    # Create the schema
    Invoke-Azsql `
        -ResourceGroup $resourceGroupName `
        -Server $DatabaseServerName `
        -Database $DatabaseName `
        -FirewallRuleName "TEMP RULE FOR PROVISIONING PLEASE DELETE" `
        -InputFile "${PSScriptRoot}/../sql/Migrations.sql"
    
    # Grant access to the tables
    $generateGrantsSql = Invoke-Azsql `
        -ResourceGroup $resourceGroupName `
        -Server $DatabaseServerName `
        -Database $DatabaseName `
        -FirewallRuleName "TEMP RULE FOR PROVISIONING PLEASE DELETE" `
        -InputFile "${PSScriptRoot}/../sql/GenerateGrantsSql.sql"
    Invoke-Azsql `
        -ResourceGroup $resourceGroupName `
        -Server $DatabaseServerName `
        -Database $DatabaseName `
        -FirewallRuleName "TEMP RULE FOR PROVISIONING PLEASE DELETE" `
        -Query $generateGrantsSql[0]


    # Now deploy the configurations
    Write-Verbose "Deploy layer 2 ARM template"

    # Put this in a retry wrapper since the zipdeploy seems to struggle occasionally
    Wait-Success {
        $keyVaultName = $deployment1.Outputs.keyVaultName.Value
        $m365tenantId = (Get-MgContext).TenantId
        $armParameters = @{
            ResourceGroupName = $resourceGroupName
            TemplateFile = "$PSScriptRoot/../arm/azuredeploy-layer2.json"
            #variables for the template
            location = $location;
            baseResourceName = $resourceGroupName;
            botAppId = $botApp.AppId
            botDisplayName = "QBot"
            botHost = "https://${hostname}"
            botCertName = "BotApp"
            manifestId = $manifestId
    
            "QnAMaker:Endpoint" = $qnaMakerEndpoint 
            "QnAMaker:EndpointKey" = $kbKeys.primaryEndpointKey
            "QnAMaker:KnowledgeBaseId" = $kb.Id 
            "QnAMaker:RuntimeEndpoint" =  "https://$($deployment1.Outputs.qnaMakerRuntimeEndpoint.Value)"
            "QnAMaker:ScoreThreshold" = "50" 
            "QnAMaker:SubscriptionKey" = $qnaMakerKey
            "keyvaultUrl" = "https://${keyVaultName}.vault.azure.net/"
            "graphAppId" = $graphApp.AppId 
            "graphAppTenantId" = $m365tenantId 
            "graphAppCertName" = "GraphApp"
            databaseServer = "${DatabaseServerName}.database.windows.net"
        }
        $deployment2 = New-AzResourceGroupDeployment @armParameters
        if ($deployment2.ProvisioningState -eq 'Failed')
        {
            # we want to retry since the zipdeploy runs into 'conflict' errors seemingly at random.
            throw "deployment failed"
        }
    }

    # Get the current working directory
    $currentWorkingDirectory = Get-Location

    # Build and zip the project to deploy.
    Write-Verbose "Building the web project."
    $webProjectDirectory = "$($currentWorkingDirectory)/../Source/Microsoft.Teams.Apps.QBot.Web"
    Set-Location -Path $webProjectDirectory
    dotnet publish --configuration Release --output .\bin\publish --verbosity quiet

    Write-Verbose "Preparing qbot.zip."
    Compress-Archive .\bin\publish\* .\bin\qbot.zip -Force

    # Deploy zip to app service
    Write-Verbose "Deploying zip to app service."
    $zipFilePath = "$($webProjectDirectory)/bin/qbot.zip"
    Publish-AzWebApp -ResourceGroupName $resourceGroupName -Name "$($resourceGroupName)-webapp" -ArchivePath $zipFilePath

    Set-Location -Path $currentWorkingDirectory

    # Now create the manifest zip file for upload to the catalog for the generated bot
    Write-Verbose "Generating Manifest"
    New-QbotManifest `
        -Company_Name $companyName `
        -Website_Url $websiteUrl `
        -Privacy_Url $privacyUrl `
        -Terms_Of_Use_Url $termsOfUseUrl `
        -Host_Name $hostname `
        -Graph_App_Id $graphApp.AppId `
        -Bot_App_Id $botApp.AppId `
        -OutFile $manifestLocation

    return @{
        ResourceGroupName = $resourceGroupName
        ManifestLocation = $manifestLocation
        GraphAppId = $graphApp.AppId
        BotAppId = $botApp.AppId
        Host = $hostname
    }
}


