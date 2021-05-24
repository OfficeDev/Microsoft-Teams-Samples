// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AdaptiveCards;
using AdaptiveCards.Templating;
using MeetingExtension_SP.Models;
using MessageExtension_SP.Helpers;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MeetingExtension_SP.Helpers
{
    /// <summary>
    /// Adaptive card helper
    /// </summary>
    public class CardHelper
    {
        public Attachment GetAssetCard(AssetCard data, string parent = null)
        {
            var assetListCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveImage()
                    {
                        Id = "bannerimage",
                        Url = data?.ImageUrl != null ? new Uri(data?.ImageUrl?.ToString()) : null,
                        Size = AdaptiveImageSize.Stretch,
                    },
                    new AdaptiveContainer
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = "100",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Id = "Title",
                                                Text = data?.Title,
                                                Weight = AdaptiveTextWeight.Bolder,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },

                    new AdaptiveContainer
                    {
                        Items = new List<AdaptiveElement>()
                        {
                             new AdaptiveColumnSet()
                             {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = "100",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Id = "ShortDescription",
                                                Text = data?.Description,
                                                Size = AdaptiveTextSize.Small,
                                                Wrap = true,
                                            },
                                        },
                                    },
                                },
                             },
                        },
                    },
                },

                Actions = new List<AdaptiveAction>() { },
            };

            var openUrlAction1 = this.AddOpenUrlCardAction(data.ServerRelativeUrl);
            {
                assetListCard.Actions.Add(openUrlAction1);
            }

            var fileType = data.Title.Split('.');
            if (fileType.Count() > 1)
            {

                var action1 = this.AddCardAction(fileType[1].ToString(), data.ServerRelativeUrl);
                if (action1 != null)
                {
                    assetListCard.Actions.Add(action1);
                }
            }
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = assetListCard,
            };
        }

        private AdaptiveAction AddOpenUrlCardAction(string actionData_URL)
        {
            return new AdaptiveOpenUrlAction() { Title = "View in Browser", Url = new Uri(actionData_URL) };
        }

        public AdaptiveAction AddCardAction(string actionType, string actionData_URL)
        {

            switch (actionType)
            {                
                case "pptx":
                    return new AdaptiveOpenUrlAction() { Title = "Open in Teams", Url = new Uri(GetFileViewerURL(actionData_URL, "pptx")) };
                case "xlsx":
                    return new AdaptiveOpenUrlAction() { Title = "Open in Teams", Url = new Uri(GetFileViewerURL(actionData_URL, "xlsx")) };
                case "xls":
                    return new AdaptiveOpenUrlAction() { Title = "Open in Teams", Url = new Uri(GetFileViewerURL(actionData_URL, "xls")) };
                case "docx":
                    return new AdaptiveOpenUrlAction() { Title = "Open in Teams", Url = new Uri(GetFileViewerURL(actionData_URL, "docx")) };
                case "pdf":
                    return new AdaptiveOpenUrlAction() { Title = "Open in Teams", Url = new Uri(GetFileViewerURL(actionData_URL, "pdf")) };
                default:
                    return null;
            }

        }

        private string GetFileViewerURL(string actionData_URL, string prefix)
        {
            return $"https://teams.microsoft.com/_#/{prefix}/viewer/teams/" + actionData_URL?.Replace("/", "~2F");
        }

        public static Attachment CreateAdaptiveCardAttachment(string cardType,IConfiguration configuration)
        {
            switch (cardType)
            {
                case "approvedCard":                    
                    string[] approved = { ".", "Resources", "ApprovedCard.json" };
                    return GetPayloadBasedOnCardType(approved, configuration);
                case "ownerCard":                   
                    string[] owner = { ".", "Resources", "adaptiveCardOwner.json" };
                    return GetPayloadBasedOnCardType(owner, configuration);
                case "userCard":
                    string[] user = { ".", "Resources", "adaptiveCardUser.json" };
                    return GetPayloadBasedOnCardType(user, configuration);
                case "rejectCard":
                    string[] reject = { ".", "Resources", "RejectCard.json" };
                    return GetPayloadBasedOnCardType(reject,configuration);
                default:
                    return null;
            }
        }

        private static Attachment GetPayloadBasedOnCardType(string[] type,IConfiguration configuration)
        {
            var adaptiveCard = File.ReadAllText(Path.Combine(type));

            // Create a Template instance from the template payload
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCard);

            var assetDetails = Common.GetAssetDetails(configuration);

            // You can use any serializable object as your data
            var payloadData = new
            {
                approverName = assetDetails.Result.ApproverName,
                submittedBy = assetDetails.Result.SubmittedBy,
                dateOfSubmission = assetDetails.Result.DateOfSubmission,
                submittedTo = assetDetails.Result.SubitteTo,
                docName = assetDetails.Result.DocName,
                url = assetDetails.Result.url,
                userMRI = assetDetails.Result.userMRI,
                userChat = assetDetails.Result.userChat
            };

            //"Expand" the template -this generates the final Adaptive Card payload
            string cardJson = template.Expand(payloadData);

            var adaptiveCardAttachmnt = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
            return adaptiveCardAttachmnt;
        }
        
    }
}
