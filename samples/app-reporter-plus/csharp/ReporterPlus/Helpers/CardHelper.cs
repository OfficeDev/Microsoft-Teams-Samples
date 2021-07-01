using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.IO;
using ReporterPlus.Models;
using AdaptiveCards;

namespace ReporterPlus.Helpers
{
    public class CardHelper
    {
        public static Attachment CreateAdaptiveCardAttachment(string cardType, BlobDataDeserializer blobData, string channel, out string cardJsonstring)
        {
            Attachment cardAttachment;
            switch (cardType)
            {
                case Status.BaseCard:
                    string[] baseCard = { ".", "Resources", "BaseCard.json" };
                    cardAttachment = GetPayloadBasedOnCardType(baseCard, blobData, Constants.PendingImageUrl, out cardJsonstring);
                    string[] baseMail = { ".", "Resources", "AssignedToCardEmail.json" };
                    GetPayloadBasedOnCardType(baseMail, blobData, Constants.PendingImageUrl, out cardJsonstring);
                    return cardAttachment;
                case Status.Pending:
                    string[] refresh = { ".", "Resources", "AssignedToCard.json" };
                    cardAttachment = GetPayloadBasedOnCardType(refresh, blobData, Constants.PendingImageUrl, out cardJsonstring);
                    if (channel == "outlook")
                    {
                        string[] approvedMail = { ".", "Resources", "AssignedToCardEmail.json" };
                        GetPayloadBasedOnCardType(approvedMail, blobData, Constants.PendingImageUrl, out cardJsonstring);
                    }
                    return cardAttachment;
                case Status.Approved:
                    string[] approved = { ".", "Resources", "ApproveRejectCard.json" };
                    cardAttachment = GetPayloadBasedOnCardType(approved, blobData, Constants.ApprovedImageUrl, out cardJsonstring);
                    if(channel == "outlook")
                    {
                        string[] approvedMail = { ".", "Resources", "ApproveRejectCardEmail.json" };
                        GetPayloadBasedOnCardType(approvedMail, blobData, Constants.ApprovedImageUrl, out cardJsonstring);
                    }
                    return cardAttachment;
                case Status.Rejected:
                    string[] reject = { ".", "Resources", "ApproveRejectCard.json" };
                    cardAttachment = GetPayloadBasedOnCardType(reject, blobData, Constants.RejectedImageUrl, out cardJsonstring);
                    if (channel == "outlook")
                    {
                        string[] rejectedMail = { ".", "Resources", "ApproveRejectCardEmail.json" };
                        GetPayloadBasedOnCardType(rejectedMail, blobData, Constants.RejectedImageUrl, out cardJsonstring);
                    }
                    return cardAttachment;
                default:
                    cardJsonstring = "";
                    return null;
            }
        }

        private static Attachment GetPayloadBasedOnCardType(string[] type, BlobDataDeserializer blobData, string cardStatusImg, out string cardJsonstring)
        {
            var adaptiveCard = File.ReadAllText(Path.Combine(type));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCard);

            var payloadData = new
            {
                originatorId = Constants.OriginatorId,
                requestID = blobData.requestID,
                userMRI = blobData.assignedToId,
                cardTitle = "Request " + blobData.status,
                cardStatusImg = cardStatusImg,
                status = blobData.status,
                itemName = blobData.itemName,
                itemCode = blobData.itemCode,
                assignedToImg = blobData.assignedToUserImage,
                assignedToName = blobData.assignedToName,
                submittedByImg = blobData.submittedByUserImage,
                submittedByName = blobData.submittedByName,
                imagesCount = blobData.imageURL.Count,
                mailViewDetailsUrl = Constants.BaseUrl + "/ViewDetails?id=" + blobData.requestID
            };

            cardJsonstring = template.Expand(payloadData);
            var adaptiveCardAttachmnt = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJsonstring),
            };
            return adaptiveCardAttachmnt;
        }

        public static Attachment CreateJustInTimeCard(string fileName)
        {
            string[] paths = { ".", "Resources", fileName };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}
