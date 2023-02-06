
# Common Retry code
class WaitBail : Exception {
    WaitBail($Message) : base($Message) {
    }
}

class MultipleException : Exception {
    $exceptions
    MultipleException($exceptions) : base("Multiple Exceptions") {
        $this.exceptions = $exceptions
    }
}

function Wait-Success([scriptblock]$block, $maxRetries = 10, $baseSleepTime = 10)
{
    $errors = @()
    for ($i = 0; $i -lt $maxRetries; $i++)
    {
        try 
        {
            return Invoke-Command $block -ArgumentList $i
        }
        catch [WaitBail]
        {
            throw $_
        }
        catch
        {
            $errors += $_
            $waitTime = $baseSleepTime * $i
            Write-Verbose "Error: ${_}"
            Write-Verbose "Waiting for ${waitTime} seconds after attempt ${i}"
            Start-Sleep -Seconds $waitTime
            continue;
        }
    }

    # Log errors.
    Write-Verbose "Errors :${errors}"
    throw [MultipleException]::new($errors)
}

# Credit to user gregjhogan on github: https://gist.github.com/gregjhogan/2350eb60d02aa759c9d269c3fc6265b1 
function Get-Nonce {
    param ([int] $length)
    return -join ((48..57) + (97..122) | Get-Random -Count $length | ForEach-Object {[char]$_})
}



Export-ModuleMember -Function Get-Nonce, Start-Step, Wait-Success