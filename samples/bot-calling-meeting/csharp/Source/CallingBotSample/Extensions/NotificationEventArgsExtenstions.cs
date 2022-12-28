// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Graph;
using Microsoft.Graph.Communications.Core.Notifications;

namespace CallingMeetingBot.Extensions
{
    /// <summary>
    /// Extensions for NotificationEventArgs
    /// </summary>
    public static class NotificationEventArgsExtensions
    {
        /// <summary>
        /// Checks if an incoming notification is about participants changing
        /// </summary>
        /// <param name="args">The notification</param>
        /// <returns>Whether it is a participant notification</returns>
        public static bool IsParticipantsNotification(this NotificationEventArgs args) =>
            args.ChangeType == ChangeType.Updated &&
                args.Notification.ResourceUrl.Contains("/participants");
    }
}
