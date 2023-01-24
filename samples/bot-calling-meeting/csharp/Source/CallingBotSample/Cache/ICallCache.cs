// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingBotSample.Cache
{
    public interface ICallCache
    {
        /// <summary>
        /// If the at least one user has joined the call
        /// </summary>
        /// <param name="callId">The call's ID</param>
        /// <returns>Whether the a user has joined the call</returns>
        bool GetAtLeastOneUserJoined(string callId);

        /// <summary>
        /// Set's whether a user has joined a call
        /// </summary>
        /// <param name="callId">The call's ID</param>
        /// <param name="hasAtLeastOneUserJoined"></param>
        /// <returns></returns>
        void SetAtLeastOneUserJoined(string callId, bool hasAtLeastOneUserJoined = true);

        /// <summary>
        ///
        /// </summary>
        /// <param name="callId">The call's ID</param>
        /// <returns>Whether the call has been established</returns>
        bool GetIsEstablished(string callId);

        /// <summary>
        /// Set's if the call has been established
        /// </summary>
        /// <param name="callId">The call's ID</param>
        /// <param name="isEstablished"></param>
        /// <returns></returns>
        void SetIsEstablished(string callId, bool isEstablished = true);
    }
}
