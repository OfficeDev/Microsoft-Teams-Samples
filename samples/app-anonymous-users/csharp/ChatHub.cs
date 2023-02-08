// <copyright file="ChatHub.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AnonymousUsers.Hubs
{
    /// <summary>
    /// Create a hub by declaring a class that inherits from Hub.
    /// Add public methods to the class to make them callable from clients.
    /// </summary>
    public class ChatHub : Hub
    {
        public async Task SendMessage(string description, int count)
        {
            await Clients.All.SendAsync("ReceiveMessage", description, count);
        }
    }
}