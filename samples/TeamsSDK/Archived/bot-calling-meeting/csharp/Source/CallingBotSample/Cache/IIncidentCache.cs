// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CallingBotSample.Models;

namespace CallingBotSample.Cache
{
    public interface IIncidentCache
    {
        /// <summary>
        /// Try get the an incident's details
        /// </summary>
        /// <param name="callId">The incident's call ID</param>
        /// <param name="incidentDetails">The incident's details if it exists</param>
        /// <returns>Whether the details exists</returns>
        bool TryGetValue(string callId, out IncidentDetails incidentDetails);

        /// <summary>
        /// Sets the incident's details
        /// </summary>
        /// <param name="callId">The incident's call ID</param>
        /// <param name="incidentDetails">The incident's details</param>
        /// <returns></returns>
        void Set(string callId, IncidentDetails incidentDetails);
    }
}
