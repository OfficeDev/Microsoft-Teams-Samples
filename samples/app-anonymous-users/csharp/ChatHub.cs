// <copyright file="ChatHub.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AnonymousUsers.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string description, int count)
        {
            await Clients.All.SendAsync("ReceiveMessage", description, count);
        }
    }
}