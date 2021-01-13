// <copyright file="UserToken.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Models
{
    /// <summary>
    /// User token model.
    /// </summary>
    public class UserToken
    {
        /// <summary>
        /// Gets or sets the user information.
        /// </summary>
        public UserInfo UserInfo { get; set; }

        /// <summary>
        /// Gets or sets the token number.
        /// </summary>
        public int TokenNumber { get; set; }

        /// <summary>
        /// Gets or sets the status of the token.
        /// </summary>
        public TokenStatus Status { get; set; }

        /// <summary>
        /// Gets the deep copy of User Token.
        /// </summary>
        /// <returns>the user token instance.</returns>
        public UserToken Clone()
        {
            return new UserToken()
            {
                Status = this.Status,
                TokenNumber = this.TokenNumber,
                UserInfo = this.UserInfo.Clone(),
            };
        }
    }
}