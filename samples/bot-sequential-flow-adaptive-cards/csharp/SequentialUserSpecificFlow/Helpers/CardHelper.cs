using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SequentialUserSpecificFlow.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SequentialUserSpecificFlow.Helpers
{
    public class CardHelper
    {
        /// <summary>
        /// Method to get First options Adaptive card.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="name"></param>
        /// <param name="userMRI"></param>
        /// <returns></returns>
        public static Attachment GetFirstOptionsAdaptiveCard(string[] filepath, string name = null, string userMRI = null)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                createdById = userMRI,
                createdBy = name
            };
            var cardJsonstring = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJsonstring),
            };

            return adaptiveCardAttachment;
        }

        public static Attachment GetResponseAttachment(string[] filepath, InitialSequentialCard data, out string cardJsonString)
        {
            var adaptiveCardJson = File.ReadAllText(Path.Combine(filepath));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                incidentTitle = data.action.data.IncidentTitle,
                assignedTo = data.action.data.AssignedTo,
                category = data.action.data.Category,
                subCategory = data.action.data.SubCategory,
                createdBy = data.action.data.CreatedBy,
                assignedToName = data.action.data.AssignedToName,
                userMRI = data.action.data.UserMRI,
                incidentId = data.action.data.IncidentId
            };
            cardJsonString = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJsonString),
            };

            return adaptiveCardAttachment;
        }

        // Get no incident found card.
        public static Attachment GetNoInicidentFoundCard()
        {
            //Read the card json and create attachment.
            string[] paths = { ".", "Resources", "noIncidentFound.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }

        // Get incident list card.
        public static Attachment GetInicidentListCard(IncidentList incidentList)
        {
            //Read the card json and create attachment.
            string[] paths = { ".", "Resources", "incidentListCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var cardJsonstring = template.Expand(incidentList);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJsonstring)
            };

            return adaptiveCardAttachment;
        }

        // Get incident review card send from messaging extension.
        public static Attachment GetIncidentReviewCard(IncidentDetails incidentDetail)
        {
            string[] paths = { ".", "Resources", "reviewCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var payloadData = new
            {
                incidentTitle = incidentDetail.IncidentTitle,
                assignedTo = incidentDetail.AssignedToName,
                category = incidentDetail.Category,
                subCategory = incidentDetail.SubCategory,
                createdBy = incidentDetail.CreatedBy,
                assignedToName = incidentDetail.AssignedToName,
                userMRI = incidentDetail.AssignedToMRI,
                incidentId = incidentDetail.IncidentId
            };
            var cardJsonString = template.Expand(payloadData);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(cardJsonString),
            };

            return adaptiveCardAttachment;
        }
    }
}
