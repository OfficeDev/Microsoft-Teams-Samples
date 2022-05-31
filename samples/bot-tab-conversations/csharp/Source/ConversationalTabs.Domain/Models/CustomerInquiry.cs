// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Model for a CustomerInquiry, which is a SubEntity in our example.
/// </summary>
public record CustomerInquiry(
    string SubEntityId,
    DateTimeOffset CreatedDateTime,
    string CustomerName,
    string Question,
    string ConversationId,
    bool Active
): ISubEntity;

/// <summary>
/// Model to create a CustomerInquiry, passed in request body of the API
/// </summary>
public record CustomerInquiryInput(
    [MaxLength(9000)]
    string CustomerName,
    [MaxLength(9000)]
    string Question
);
