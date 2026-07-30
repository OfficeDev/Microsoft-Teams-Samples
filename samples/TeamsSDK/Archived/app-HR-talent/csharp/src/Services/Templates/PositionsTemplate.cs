using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using TeamsTalentMgmtApp.Bot.Models;
using TeamsTalentMgmtApp.Extensions;
using TeamsTalentMgmtApp.Models.TemplateModels;
using TeamsTalentMgmtApp.Constants;
using TeamsTalentMgmtApp.Models.Bot;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Templates
{
    public class PositionsTemplate : BaseTemplateManager
    {
        private readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                { TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, (_, data) => BuildPositionAsAdaptiveCardWithMultipleItems(data) },
                { TemplateConstants.PositionAsThumbnailCardWithMultipleItems, (_, data) => BuildPositionAsThumbnailCardWithMultipleItems(data) }
            }
        };

        public PositionsTemplate() => Register(new DictionaryRenderer(_responseTemplates));

        private static IMessageActivity BuildPositionAsAdaptiveCardWithMultipleItems(dynamic data)
            => BuildCards<PositionTemplateModel, Position>((PositionTemplateModel)data, BuildPositionAsAdaptiveCard, BuildMultiplePositionsCard);

        private static IMessageActivity BuildPositionAsThumbnailCardWithMultipleItems(dynamic data)
            => BuildCards<PositionTemplateModel, Position>((PositionTemplateModel)data, BuildPositionAsThumbnailCard, BuildMultiplePositionsCard);

        private static Attachment BuildPositionAsAdaptiveCard(PositionTemplateModel data)
        {
            var card = new AdaptiveCard("1.0");
            var position = data.Items.First();
            card.Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock(position.Title)
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.Medium
                },

                new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact("Position ID:", position.PositionExternalId),
                        new AdaptiveFact("Location:", position.Location?.LocationAddress ?? string.Empty),
                        new AdaptiveFact("Days open:", position.DaysOpen.ToString()),
                        new AdaptiveFact("Applicants:", position.Candidates.Count.ToString()),
                        new AdaptiveFact("Hiring manager:", position.HiringManager.Name)
                    }
                },

                new AdaptiveTextBlock($"Description: {position.Description}")
                {
                    Wrap = true
                }
            };

            if (data.ButtonActions != null && data.ButtonActions is List<AdaptiveAction> actions && actions.Count > 0)
            {
                card.Actions = actions;
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        private static Attachment BuildPositionAsThumbnailCard(PositionTemplateModel data)
        {
            var position = data.Items.First();
            var card = new ThumbnailCard
            {
                Title = $"{position.Title} / {position.PositionExternalId}",
                Text = $"Hiring manager: {position.HiringManager.Name} | {position.Location?.LocationAddress ?? string.Empty}"
            };

            return card.ToAttachment();
        }

        private static Attachment BuildMultiplePositionsCard(PositionTemplateModel data)
        {
            var positions = data.Items;
            var botCommand = data.BotCommand;
            var title = data.ListCardTitle;
            var cardListItems = new List<CardListItem>();
            foreach (var position in positions)
            {
                var cardListItem = new CardListItem
                {
                    Icon = position.HiringManager.ProfilePicture,
                    Type = CardListItemTypes.ResultItem,
                    Title = $"<b>{position.PositionId} - {position.Title}</b>",
                    Subtitle = $"Applicants: {position.Candidates.Count} | Hiring manager: {position.HiringManager.Name} | Days open: {position.DaysOpen}",
                    Tap = new CardAction(ActionTypes.ImBack, value: $"{botCommand} {position.PositionExternalId}")
                };

                cardListItems.Add(cardListItem);
            }

            var listCard = new ListCard
            {
                Title = title,
                Items = cardListItems
            };

            if (data.ButtonActions != null && data.ButtonActions != default(Dictionary<string, string>))
            {
                var buttonActions = (Dictionary<string, string>)data.ButtonActions;
                listCard.Buttons = new List<CardAction>();
                foreach (var item in buttonActions)
                {
                    listCard.Buttons.Add(new CardAction(ActionTypes.ImBack, item.Key, value: item.Value));
                }
            }

            return new Attachment
            {
                ContentType = ListCard.ContentType,
                Content = listCard
            };
        }
    }
}
