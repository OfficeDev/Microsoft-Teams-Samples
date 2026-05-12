// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples.Models;

public class WelcomeUserState
{
    public bool DidUserSelectDomain { get; set; } = false;

    public string SelectedDomain { get; set; } = string.Empty;

    public string SelectedRegion { get; set; } = string.Empty;
}
