// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Model for a Microsoft Teams Bot data
/// This contains info that will allow proactive message to this MS Team later
/// </summary>
public record MsTeamsBotData(
    [property: JsonPropertyName("teamId")]
    string Id,
    Uri ServiceUrl
) : IRepositoryObject;
