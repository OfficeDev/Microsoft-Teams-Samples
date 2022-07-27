# Get Azure SQL's view of our IP address
# We intentially 'fail' a request so that the server will echo back the IP address
# this is done instead of services such as https://api.ipify.org/ since multiple NICs / gateways
# can cause this external ip not to match the client's IP connecting to Azure SQL
function Get-ExternalIpAddress {
    Param (
        [string]$ServerName
    )
    $serverInstance = "${ServerName}.database.windows.net"
    $accessToken=(Get-AzAccessToken -Resource "https://database.windows.net").Token
    $tempFile=New-TemporaryFile
    try {
        Invoke-Sqlcmd -ServerInstance $serverInstance -AccessToken $accessToken -Query 'Select 1' 2>$tempFile | Out-Null
        return $null
    } catch {
        
        $match=(Select-String -Path $tempFile -Pattern "(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})").Matches
        if ($null -eq $match) {
            $errorMessage = Get-Content -Path $tempFile
            throw $errorMessage
        }
        $clientIp= $match.Captures[0]
        return $clientIp
    }
    # There wasn't a firewall rule set up
    return $null
}

function Invoke-AzSql {
    Param (
        [string]$ResourceGroup,
        [string]$ServerName,
        [string]$FirewallRuleName
    )
    $serverInstance = "${ServerName}.database.windows.net"

    # First we get the client's ip address (or null if no firewall rule needed)
    $clientIp = Get-ExternalIpAddress $ServerName
    if ($null -ne $clientIp) {
        New-AzSqlServerFirewallRule `
            -FirewallRuleName $FirewallRuleName `
            -StartIpAddress $clientIp `
            -EndIpAddress  $clientIp `
            -ResourceGroupName $ResourceGroup `
            -ServerName $ServerName | Out-Null
    }
    $accessToken=(Get-AzAccessToken -Resource "https://database.windows.net").Token

    $result = Invoke-Sqlcmd `
        -ServerInstance $serverInstance `
        -AccessToken $accessToken `
        @args

    if ($null -ne $clientIp) {
        Remove-AzSqlServerFirewallRule `
            -FirewallRuleName $FirewallRuleName `
            -ResourceGroupName $ResourceGroup `
            -ServerName $ServerName | Out-Null
    }
    return $result
}

Export-ModuleMember -Function Invoke-AzSql