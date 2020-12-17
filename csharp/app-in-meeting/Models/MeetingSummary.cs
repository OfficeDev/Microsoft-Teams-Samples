// <copyright file="MeetingSummary.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TokenApp.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Meeting summary model.
    /// </summary>
    public class MeetingSummary
    {
        private static readonly object SyncObj = new object();

        /// <summary>
        /// Gets or sets the meeting metadata.
        /// </summary>
        public MeetingMetadata MeetingMetadata { get; set; }

        /// <summary>
        /// Gets or sets the user token list.
        /// </summary>
        public List<UserToken> UserTokens { get; set; }

        /// <summary>
        /// Gets the deep copy of meeting summary.
        /// </summary>
        /// <returns>the meeting token instance.</returns>
        public MeetingSummary Clone()
        {
            return new MeetingSummary()
            {
                MeetingMetadata = this.MeetingMetadata.Clone(),
                UserTokens = this.UserTokens
                .Where(x => x.Status.Equals(TokenStatus.Waiting) || x.Status.Equals(TokenStatus.Current))
                .Select(userToken => userToken.Clone()).ToList(),
            };
        }

        /// <summary>
        /// Update the user token status.
        /// The method does the below steps:
        /// 1. Get the user token for the input user id or get the current user token if user id is null.
        /// 2. Update the user token with the token status.
        /// 3. Find the next user token with status as Waiting.
        /// 4. Set the current token number with above found user token number.
        /// </summary>
        /// <param name="tokenStatus">The token status to be updated.</param>
        /// <param name="userId">The user's id.</param>
        /// <returns>The meeting summary instance.</returns>
        public MeetingSummary UpdateTokenStatus(TokenStatus tokenStatus, string userId = null)
        {
            lock (SyncObj)
            {
                var userToken = (userId == null) ? this.UserTokens.Where(t => t.TokenNumber == this.MeetingMetadata.CurrentToken).FirstOrDefault() :
                    this.UserTokens.Find((token) => token.UserInfo.AadObjectId == userId);

                if (userToken != null)
                {
                    userToken.Status = tokenStatus;
                    if (userToken.TokenNumber == this.MeetingMetadata.CurrentToken)
                    {
                        if (this.MeetingMetadata.CurrentToken < this.MeetingMetadata.MaxTokenIssued)
                        {
                            var iteratorToken = this.UserTokens.FirstOrDefault(tok => tok.Status.Equals(TokenStatus.Waiting));
                            if (iteratorToken != null)
                            {
                                iteratorToken.Status = TokenStatus.Current;
                                this.MeetingMetadata.CurrentToken = iteratorToken.TokenNumber;
                            }
                            else
                            {
                                this.MeetingMetadata.CurrentToken = this.MeetingMetadata.MaxTokenIssued;
                            }
                        }
                    }
                }

                return this;
            }
        }

        /// <summary>
        /// Get or Add the user token.
        /// The method does the below steps:
        /// 1. Find the user token in the user token list.
        /// 2. If the status of user token is Waiting or Current, then return user token, or delete the token.
        /// 3. Generate the new user token, if the user token was not found or deleted.
        /// 4. Add the token to the user token list.
        /// </summary>
        /// <param name="userInfo">the user information.</param>
        /// <returns>The user token.</returns>
        public UserToken GetorAddToken(UserInfo userInfo)
        {
            lock (SyncObj)
            {
                var userId = userInfo.AadObjectId;

                var userToken = this.UserTokens.Find((token) => token.UserInfo.AadObjectId == userId);

                if (userToken != null)
                {
                    if (userToken.Status.Equals(TokenStatus.Waiting)
                    || userToken.Status.Equals(TokenStatus.Current))
                    {
                        return userToken;
                    }
                    else
                    {
                        this.UserTokens.RemoveAll(x => x.UserInfo.AadObjectId == userToken.UserInfo.AadObjectId);
                    }
                }

                // If token is not available, generate one and sort.
                var newUserToken = new UserToken
                {
                    UserInfo = new UserInfo
                    {
                        Email = userInfo.Email,
                        Name = userInfo.Name,
                        AadObjectId = userInfo.AadObjectId,
                        Role = userInfo.Role,
                    },
                    Status = TokenStatus.Waiting,
                    TokenNumber = ++this.MeetingMetadata.MaxTokenIssued,
                };

                // Making the status of first token as Current.
                if ((this.UserTokens.Count == 0) || ((this.MeetingMetadata.MaxTokenIssued - 1 == this.MeetingMetadata.CurrentToken)
                    && (this.UserTokens.Count > 0) && !this.UserTokens.LastOrDefault().Status.Equals(TokenStatus.Current)))
                {
                    newUserToken.Status = TokenStatus.Current;
                    this.MeetingMetadata.CurrentToken++;
                }

                this.UserTokens.Add(newUserToken);
                this.UserTokens = this.UserTokens?.OrderBy(x => x.TokenNumber).ToList();
                return newUserToken;
            }
        }
    }
}