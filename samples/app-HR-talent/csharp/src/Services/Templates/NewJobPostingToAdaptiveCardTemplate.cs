using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using TeamsTalentMgmtApp.Extensions;
using TeamsTalentMgmtApp.Constants;
using TeamsTalentMgmtApp.Models.DatabaseContext;

namespace TeamsTalentMgmtApp.Services.Templates
{
    public class NewJobPostingToAdaptiveCardTemplate : TemplateManager
    {
        private readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                { nameof(NewJobPostingToAdaptiveCardTemplate), (_, data) => BuildNewJobPostingCard(data) }
            }
        };

        public NewJobPostingToAdaptiveCardTemplate() => Register(new DictionaryRenderer(_responseTemplates));

        private static IMessageActivity BuildNewJobPostingCard(dynamic data)
        {
            var locations = (ReadOnlyCollection<Location>)data.Locations;
            var hiringManagers = (ReadOnlyCollection<Recruiter>)data.HiringManagers;
            var description = (string)data.Description;

            var command = new
            {
                commandId = AppCommands.OpenNewPosition
            };

            var wrapAction = new CardAction
            {
                Title = "Create posting",
                Value = command
            };

            var action = new AdaptiveSubmitAction
            {
                Data = command
            };

            action.RepresentAsBotBuilderAction(wrapAction);

            var card = new AdaptiveCard(new AdaptiveSchemaVersion("1.3"))
            {
                Version = "1.0",
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock("Enter in basic information about this posting")
                    {
                        IsSubtle = true,
                        Wrap = true,
                        Size = AdaptiveTextSize.Small
                    },
                    new AdaptiveTextBlock("Title")
                    {
                        Wrap = true
                    },
                    new AdaptiveTextInput
                    {
                        Id = "jobTitle",
                        Placeholder = "E.g. Senior PM"
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch.ToLowerInvariant(),
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Level") { Wrap = true },
                                    new AdaptiveChoiceSetInput
                                    {
                                        Id = "jobLevel",
                                        Style = AdaptiveChoiceInputStyle.Compact,
                                        Choices = Enumerable.Range(7, 4).Select(x =>
                                        {
                                            var s = x.ToString();
                                            return new AdaptiveChoice
                                            {
                                                Title = s,
                                                Value = s
                                            };
                                        }).ToList(),
                                        Value = "7"
                                    }
                                }
                            },

                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch.ToLowerInvariant(),
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Post by")
                                    {
                                        Wrap = true
                                    },
                                    new AdaptiveDateInput
                                    {
                                        Id = "jobPostingDate",
                                        Placeholder = "Posting date",
                                        Value = DateTime.Now.ToShortDateString()
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Location"),
                                    new AdaptiveChoiceSetInput
                                    {
                                        Id = "jobLocation",
                                        Style = AdaptiveChoiceInputStyle.Compact,
                                        Choices = locations.Select(x => new AdaptiveChoice
                                        {
                                            Value = x.LocationId.ToString(),
                                            Title = x.City
                                        }).ToList(),
                                        Value = Convert.ToString(locations[0].LocationId)
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Hiring manager"),
                                    new AdaptiveChoiceSetInput
                                    {
                                        Id = "jobHiringManager",
                                        Style = AdaptiveChoiceInputStyle.Compact,
                                        Choices = hiringManagers.Select(x => new AdaptiveChoice
                                        {
                                            Value = x.RecruiterId.ToString(),
                                            Title = x.Name
                                        }).ToList(),
                                        Value = Convert.ToString(hiringManagers[0].RecruiterId)
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveTextBlock("Description"),
                    new AdaptiveTextInput
                    {
                        Id = "jobDescription",
                        IsMultiline = true,
                        Placeholder = "E.g. Senior Product Manager in charge of driving complicated work and stuff.",
                        Value = description
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    action
                }
            };

            return MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            });
        }
    }
}
