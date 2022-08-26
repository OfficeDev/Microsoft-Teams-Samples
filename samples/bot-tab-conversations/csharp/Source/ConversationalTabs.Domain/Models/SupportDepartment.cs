// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Model for a Support Department, which is the Entity in our example
/// </summary>
public record SupportDepartment(
    [property: JsonPropertyName("supportDepartmentId")]
    string Id,
    string Title,
    string Description,
    string GroupId,
    string TeamChannelId,
    string TenantId,
    MsTeamsBotData ProactiveBotData,
    ICollection<CustomerInquiry> SubEntities
) : IEntity<CustomerInquiry>;

/// <summary>
/// Model to create a Customer Support Department, passed in request body of the API
/// </summary>
public record SupportDepartmentInput(
    string Title,
    string Description,
    string TeamChannelId,
    string TeamId,
    string GroupId,
    string TenantId
);
