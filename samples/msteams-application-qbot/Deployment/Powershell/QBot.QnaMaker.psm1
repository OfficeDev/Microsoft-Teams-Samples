function Wait-QnaMakerOperation($endpoint, $key, $operationId, $maxRetries = 10, $sleepTimeSeconds = 10) {
    Write-Verbose "Wait-QnaMakerOperation: endpoint:[${endpoint}] key:[${key}] operationId: [${operationId}]"
    $cmd = {
        $headers = @{}
        $headers['Ocp-Apim-Subscription-Key'] = $key
        $uri = "${endpoint}qnaMaker/v4.0/operations/${operationId}"
        $response = Invoke-WebRequest `
                -Uri $uri `
                -Method GET `
                -Headers $headers
        $response = $response.Content | ConvertFrom-Json
        $status = $response.operationState
        Write-Verbose "Wait Operation current status: [${status}]"
        if ($response.operationState -eq 'Succeeded') 
        {
            return $response
        }
        elseif ($response.operationState -eq 'Failed') 
        {
            throw [WaitBail]::new((ConvertTo-Json $response.errorResponse))
        }
        else
        {
            # We want to "fail" on the other statuses so we can continue. 
            throw $response
        }
    }
    Wait-Success $cmd $maxRetries $sleepTimeSeconds
}

function New-QnAMakerKB($key, $endpoint, $name) {
    Write-Verbose "New-QnAMakerKb: ${key}, ${endpoint}, ${name}"

    $headers = @{}
    $questions = "Who are you?", "What are you?"
    $body = (ConvertTo-Json -Depth 20 @{name = $name; qnaList = @(@{id = 0; questions = $questions; answer = "A bot for answering your questions" }) })
    
    $headers['Ocp-Apim-Subscription-Key'] = $key
    $uri = "${endpoint}qnaMaker/v4.0/knowledgebases/create"
    return Wait-Success {
        $createOperation = Invoke-WebRequest $uri -Method POST -Headers $headers -ContentType 'application/json' -Body $body

        if ($null -eq $createOperation) {
            throw "Failed to start KB creation"
        }
        Write-Verbose "Create Result: ${createOperation}"
        $createResult = ConvertFrom-Json $createOperation
        $op = Wait-QnaMakerOperation `
            -endpoint $endpoint `
            -key $key `
            -operationId $createResult.operationId
        Write-Verbose "Wait result: ${op}"
        $resourceLocation = $op.resourceLocation
        Write-Verbose "Getting kb object: ${resourceLocation}"
        $uri = "${endpoint}qnaMaker/v4.0${resourceLocation}"
        $response = Invoke-WebRequest `
            -Uri $uri `
            -Method GET `
            -Headers $headers `
            -ContentType 'application/json'
        $kb = $response.Content | ConvertFrom-Json
        $response = Invoke-WebRequest `
            -Uri $uri `
            -Method POST `
            -Headers $headers
        return $kb
    } -baseSleepTime 150
}

function Get-QnaMakerEndPointKeys($key, $endpoint) {
    Write-Verbose "Get-QnaMakerEndPointKeys ${key}, ${endpoint}"
    
    $response = Wait-Success {
        $headers = @{
            "Ocp-Apim-Subscription-Key" = $key
        }
        $uri = "${endpoint}qnamaker/v4.0/endpointkeys"
        Invoke-WebRequest $uri -Headers $headers
    } -baseSleepTime 150

    return ($response.Content | ConvertFrom-Json)
}