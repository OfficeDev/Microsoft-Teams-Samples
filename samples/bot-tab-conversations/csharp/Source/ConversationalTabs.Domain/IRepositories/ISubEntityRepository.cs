// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;

using System.Threading.Tasks;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public interface ISubEntityRepository<T> where T : ISubEntity
{
    /// <summary>
    /// Inserts a subEntity into the database.
    /// </summary>
    /// <param name="objectEntityId">EntityId of T</param>
    /// <param name="subEntity"></param>
    /// <returns>Item created</returns>
    Task<T> CreateSubEntity(string objectEntityId, T subEntity);

    /// <summary>
    /// Updates a subEntity in the database.
    /// </summary>
    /// <param name="objectEntityId">EntityId of T</param>
    /// <param name="subEntity"></param>
    /// <returns>Item Updated</returns>
    Task<T> UpdateSubEntity(string objectEntityId, T subEntity);

    /// <summary>
    /// Deletes a subEntity from the database.
    /// </summary>
    /// <param name="objectEntityId">EntityId of T</param>
    /// <param name="subEntity">subEntity to delete</param>
    /// <returns>Boolean if deletion is successful</returns>
    Task<bool> DeleteSubEntity(string objectEntityId, T subEntity);

    /// <summary>
    /// Gets all the subEntities that matches the entityId
    /// </summary>
    /// <param name="objectEntityId">EntityId of T</param>
    /// <returns>List subEntities under the object, empty if no subEntities are found</returns>
    Task<ICollection<T>> GetSubEntities(string objectEntityId);
}
