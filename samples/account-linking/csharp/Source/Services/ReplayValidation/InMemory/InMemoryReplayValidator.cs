namespace Microsoft.Teams.Samples.AccountLinking.ReplayValidation;

/// <summary>
/// The InMemoryReplayValidator is a naive implementation of the <see cref="IReplayValidator" />.
/// 
/// It works by keeping track of the known tokens in an infinitely growing set of ids in local memory. 
/// This shouldn't be used in production.
/// </summary>

public sealed class InMemoryReplayValidator : IReplayValidator
{
    private readonly ILogger<InMemoryReplayValidator> _logger;
    private readonly HashSet<string> seenTokens = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public InMemoryReplayValidator(
        ILogger<InMemoryReplayValidator> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ClaimIdAsync(string jti, DateTimeOffset expiration)
    {
        _logger.LogInformation("Checking if token [{id}] is a replay", jti);
        await _semaphore.WaitAsync();
        // bit of an abuse of the 'try/finally' concept but helps prevent missing `Release`
        try
        {
            if (!this.seenTokens.Contains(jti))
            {
                this.seenTokens.Add(jti);
                return true;
            }
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
