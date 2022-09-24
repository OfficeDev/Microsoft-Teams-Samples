// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Domain.IRepositories;

using System.Threading.Tasks;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;

public interface IRepository<T> where T: IRepositoryObject
{
    /// <summary>
    /// Inserts or updates a object in the database.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Object created</returns>
    Task<T> CreateOrUpdateObject(T input);

    /// <summary>
    /// Gets the object that matches the id
    /// </summary>
    /// <param name="id">id of the object</param>
    /// <returns>Object found or null if not found</returns>
    Task<T> GetObject(string id);

    /// <summary>
    /// Get all the objects.
    /// </summary>
    /// <returns>List of objects found. An empty list if no matching objects are found.</returns>
    Task<ICollection<T>> GetAllObjects();

    /// <summary>
    /// Deletes a object from the database.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Wether the object was deleted</returns>
    Task<bool> DeleteObject(T item);
}
