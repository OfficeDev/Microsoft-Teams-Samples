// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingMediaBot.Web.Services.MicrosoftGraph;

using System.Threading.Tasks;
using CallingMediaBot.Web.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

public class OnlineMeetingService : IOnlineMeetingService
{
    private readonly GraphServiceClient graphServiceClient;
    private readonly UsersOptions users;

    public OnlineMeetingService(GraphServiceClient graphServiceClient, IOptions<UsersOptions> users)
    {
        this.graphServiceClient = graphServiceClient;
        this.users = users.Value;
    }

    /// <inheritdoc/>
    public Task<OnlineMeeting> Create(string subject)
    {
        var onlineMeeting = new OnlineMeeting
        {
            StartDateTime = DateTime.UtcNow,
            EndDateTime = DateTime.UtcNow.AddMinutes(30),
            Subject = subject,
        };

        var userId = users.UserIdWithAssignedOnlineMeetingPolicy;

        return graphServiceClient.Users[userId].OnlineMeetings
            .Request()
            .AddAsync(onlineMeeting);
    }

    /// <inheritdoc/>
    public Task<OnlineMeeting> Get(string meetingId)
    {
        var userId = users.UserIdWithAssignedOnlineMeetingPolicy;

        return graphServiceClient.Users[userId].OnlineMeetings[meetingId]
            .Request()
            .GetAsync();
    }
}
