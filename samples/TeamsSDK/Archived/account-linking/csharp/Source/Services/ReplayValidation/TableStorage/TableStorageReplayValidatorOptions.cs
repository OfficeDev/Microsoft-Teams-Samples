using System.ComponentModel.DataAnnotations;

namespace Microsoft.Teams.Samples.AccountLinking.ReplayValidation;

/// <summary>
/// Options for the <see cref="TableStorageReplayValidator" />
/// </summary>
public sealed class TableStorageReplayValidatorOptions
{
    /// <summary>
    /// The prefix to use for the id buckets.
    /// 
    /// E.G. the prefix 'StateReplay' will generate tables like 'StateReplay1647561600'
    /// </summary>
    /// <remarks>
    /// It is important that this prefix is unique to the replay validator as it will periodically delete
    /// tables that it identifies with this prefix that it deems 'old'. 
    /// </remarks>

    [Required(AllowEmptyStrings = false)]
    public string TablePrefix { get; set; } = string.Empty;

    /// <summary>
    /// The Azure Tables endpoint.
    /// E.G. https://examplestorageaccount.table.core.windows.net
    /// </summary>

    [Required(AllowEmptyStrings = false)]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// The duration of time that the storage replay validator should bucket together token ids. 
    /// This results in a trade-off between growth of the table (persistance cost) and the cost to 
    /// scan & delete old tables. 
    /// </summary>
    /// <remarks>
    /// At this time, longer bucket sizes don't improve costs as the implementation will attempt to delete
    /// all old tables on every get/set operation.
    /// </remarks>
    public TimeSpan BucketSize { get; set; } = TimeSpan.FromDays(1);
}
