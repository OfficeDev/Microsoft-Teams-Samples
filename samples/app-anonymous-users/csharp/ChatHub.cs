// <copyright file="ChatHub.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AnonymousUsers
{
    /// <summary>
    /// To make calls to specific clients, use the properties of the Clients object.
    /// </summary>
    public class ChatHub : Hub
    {
        /// This method is used to sends a message to all connected clients using Clients.All and it will sync the data in UI.
        public async Task SendMessage(string description, int count)
        {
            await Clients.All.SendAsync("ReceiveMessage", description, count);
        }
    }
}