// <copyright file="IRequestProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabActivityFeed.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TabRequestApproval.Model;

    /// <summary>
    /// Represents the request provider.
    /// </summary>
    public interface IRequestProvider
    {
        /// <summary>
        /// Creates a request within the database.
        /// </summary>
        /// <param name="requestInfo">Represents the request information.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the container id calculated by the developer.</param>
        /// <returns>A Task representing completion of asynchronous operation.</returns>
        Task CreateRequestAsync(RequestInfo requestInfo, string teamsAppInstallationScopeId);

        /// <summary>
        /// Retrieves all the requests stored within the database.
        /// </summary>
        /// <param name="teamsAppInstallationScopeId">Represents the container id calculated by the developer.</param>
        /// <returns>An iterable object containing all requests.</returns>
        Task<IEnumerable<RequestInfo>> GetRequestsAsync(string teamsAppInstallationScopeId);

        /// <summary>
        /// Retrieves the request with the specified id from the database.
        /// </summary>
        /// <param name="requestId">Represents the request id.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the container id calculated by the developer.</param>
        /// <returns>The specified request.</returns>
        Task<RequestInfo> GetRequestAsync(string requestId, string teamsAppInstallationScopeId);

        /// <summary>
        /// Updates the specified request.
        /// </summary>
        /// <param name="requestId">Represents the request id.</param>
        /// <param name="requestInfo">Represents the updated request.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the container id calculated by the developer.</param>
        /// <returns>The updated request.</returns>
        Task<RequestInfo> UpdateRequestAsync(string requestId, RequestInfo requestInfo, string teamsAppInstallationScopeId);
    }
}
