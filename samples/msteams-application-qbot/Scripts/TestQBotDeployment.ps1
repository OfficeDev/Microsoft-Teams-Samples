$azContext = Get-AzContext

$accessToken = (Get-AzAccessToken).Token
$accountId = $azContext.Account.Id
$subscriptionName = $azContext.Subscription.Name

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

$jobs = 1..2 | ForEach-Object {
    Start-Job -ScriptBlock {
        param ($azToken, $accountId, $subscriptionName)
        $manifestId = (New-Guid).Guid
        New-Item -ItemType 'Directory' -Path "./runs/${manifestId}/"
        $downloadLocation = "../Deployment/Powershell/QBot.psd1"
        $qbotPsManifest=Import-PowerShellDataFile -Path $downloadLocation
        foreach ($dependency in $qbotPsManifest.RequiredModules) {
            Import-Module $dependency
        }
        Import-Module $downloadLocation

        Connect-MgGraph -Scopes 'Application.ReadWrite.All','User.Read'
        Connect-AzAccount -Accesstoken $azToken -
        $VerbosePreference='continue'
        $nonce = Get-Nonce 5
        
        New-QBotDeployment `
            -resourceGroupName "QBot-${nonce}" `
            -manifestLocation "./runs/${manifestId}/manifest-${nonce}.zip" `
            -manifestId $manifestId `
            *>> "./runs/${manifestId}/deploy-${nonce}.log"
    } -ArgumentList @($accessToken, $accountId, $subscriptionName)
}

Wait-JobsWithProgress $jobs
