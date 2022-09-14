// <copyright file="ISimpleBetaGraphClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace EventMeeting.Provider
{
    using Microsoft.Graph;

    public interface ISimpleBetaGraphClient
    {
        public GraphServiceClient GetGraphClientforApp();
    }
}
