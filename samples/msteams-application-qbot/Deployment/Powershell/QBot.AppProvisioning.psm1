
$GRAPH_APP_ID='00000003-0000-0000-c000-000000000000'
$GraphServicePrincipal = $null
function Get-GraphServicePrincipal {
    if ($null -eq $GraphServicePrincipal)
    {
        $GraphServicePrincipal = Get-MgServicePrincipal -All -Filter "AppId eq '$GRAPH_APP_ID'"
    }
    return $GraphServicePrincipal;
}

function Get-GraphPermission([string]$Permission, [bool]$AppContext)
{
    $graphSp = Get-GraphServicePrincipal
    $permissions = if ($AppContext) {$graphSp.AppRoles} else {$graphSp.Oauth2Permissions}

    $filteredPermissions = @($permissions | Where-Object {$_.Value -eq $Permission})
    if ($null -eq $filteredPermissions) {
        return $null
    } 
    $permissionId= $filteredPermissions[0].Id;
    $resourceAccess = @{
        Id=$permissionId;
        Type= if ($AppContext) {'Role'} else {'Scope'}
    }
    return $resourceAccess
}

# https://github.com/MicrosoftDocs/azure-docs/issues/56214
function Get-TeamsTabSSOPreAuthorizedApplications([string] $PermissionId)
{
    return @(
        @{
            AppId = '1fec8e78-bce4-4aaf-ab1b-5451cc387264';
            DelegatedPermissionIds=@($PermissionId);
        },
        @{
            AppId = '5e3ce6c0-2b1f-4285-8d4b-75ee78787346';
            DelegatedPermissionIds=@($PermissionId);
        }
    )

    return @($webClientAuthorization, $desktopClientAuthorization)
}

function Get-TeamsTabSSOApiApplication
{
    $oauth2PermissionScope = @{
        adminConsentDescription = "Access as User";
        adminConsentDisplayName = "Access as User";
        id                      = New-Guid;
        isEnabled               = $true;
        type                    = "User";
        userConsentDescription  = "Access as User";
        userConsentDisplayName  = "Access as User";
        value                   = "access_as_user";
    }
    $preAuthorizedApplications = Get-TeamsTabSSOPreAuthorizedApplications -PermissionId $oauth2PermissionScope.Id
    $apiApplication = @{
        oauth2PermissionScopes=@($oauth2PermissionScope);
        preAuthorizedApplications=@($preAuthorizedApplications);
    }
    return $apiApplication
}

function Get-TeamsTabSSOIdentifierUri($appId, $domain)
{
    return "api://$domain/$appId"
}

function Get-GraphRequiredResourceAccess {
    param (
        [string[]]
        $DelegatedPermissions = @(),
        [string[]]
        $ApplicationPermissions = @()
    )
    $oauthPermissions = @($DelegatedPermissions | % { Get-GraphPermission -Permission $_ -AppContext $false })
    $roles = @($ApplicationPermissions | % { Get-GraphPermission -Permission $_ -AppContext $true })
    $resourceAccess = $oauthPermissions + $roles

    $requiredResourceAccess = @{
        ResourceAppId = $GRAPH_APP_ID;
        ResourceAccess = $resourceAccess;
    }
    return $requiredResourceAccess
}

function ConvertTo-KeyCredential($certificate) {
    return @{
        type = "AsymmetricX509Cert";
        customKeyIdentifier = $certificate.GetCertHash();
        keyId = (New-Guid).ToString();
        key =  $certificate.GetRawCertData(); #$base64Value;
        startDateTime = $certificate.NotBefore.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        endDateTime = $certificate.NotAfter.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        usage = "Verify"
    }
}

# Strange workaround due to the fact that the Update-MgApplication cmdlet re-formats the date
# strings into non ISO 8061 format
function Set-ApplicationKeyCredential($applicationId, $certificate)
{
    $keyCredential = ConvertTo-KeyCredential $certificate
    Invoke-MgGraphRequest `
        -Method PATCH `
        -Uri "https://graph.microsoft.com/v1.0/applications/${applicationId}" `
        -Body @{keyCredentials=@($keyCredential)}
}