function Wait-JobsWithProgress($jobs)
{
    $incompleteJobIds = New-Object System.Collections.Generic.HashSet[int]
    foreach ($job in $jobs) {
        $incompleteJobIds.Add($job.Id) | Out-Null
    }
    while ($incompleteJobIds.Count -gt 0)
    {
        $percentComplete = [Math]::Round((($jobs.Count - $incompleteJobIds.Count) / $jobs.Count)*100) 
        Write-Progress -Activity "Wait Jobs" -Status "In progress: [${percentComplete}%]" -PercentComplete $percentComplete
        $completedJob = Wait-Job -Any @($incompleteJobIds)
        $incompleteJobIds.Remove($completedJob.Id) | Out-Null
    }
}

$qBotRgs = Get-AzResourceGroup | Where-Object { $_.ResourceGroupName -match 'QBot-\w{5}$' }

foreach ($rg in $qBotRgs) {
    Write-Host "  - $($rg.ResourceGroupName)"
}

$confirm = $host.UI.PromptForChoice("Proceed?", "" , @("&Yes", "&No"), 0)
if ($confirm -eq 1)
{
    exit
}

$jobs = $qBotRgs | ForEach-Object { 
    Remove-AzResourceGroup `
            -Force `
            -ResourceGroupName $_.ResourceGroupName `
            -AsJob
}

Wait-JobsWithProgress -jobs $jobs