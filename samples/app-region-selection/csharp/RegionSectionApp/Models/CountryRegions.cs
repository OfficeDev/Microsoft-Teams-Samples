// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples.Models;

public class RootObject
{
    public RegionDomain[]? RegionDomains { get; set; }
}

public class RegionDomain
{
    public int Id { get; set; }
    
    public string Region { get; set; } = string.Empty;
    
    public string Country { get; set; } = string.Empty;
    
    public string Domain { get; set; } = string.Empty;
}
