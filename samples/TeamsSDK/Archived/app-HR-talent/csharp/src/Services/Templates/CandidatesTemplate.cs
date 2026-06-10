using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
    public class CandidatesTemplate : BaseTemplateManager
    {
        private const string AdaptiveCardVersion = "1.0";

        private readonly LanguageTemplateDictionary _responseTemplates = new LanguageTemplateDictionary
        {
            ["default"] = new TemplateIdMap
            {
                { TemplateConstants.CandidateAsAdaptiveCardWithMultipleItems, (_, data) => BuildCandidateAsAdaptiveCardWithMultipleItems(data) },
                { TemplateConstants.CandidateAsFileConsentCardWithMultipleItems, (_, data) => BuildCandidateAsFileConsentCardWithMultipleItems(data) },
                { TemplateConstants.CandidateAsThumbnailCardWithMultipleItems, (_, data) => BuildCandidateAsThumbnailCardWithMultipleItems(data) }
            }
        };

        public CandidatesTemplate() => Register(new DictionaryRenderer(_responseTemplates));

        private static IMessageActivity BuildCandidateAsFileConsentCardWithMultipleItems(dynamic data)
            => BuildCards<CandidateTemplateModel, Candidate>((CandidateTemplateModel)data, BuildSingleCandidateCardAsFileConsent, BuildMultipleCandidatesCard);

        private static IMessageActivity BuildCandidateAsAdaptiveCardWithMultipleItems(dynamic data)
            => BuildCards<CandidateTemplateModel, Candidate>((CandidateTemplateModel)data, BuildSingleCandidateCardAsAdaptive, BuildMultipleCandidatesCard);

        private static IMessageActivity BuildCandidateAsThumbnailCardWithMultipleItems(dynamic data)
            => BuildCards<CandidateTemplateModel, Candidate>((CandidateTemplateModel)data, BuildSingleCandidateCardAsThumbnail, BuildMultipleCandidatesCard);

        private static Attachment BuildSingleCandidateCardAsThumbnail(CandidateTemplateModel data)
        {
            var candidate = data.Items.First();
            var card = new ThumbnailCard
            {
                Title = candidate.Name,
                Text = $"Current role: {candidate.CurrentRole} | {candidate.Location?.LocationAddress ?? string.Empty}",
                Images = new List<CardImage>
                {
                    new CardImage(candidate.ProfilePicture)
                }
            };

            return card.ToAttachment();
        }

        private static Attachment BuildSingleCandidateCardAsFileConsent(CandidateTemplateModel data)
        {
            var candidate = data.Items.First();
            var consentContext = new FileConsentContext
            {
                CandidateId = candidate.CandidateId
            };

            var fileConsentCard = new FileConsentCard
            {
                Description = $"Here is summary for {candidate.Name}",
                SizeInBytes = Encoding.UTF8.GetBytes(candidate.Summary).Length,
                AcceptContext = consentContext,
                DeclineContext = consentContext
            };

            return new Attachment
            {
                ContentType = FileConsentCard.ContentType,
                Content = fileConsentCard,
                Name = SanitizeFileName($"{candidate.Name} Summary.txt")
            };
        }

        private static Attachment BuildMultipleCandidatesCard(CandidateTemplateModel data)
        {
            var botCommand = string.IsNullOrEmpty(data.BotCommand)
                ? BotCommands.CandidateDetailsDialogCommand
                : data.BotCommand;
            var cardListItems = new List<CardListItem>();
            foreach (var candidate in data.Items)
            {
                var cardListItem = new CardListItem
                {
                    Icon = candidate.ProfilePicture,
                    Type = CardListItemTypes.ResultItem,
                    Title = $"<b>{candidate.Name}</b>",
                    Subtitle = $"Current role: {candidate.CurrentRole} | Stage: {candidate.Stage.ToString()} | {candidate.Location?.LocationAddress ?? string.Empty}",
                    Tap = new CardAction(ActionTypes.ImBack, value: $"{botCommand} {candidate.Name}")
                };

                cardListItems.Add(cardListItem);
            }

            var attachment = new Attachment
            {
                ContentType = ListCard.ContentType,
                Content = new ListCard
                {
                    Title = data.ListCardTitle,
                    Items = cardListItems
                }
            };

            return attachment;
        }

        private static Attachment BuildSingleCandidateCardAsAdaptive(CandidateTemplateModel data)
        {
            var card = new AdaptiveCard(AdaptiveCardVersion);

            var candidate = data.Items.First();
            card.Body = new List<AdaptiveElement>
            {
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = AdaptiveColumnWidth.Auto.ToLowerInvariant(),
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveImage(candidate.ProfilePicture)
                                {
                                    Size = AdaptiveImageSize.Large,
                                    Style = AdaptiveImageStyle.Person
                                }
                            }
                        },
                        new AdaptiveColumn
                        {
                            Width = AdaptiveColumnWidth.Stretch.ToLowerInvariant(),
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock(candidate.Name)
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Medium
                                },
                                new AdaptiveTextBlock(candidate.Summary)
                                {
                                    Wrap = true
                                }
                            }
                        }
                    }
                },
                new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact("Current role:", candidate.CurrentRole),
                        new AdaptiveFact("Location:", candidate.Location?.LocationAddress ?? string.Empty),
                        new AdaptiveFact("Stage:", candidate.Stage.ToString()),
                        new AdaptiveFact("Position applied:", candidate.Position.Title),
                        new AdaptiveFact("Date applied:", candidate.DateApplied.ToLongDateString()),
                        new AdaptiveFact("Phone number:", candidate.Phone)
                    }
                }
            };

            card.Actions = new List<AdaptiveAction>();

            if (candidate.Stage != InterviewStageType.Offered)
            {
                card.Actions.Add(new AdaptiveShowCardAction
                {
                    Title = "Schedule an interview",
                    Card = GetAdaptiveCardForInterviewRequest(data.Interviewers.ToList(), candidate, DateTime.Now.Date.AddDays(1.0), data.Locale)
                });
            }

            if (candidate.Comments.Any() || candidate.Interviews.Any())
            {
                var contentUrl = data.AppSettings.BaseUrl + $"StaticViews/CandidateFeedback.html?candidateId={candidate.CandidateId}";
                card.Actions.Add(new AdaptiveOpenUrlAction
                {
                    Title = "Open candidate feedback",
                    Url = new Uri(string.Format(
                        CommonConstants.TaskModuleUrlFormat,
                        data.AppSettings.TeamsAppId,
                        Uri.EscapeDataString(contentUrl),
                        Uri.EscapeDataString("Feedback for " + candidate.Name),
                        data.AppSettings.MicrosoftAppId,
                        "large",
                        "large"))
                });
            }

            var leaveCommentCommand = new
            {
                commandId = AppCommands.LeaveInternalComment,
                candidateId = candidate.CandidateId
            };

            var wrapAction = new CardAction
            {
                Title = "Submit",
                Value = leaveCommentCommand
            };

            var action = new AdaptiveSubmitAction
            {
                Data = leaveCommentCommand
            };

            action.RepresentAsBotBuilderAction(wrapAction);

            card.Actions.Add(new AdaptiveShowCardAction
            {
                Title = "Leave comment",
                Card = new AdaptiveCard(AdaptiveCardVersion)
                {
                    Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextInput
                        {
                            Id = "comment",
                            Placeholder = "Leave an internal comment for this candidate",
                            IsMultiline = true
                        }
                    },
                    Actions = new List<AdaptiveAction>
                    {
                        action
                    }
                }
            });

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        private static AdaptiveCard GetAdaptiveCardForInterviewRequest(
            List<Recruiter> interviewers,
            Candidate candidate,
            DateTime interviewDate,
            string locale)
        {
            var command = new
            {
                commandId = AppCommands.ScheduleInterview,
                candidateId = candidate.CandidateId
            };

            var wrapAction = new CardAction
            {
                Title = "Schedule",
                Value = command
            };

            var action = new AdaptiveSubmitAction
            {
                Data = command
            };

            action.RepresentAsBotBuilderAction(wrapAction);

            return new AdaptiveCard(AdaptiveCardVersion)
            {
                Version = "1.0",
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = $"Set interview date for {candidate.Name}",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto.ToLowerInvariant(),
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(candidate.ProfilePicture),
                                        Size = AdaptiveImageSize.Medium,
                                        Style = AdaptiveImageStyle.Person
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch.ToLowerInvariant(),
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"Position: {candidate.Position.Title}",
                                        Wrap = true
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"Position ID: {candidate.Position.PositionExternalId}",
                                        Spacing = AdaptiveSpacing.None,
                                        Wrap = true,
                                        IsSubtle = true
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = "interviewerId",
                        Style = AdaptiveChoiceInputStyle.Compact,
                        Choices = interviewers.Select(x => new AdaptiveChoice
                        {
                            Value = x.RecruiterId.ToString(),
                            Title = x.Name
                        }).ToList(),
                        Value = Convert.ToString(interviewers[0].RecruiterId)
                    },
                    new AdaptiveDateInput
                    {
                        Id = "interviewDate", Placeholder = "Enter in a date for the interview"
                        //Removing this for now as there seems to be an issue with the placeholder date value
                        //the passed local is en-GB but Teams seems to be expecting a US date format
                        //, Value = "01/14/2022"
                        //, Value = interviewDate.ToString(new CultureInfo(locale).DateTimeFormat.ShortDatePattern)
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = "interviewType",
                        Style = AdaptiveChoiceInputStyle.Compact,
                        IsMultiSelect = false,
                        Choices = new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Phone screen", Value = "phoneScreen" },
                            new AdaptiveChoice { Title = "Full loop", Value = "fullLoop" },
                            new AdaptiveChoice { Title = "Follow-up", Value = "followUp" }
                        },
                        Value = "phoneScreen"
                    },
                    new AdaptiveToggleInput { Id = "isRemote", Title = "Remote interview" }
                },
                Actions = new List<AdaptiveAction>
                {
                    action
                }
            };
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(invalidChar))
                {
                    fileName = fileName.Replace(invalidChar, '_');
                }
            }

            return fileName;
        }
    }
}
