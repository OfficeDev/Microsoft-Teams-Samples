function New-QBotKeyVaultCert($VaultName, $Name, $HostName, $ValidMonths = 36)
{
    $policy = New-AzKeyVaultCertificatePolicy `
        -SecretContentType "application/x-pkcs12" `
        -SubjectName "CN=$HostName" `
        -IssuerName "Self" `
        -ValidityInMonths $ValidMonths `
        -ReuseKeyOnRenewal

    # Add the certificate
    # NB: this is an async operation & this only returns a operation object, not the cert
    Add-AzKeyVaultCertificate `
        -VaultName $VaultName `
        -Name $Name `
        -CertificatePolicy $policy | Out-Null

    # Poll for the cert creation to finish
    Wait-Success {
        $operation = Get-AzKeyVaultCertificateOperation `
            -VaultName $VaultName `
            -Name $Name
        if ($operation.Status -eq 'completed')
        {
            return $operation
        }
        else 
        {
            throw $operation.Status 
        }
    } | Out-Null

    $azCertificate = Get-AzKeyVaultCertificate `
        -VaultName $VaultName `
        -Name $Name

    if ($null -eq $azCertificate.Certificate)
    {
        throw "Certificate returned from keyvault was null"
    }

    return $azCertificate.Certificate
}
