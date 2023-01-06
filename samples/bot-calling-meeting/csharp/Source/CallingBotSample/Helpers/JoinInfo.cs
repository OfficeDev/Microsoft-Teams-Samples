// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Graph;

namespace CallingBotSample.Helpers
{
    /// <summary>
    /// Gets the join information.
    /// </summary>
    public class JoinInfo
    {
        /// <summary>
        /// Parse Join URL into its components.
        /// </summary>
        /// <param name="joinURL">Join URL from Team's meeting body.</param>
        /// <returns>Parsed data.</returns>
        public static MeetingInfo ParseMeetingInfo(string joinURL)
        {
            var decodedURL = WebUtility.UrlDecode(joinURL);

            //// URL being needs to be in this format.
            //// https://teams.microsoft.com/l/meetup-join/19:cd9ce3da56624fe69c9d7cd026f9126d@thread.skype/1509579179399?context={"Tid":"72f988bf-86f1-41af-91ab-2d7cd011db47","Oid":"550fae72-d251-43ec-868c-373732c2704f","MessageId":"1536978844957"}
            //// https://teams.microsoft.com/l/meetup-join/19:meeting_MDQzYmJlMDctMWJiZS00OGExLTlmYjUtZTczNzVhZGM1OTQx@thread.v2/0?context={"Tid":"c80f38d3-c04c-49bf-a48b-9d99278d4ac6","Oid":"782f076f-f6f9-4bff-9673-ea1997283e9c"}

            var regex = new Regex("https://teams\\.microsoft\\.com.*/(?<thread>[^/]+)/(?<message>[^/]+)\\?context=(?<context>{.*})");
            var match = regex.Match(decodedURL);
            if (!match.Success)
            {
                throw new ArgumentException($"Join URL cannot be parsed: {joinURL}.", nameof(joinURL));
            }

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(match.Groups["context"].Value)))
            {
                var ctxt = (Context)new DataContractJsonSerializer(typeof(Context)).ReadObject(stream);

                var meetingInfo = new OrganizerMeetingInfo
                {
                    Organizer = new IdentitySet
                    {
                        User = new Identity { Id = ctxt.Oid },
                    },
                };
                meetingInfo.Organizer.User.SetTenantId(ctxt.Tid);

                return meetingInfo;
            }
        }

        /// <summary>
        /// Join URL context.
        /// </summary>
        [DataContract]
        private class Context
        {
            /// <summary>
            /// Gets or sets the Tenant Id.
            /// </summary>
            [DataMember]
            public string? Tid { get; set; }

            /// <summary>
            /// Gets or sets the AAD object id of the user.
            /// </summary>
            [DataMember]
            public string? Oid { get; set; }

            /// <summary>
            /// Gets or sets the chat message id.
            /// </summary>
            [DataMember]
            public string? MessageId { get; set; }
        }
    }
}
