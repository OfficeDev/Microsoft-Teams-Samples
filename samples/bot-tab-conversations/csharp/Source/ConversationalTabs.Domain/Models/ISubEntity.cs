// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

/// <summary>
/// Model for a SubEntity
/// In Microsoft Teams tab, a SubEntityId is used to indicate a specific state within an Entity
/// </summary>
public interface ISubEntity
{
    /// <summary>
    /// A subEntityId refers to an object/place/document state inside an entity.
    /// And is valuable for deep linking to a specific state in an entity.
    /// This subEntityId can be any string value you like but is used in this proof of concept like
    /// a database primary key, so it must be unique across subEntities
    /// </summary>
    string SubEntityId { get; }

    /// <summary>
    /// The creation time of the subentity
    /// </summary>
    DateTimeOffset CreatedDateTime { get; }
}
