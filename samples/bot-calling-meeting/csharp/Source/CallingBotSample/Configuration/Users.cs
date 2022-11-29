// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace CallingBotSample.Configuration
{
    public class Users
    {
        public List<User>? users { get; set; }
    }

    public class User
    {
        public string? DisplayName { get; set; }
        public string? Id { get; set; }
    }

}
