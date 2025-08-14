// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace WebhookSampleBot.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Bot.Connector;
    using AdaptiveCards;

    /// <summary>
    /// Provider of sample responses based on triggers contained in input string.
    /// </summary>
    public class SampleResponseProvider
    {
        /// <summary>
        /// The input that triggers all supported responses.
        /// </summary>
        private const string EverythingTrigger = "everything";

        /// <summary>
        /// Dictionary that maps trigger words with their handers.
        /// </summary>
        private static readonly Dictionary<string, Func<string, List<Attachment>>> TriggerHandler
            = SampleResponseProvider.InitializeTriggerHandler();

        /// <summary>
        /// Builds and returns an AdaptiveCard with an image.
        /// </summary>
        /// <returns></returns>
        private static Attachment GetAdaptiveCardAttachment()
        {
            var Card = new AdaptiveCard(new AdaptiveSchemaVersion("1.2"))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveImage(){Url=new Uri("https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6")},
                    new AdaptiveTextBlock(){Text="Sample image for Adaptive Card.."}
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <param name="inputContainingTriggers">The input containing triggers.</param>
        /// <returns>Activity containing response to triggers.</returns>
        public static Microsoft.Bot.Connector.Activity GetResponseActivity(Microsoft.Bot.Connector.Activity inputContainingTriggers)
        {
            var sampleResponseActivity = new Microsoft.Bot.Connector.Activity
            {
                Text = "Here's your response: " + inputContainingTriggers.From.Name,
                Attachments = SampleResponseProvider.GetAttachments(inputContainingTriggers.Text)
            };

            if (sampleResponseActivity.Attachments == null || !sampleResponseActivity.Attachments.Any())
            {
                // Show help!
                var reply = string.Join("<br />", SampleResponseProvider.TriggerHandler.Keys);
                sampleResponseActivity.Text += "<br />" + reply;
            }

            return sampleResponseActivity;
        }

        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <param name="inputContainingTriggers">The input containing triggers.</param>
        /// <returns>A comprehensive list of attachments triggered by each of the matching triggers present in the input.</returns>
        private static List<Attachment> GetAttachments(string inputContainingTriggers)
        {
            if (string.IsNullOrEmpty(inputContainingTriggers))
            {
                return null;
            }

            // Set to lower case for easier matching of triggers.
            // This sample code assumes that the culture and language is English.
            inputContainingTriggers = inputContainingTriggers.ToLower();

            var cumulativeAttachments = new List<Attachment>();
            foreach (var trigger in SampleResponseProvider.TriggerHandler.Keys)
            {
                if (inputContainingTriggers.Contains(trigger))
                {
                    Trace.TraceInformation("Trigger found in user query.");
                    var attachmentsForTrigger = SampleResponseProvider.TriggerHandler[trigger](trigger);
                    cumulativeAttachments.AddRange(attachmentsForTrigger);
                }
            }

            return cumulativeAttachments;
        }

        /// <summary>
        /// Initializes the trigger handler with a method for each trigger word.
        /// </summary>
        /// <returns>The fully initialized trigger handler.</returns>
        private static Dictionary<string, Func<string, List<Attachment>>> InitializeTriggerHandler()
        {

            return new Dictionary<string, Func<string, List<Attachment>>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    TriggerIs(SampleResponseProvider.EverythingTrigger),
                    (input) =>
                    {
                        var results = new List<Attachment>();
                        foreach (var th in SampleResponseProvider.TriggerHandler)
                        {
                            if (th.Key != TriggerIs(SampleResponseProvider.EverythingTrigger))
                            {
                                results.AddRange(th.Value(input));
                            }
                        }
                        return results;
                    }
                },
                {
                    TriggerIs("adaptivecard"),
                    (input) => new List<Attachment>()
                    {
                        // Calling Adaptive Card with Image.
                        GetAdaptiveCardAttachment()
                    }
                },
                {
                    TriggerIs("hero"),
                    (input) => new List<Attachment>()
                    {
                        // Card with everything
                        GetHeroCardAttachment(
                            "Subject Title",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "See more like this", "/test ipsum" })
                    }
                },
                {
                    TriggerIs("richcard"),
                    (input) =>
                    {
                        var useThumbnail = false;
                        var useTitle = false;
                        var useSubtitle = false;
                        var useContent = false;
                        var useAttribution = false;
                        var useImage = false;
                        var useButtons = false;
                        var useTap = false;
                        var useHTML = false;

                        var args = input.Split((string[]) null, StringSplitOptions.RemoveEmptyEntries);

                        if( args.Length >= 3)
                        {
                            useTitle = args.Any("title".Equals);
                            useSubtitle = args.Any("subtitle".Equals);
                            useContent = args.Any("content".Equals);
                            useAttribution = args.Any("attr".Equals);
                            useImage = args.Any("image".Equals);
                            useButtons = args.Any("button".Equals);
                            useThumbnail = args.Any("thumbnail".Equals);
                            useTap = args.Any("tap".Equals);
                            useHTML = args.Any("html".Equals);
                        }
                        else
                        {
                            useTitle = useSubtitle = useContent = useAttribution = useImage = useButtons = true;
                        }

                        var title = useHTML ? "Subject <a href=\"http://www.bings.com\">Title</a>" : "Subject Title";
                        var subtitle = useHTML ? "<a href=\"http://yammer.com\">Subtitle</a> or breadcrumb" : "Subtitle or breadcrumb";
                        var content = useHTML ? "Some text before <a href=\"https://youtu.be/nKU-FMzZFF0\">content.</a> Some text after content." : "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim";

                        // Card with flexible fields
                        Attachment card;
                        if (useThumbnail)
                        {
                            card =
                            GetThumbnailCardAttachment(
                            useTitle ? title : null,
                            useSubtitle ? subtitle : null,
                            useContent ? content : null,
                            useAttribution ? "attribution" : null,
                            useImage ? new[] { "https://c.s-microsoft.com/en-us/CMSImages/PhoneChat-5_UPDATED.png?version=2a9fe070-700a-1138-b996-01adf54a57a3" } : null,
                            useButtons ? new[] { "View in article", "See more like this", "/test ipsum" } : null);
                            if (useTap)
                            {
                                var thumbnailCard = card.Content as ThumbnailCard;
                                if (thumbnailCard != null)
                                {
                                    thumbnailCard.Tap = new CardAction() { Title = "Open URL", Type = ActionTypes.OpenUrl, Value = "http://microsoft.com" };
                                }
                            }
                        }
                        else
                        {
                            card =
                            GetHeroCardAttachment(
                            useTitle ? title : null,
                            useSubtitle ? subtitle : null,
                            useContent ? content : null,
                            useAttribution ? "attribution" : null,
                            useImage ? new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" } : null,
                            useButtons ? new[] { "View in article", "See more like this", "/test ipsum" } : null);

                            if (useTap)
                            {
                                var heroCard = card.Content as HeroCard;
                                if (heroCard != null)
                                {
                                    heroCard.Tap = new CardAction() { Title = "Open URL", Type = ActionTypes.OpenUrl, Value = "http://microsoft.com" };
                                }
                            }
                        }

                        return new List<Attachment>() {card};
                    }
                },
                {
                    TriggerIs("img"),
                    (input) => new List<Attachment>()
                    {
                        GetHeroCardAttachment(
                            "Card with image containing no width or height",
                            null,
                            "<img src='https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6'/>",
                            null,
                            null,
                            new[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" })
                    }
                },
                {
                    TriggerIs("carousel1"),
                    (input) => new List<Attachment>()
                    {
                        // Card with only an image and 5 buttons
                        GetHeroCardAttachment(
                            null,
                            null,
                            null,
                            null,
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }),

                        // Card with only a title and 5 buttons
                        GetHeroCardAttachment(
                            "Subject Title Carousel 1",
                            null,
                            null,
                            null,
                            null,
                            new[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }),

                        // Card with no image but 5 buttons
                        GetHeroCardAttachment(
                            "Subject Title Carousel 1",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            null,
                            new[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5" }),

                        // Card with everything
                        GetHeroCardAttachment(
                            "Subject Title Carousel 1",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "See more like this", "Action 3" }),

                        // Card with text only and some buttons
                        GetHeroCardAttachment(
                            "Subject Title Carousel 1",
                            null,
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            null,
                            null,
                            new[] { "View in article", "See more like this", "Action 3" })
                    }
                },
                {
                    TriggerIs("carouselx"),
                    (input) =>
                    {
                        var card = GetHeroCardAttachment(
                            "Subject Title Carouselx",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5"});

                        var attachments = new List<Attachment>();

                        for (var i = 0; i < 5; i++)
                        {
                            attachments.Add(card);
                        }

                        return attachments;
                    }
                },
                {
                    TriggerIs("mixedcarousel"),
                    (input) =>
                    {
                        var card = GetHeroCardAttachment(
                            "Subject Title Mixed Carousel",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "See more like this", "Action 3", "Action 4"});

                        var attachments = new List<Attachment>();

                        for (var i = 0; i < 3; i++)
                        {
                            attachments.Add(card);
                        }

                        return attachments;
                    }
                },
                {
                    TriggerIs("mixedcard"),
                    (input) =>
                    {
                        var card = GetHeroCardAttachment(
                            "Subject Title Mixed Card",
                            "Subtitle or breadcrumb",
                            "Bacon ipsum dolor amet flank ground round chuck pork loin. Sirloin meatloaf boudin meatball ham hock shoulder capicola tri-tip sausage biltong cupim",
                            "attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "See more like this", "Action 3", "Action 4", "Action 5"});

                        return new List<Attachment>() { card };
                    }
                },
                {
                    TriggerIs("cardspecial"),
                    (input) =>
                    {
                        var card = GetHeroCardAttachment(
                            "Subject Title Cards Special",
                            "Subtitle or breadcrumb",
                            "That's cool. Isn't it?",
                            "attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "That's cool!", "Action 3", "Action 4", "Action 5"});

                        return new List<Attachment>() { card };
                    }
                },
                {
                    TriggerIs("thumbnail"),
                    (input) =>
                    {
                        var card = GetThumbnailCardAttachment(
                            "Sample Thumbnail Card",
                            "Sample Subtitle",
                            "1 Microsoft Way, Redmond, WA 98052<br />(425) 123-4567",
                            "Attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/PhoneChat-5_UPDATED.png?version=2a9fe070-700a-1138-b996-01adf54a57a3" },
                            new[] { "View in article", "See more like this" });

                        return new List<Attachment>() { card };
                    }
                },
                {
                    TriggerIs("carouselmix2"),
                    (input) =>
                    {
                        var thumbnailCard = GetThumbnailCardAttachment(
                            "Sample Thumbnail Card Carousel Mix 2",
                            "Sample Subtitle",
                            "1 Microsoft Way, Redmond, WA 98052<br />(425) 123-4567",
                            "Attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/PhoneChat-5_UPDATED.png?version=2a9fe070-700a-1138-b996-01adf54a57a3" },
                            new[] { "View in article", "See more like this" });

                        var heroCard = GetHeroCardAttachment(
                            "Subject Title Card Carousel Mix 2",
                            "Subtitle or breadcrumb",
                            "That's cool. Isn't it?",
                            "attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/DesktopContent-04_UPDATED.png?version=43c80870-99dd-7fb1-48c0-59aced085ab6" },
                            new[] { "View in article", "That's cool!", "Action 3", "Action 4"});

                        return new List<Attachment>() { thumbnailCard, heroCard };
                    }
                },
                {
                    TriggerIs("bingnews"),
                    (input) =>
                    {
                        var cards = new List<Attachment>
                        {
                            GetHeroCardAttachment("Bing news", "My interests", null, null, null, null)
                        };

                        var storyCard = GetThumbnailCardAttachment("Old Tech", "This is what old tech looks like", "Listicle ramp chambray humblebrag, pug scenester waistcoat tofu astopub swag. Cliche heirloom pitchfork, blue bottle <a href='https://bit.ly/2bNaRsa'>https://bit.ly/2bNaRsa</a>", "TechCrunch", new string[] {
                        }, null);

                        for (var i = 0; i < 5; i++)
                        {
                            cards.Add(storyCard);
                        }

                        cards.Add(GetHeroCardAttachment(null, null, null, null, null, new[] { "View more work items" }));

                        return cards;
                    }
                },
                {
                    TriggerIs("summary"),
                    (input) =>
                    {
                        var card = GetThumbnailCardAttachment(
                            "Sample Thumbnail Card Summary",
                            "Sample Subtitle",
                            "1 Microsoft Way, Redmond, WA 98052<br />(425) 123-4567",
                            "Attribution",
                            new[] { "https://c.s-microsoft.com/en-us/CMSImages/PhoneChat-5_UPDATED.png?version=2a9fe070-700a-1138-b996-01adf54a57a3" },
                            new[] { "View in article", "See more like this" });

                        return new List<Attachment>() { card };
                    }
                },
                {
                    TriggerIs("signin"),
                    (input) =>
                    {
                        var card =  new Attachment()
                        {
                            ContentType = SigninCard.ContentType,
                            Content = new SigninCard()
                            {
                                Text = "Time to sign in",
                                Buttons = new List<CardAction>()
                                {
                                    new CardAction()
                                    {
                                        Title = "Please sign in",
                                        Type = ActionTypes.OpenUrl,
                                        Value = "https://www.bing.com"
                                    }
                                }
                            }
                        };

                        return new List<Attachment>() { card };
                    }
                }
            };
        }

        /// <summary>
        /// Returns the trigger prefix for supplied keyword
        /// </summary>
        /// <param name="keyword">The keyword</param>
        /// <returns>Trigger string</returns>
        private static string TriggerIs(string keyword)
        {
            return keyword;
        }



        /// <summary>
        /// Builds and returns a <see cref="HeroCard"/> attachment using the supplied info
        /// </summary>
        /// <param name="title">Title of the card</param>
        /// <param name="subTitle">Subtitle of the card</param>
        /// <param name="text">Text of the card</param>
        /// <param name="attribution">Attribution of the card</param>
        /// <param name="images">Images in the card</param>
        /// <param name="buttons">Buttons in the card</param>
        /// <returns>Card attachment</returns>
        private static Attachment GetHeroCardAttachment(string title, string subTitle, string text, string attribution, string[] images, string[] buttons)
        {
            var heroCard = new HeroCard()
            {
                Title = title,
                Subtitle = subTitle,
                Text = text,
                Images = new List<CardImage>(),
                Buttons = new List<CardAction>(),
            };

            // Set images
            if (images != null)
            {
                foreach (var img in images)
                {
                    heroCard.Images.Add(new CardImage()
                    {
                        Url = img,
                        Alt = img,
                    });
                }
            }

            // Set buttons
            if (buttons != null)
            {
                foreach (var btn in buttons)
                {
                    heroCard.Buttons.Add(new CardAction()
                    {
                        Title = btn,
                        Type = ActionTypes.OpenUrl,
                        Value = btn,
                    });
                }
            }

            return new Attachment()
            {
                ContentType = HeroCard.ContentType,
                Content = heroCard,
            };
        }

        /// <summary>
        /// Builds and returns a <see cref="ThumbnailCard"/> attachment using the supplied info
        /// </summary>
        /// <param name="title">Title of the card</param>
        /// <param name="subTitle">Subtitle of the card</param>
        /// <param name="text">Text of the card</param>
        /// <param name="attribution">Attribution of the card</param>
        /// <param name="images">Images in the card</param>
        /// <param name="buttons">Buttons in the card</param>
        /// <returns>Card attachment</returns>
        private static Attachment GetThumbnailCardAttachment(string title, string subTitle, string text, string attribution, string[] images, string[] buttons)
        {
            var heroCard = new ThumbnailCard()
            {
                Title = title,
                Subtitle = subTitle,
                Text = text,
                Images = new List<CardImage>(),
                Buttons = new List<CardAction>(),
            };

            // Set images
            if (images != null)
            {
                foreach (var img in images)
                {
                    string altText = null;
                    if (img.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        altText = img;
                    }
                    else
                    {
                        altText = "The alt text for an image blob";
                    }

                    heroCard.Images.Add(new CardImage()
                    {
                        Url = img,
                        Alt = altText,
                    });
                }
            }

            // Set buttons
            if (buttons != null)
            {
                foreach (var btn in buttons)
                {
                    heroCard.Buttons.Add(new CardAction()
                    {
                        Title = btn,
                        Type = ActionTypes.OpenUrl,
                        Value = btn,
                    });
                }
            }

            return new Attachment()
            {
                ContentType = ThumbnailCard.ContentType,
                Content = heroCard,
            };
        }
    }
}
