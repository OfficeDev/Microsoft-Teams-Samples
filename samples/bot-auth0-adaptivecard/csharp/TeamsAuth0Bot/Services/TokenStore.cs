// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TeamsAuth0Bot.Services
{
    public class TokenStore
    {
        private readonly Dictionary<string, string> _tokens = new();

        public void SaveToken(string userId, string token)
        {
            _tokens[userId] = token;
        }

        public bool TryGetToken(string userId, out string token)
        {
            return _tokens.TryGetValue(userId, out token);
        }

        public void RemoveToken(string userId)
        {
            _tokens.Remove(userId);
        }
    }
}
