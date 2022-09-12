// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

public interface ISubEntityService<T, TInput>
{
    /// <summary>
    /// Inserts a sub-entity into the database.
    /// </summary>
    /// <param name="entityId">EntityId of the category</param>
    /// <param name="subEntityInput"></param>
    /// <returns>Sub-entity created</returns>
    Task<T> CreateSubEntity(string entityId, TInput subEntityInput);

    /// <summary>
    /// Gets the subEntity that matches the subEntityId
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="subEntityId"></param>
    /// <returns>SubEntity found. Throws an exception if not found</returns>
    Task<T> GetSubEntity(string entityId, string subEntityId);

    /// <summary>
    /// Gets all the sub-entities belonging to the entity that matches the entityId
    /// </summary>
    /// <param name="entityId">EntityId of the object, whose sub-entities you want</param>
    /// <returns>Array of sub-entities or empty if there are none</returns>
    Task<ICollection<T>> GetSubEntities(string entityId);
}
