
while ($true)
{
    $apps = Get-MgApplication | Where-Object {$_.DisplayName.contains("QBot")}
    Write-Host $apps
    if ($apps.Count -eq 0)
    {
        Write-Host "done"
        break
    }
    $apps | ForEach-Object {
        Remove-MgApplication -ApplicationId $_.Id
    }
}