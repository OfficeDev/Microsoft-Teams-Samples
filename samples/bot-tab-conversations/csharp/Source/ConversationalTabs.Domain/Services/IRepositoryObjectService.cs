// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public interface IRepositoryObjectService<T, TInput> where T : IRepositoryObject
{
    /// <summary>
    /// Inserts an object into the database.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Object created</returns>
    Task<T> Create(TInput input);

    /// <summary>
    /// GetSingle finds the first or default object that matches the given id
    /// </summary>
    /// <param name="id">Id of the object</param>
    /// <returns>Object found or null if not found</returns>
    Task<T> GetSingle(string id);

    /// <summary>
    /// Get all the objects that user has access too.
    /// Access is determined based on the channels, and entities associated with those channel.
    /// </summary>
    /// <returns>List of objects found. An empty list if no matching objects are found.</returns>
    Task<ICollection<T>> GetAll();
}
