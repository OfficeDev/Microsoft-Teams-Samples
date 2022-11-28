// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Graph;
using Microsoft.Graph.Communications.Core.Notifications;

namespace CallingMeetingBot.Extensions
{
    public static class NotificationEventArgsExtensions
    {
        public static bool IsParticipantsNotification(this NotificationEventArgs args) =>
            args.ChangeType == ChangeType.Updated &&
                args.Notification.ResourceUrl.Contains("/participants");
    }
}
