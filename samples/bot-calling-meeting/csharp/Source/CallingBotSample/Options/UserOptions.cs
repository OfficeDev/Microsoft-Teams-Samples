// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CallingBotSample.Options
{
    public class UsersOptions
    {
        public UserOptions[] users { get; set; }
    }

    public class UserOptions
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }
}
