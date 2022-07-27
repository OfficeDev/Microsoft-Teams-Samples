// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public interface IRepositoryObject
{
    /// <summary>
    /// A unique Id attached to the object. This can be used for storing the object in a datastore.
    /// </summary>
    /// <value>A string containing the id for the object.</value>
    string Id { get; }
}
