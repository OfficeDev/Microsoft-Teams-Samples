// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.AgentSamples.Models;

/// <summary>
/// Model for channel deep link data.
/// </summary>
public class DeepLinkChannelModel
{
    public string? LinkUrl { get; set; }
    public int Id { get; set; }
    public string? LinkTitle { get; set; }
}