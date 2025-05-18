# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from botbuilder.core import CardFactory
from botbuilder.schema import Attachment

def adaptiveCard() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {"type": "TextBlock", "text": "This is some **bold** text"},
                {"type": "TextBlock", "text": "This is some _italic_ text"},
                {"type": "TextBlock", "text": "- Bullet \r- List \r", "wrap": True},
                {"type": "TextBlock", "text": "1. Numbered\r2. List\r", "wrap": True},
                {
                    "type": "TextBlock",
                    "text": "Check out [Adaptive Cards](https://adaptivecards.io)",
                },
            ],
        }
    )

def adaptiveCardBorders() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Below is a **ColumnSet** with borders:",
                    "wrap": True,
                },
                {
                    "type": "ColumnSet",
                    "showBorder": True,
                    "style": "emphasis",
                    "columns": [
                        {
                            "type": "Column",
                            "width": "stretch",
                            "showBorder": True,
                            "style": "accent",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "This is a **Column** with borders",
                                    "wrap": True,
                                }
                            ],
                        },
                        {
                            "type": "Column",
                            "width": "stretch",
                            "style": "good",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "This is a **Column** without borders",
                                    "wrap": True,
                                }
                            ],
                        },
                    ],
                },
                {
                    "type": "Container",
                    "style": "attention",
                    "showBorder": True,
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "This is a **Container** with borders",
                            "wrap": True,
                        }
                    ],
                },
                {
                    "type": "Table",
                    "columns": [{"width": 1}, {"width": 1}],
                    "rows": [
                        {
                            "type": "TableRow",
                            "cells": [
                                {
                                    "type": "TableCell",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "text": "This **Table**...",
                                            "wrap": True,
                                        }
                                    ],
                                },
                                {
                                    "type": "TableCell",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "text": "...has borders",
                                            "wrap": True,
                                        }
                                    ],
                                },
                            ],
                        }
                    ],
                },
            ],
        }
    )

def adaptiveCardCompoundButton() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "CompoundButton",
                    "title": "Summarize",
                    "icon": {"name": "TextBulletList"},
                    "description": "Test Description",
                    "subTitle": "Review key points in file",
                    "height": "stretch",
                    "badge": "New",
                }
            ],
        }
    )

def adaptiveCardConditional() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "Input.Text",
                    "placeholder": "Placeholder text",
                    "label": "Required text input",
                    "isRequired": True,
                    "id": "text",
                },
                {
                    "type": "Input.Date",
                    "label": "Required date input",
                    "isRequired": True,
                    "id": "date",
                },
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Action.Submit",
                    "conditionallyEnabled": True,
                },
                {
                    "type": "Action.Submit",
                    "title": "Permanently disabled button",
                    "isEnabled": False,
                },
            ],
        }
    )

def adaptiveCardContainerLayouts() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "layouts": [
                {
                    "type": "Layout.AreaGrid",
                    "targetWidth": "atLeast:standard",
                    "columns": [60],
                    "areas": [{"name": "imageArea"}, {"name": "textArea", "column": 2}],
                }
            ],
            "body": [
                {
                    "type": "Image",
                    "url": "https://picsum.photos/200/200?image=110",
                    "grid.area": "imageArea",
                    "style": "RoundedCorners",
                    "targetWidth": "atLeast:narrow",
                },
                {
                    "type": "Container",
                    "grid.area": "textArea",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Container layout",
                            "wrap": True,
                            "size": "ExtraLarge",
                            "weight": "Bolder",
                        },
                        {
                            "type": "TextBlock",
                            "text": "The simple, lightweight card format to power your ideas.",
                            "wrap": True,
                        },
                    ],
                },
            ],
        }
    )

def adaptiveCardDonutChart() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "Chart.Donut",
                    "data": [
                        {"legend": "Pear", "value": 59},
                        {"legend": "Banana", "value": 292},
                        {"legend": "Apple", "value": 143},
                        {"legend": "Peach", "value": 98},
                        {"legend": "Kiwi", "value": 179},
                        {"legend": "Grapefruit", "value": 50},
                        {"legend": "Orange", "value": 212},
                        {"legend": "Cantaloupe", "value": 68},
                    ],
                }
            ],
        }
    )

def adaptiveCardFluentIcon() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "speak": "3 minute energy flow with kayo video",
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "Image",
                    "url": "https://raw.githubusercontent.com/OfficeDev/Microsoft-Teams-Card-Samples/main/samples/author-highlight-video/assets/video_image.png",
                    "altText": "3 Minute Energy Flow with Kayo Video",
                },
                {
                    "type": "TextBlock",
                    "text": "3 Minute Energy Flow with Kayo",
                    "wrap": True,
                    "size": "Large",
                    "weight": "Bolder",
                },
                {
                    "type": "ColumnSet",
                    "columns": [
                        {
                            "type": "Column",
                            "width": "stretch",
                            "items": [
                                {
                                    "type": "Icon",
                                    "name": "Calendar",
                                    "size": "Medium",
                                    "style": "Filled",
                                    "color": "Accent",
                                    "selectAction": {"type": "Action.OpenUrl"},
                                }
                            ],
                            "spacing": "Small",
                            "verticalContentAlignment": "Center",
                        }
                    ],
                },
            ],
        }
    )

def adaptiveCardGaugeChart() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {"type": "TextBlock", "text": "Basic", "size": "extraLarge"},
                {
                    "type": "Chart.Gauge",
                    "value": 50,
                    "segments": [
                        {"legend": "Low risk", "size": 33, "color": "good"},
                        {"legend": "Medium risk", "size": 34, "color": "warning"},
                        {"legend": "High risk", "size": 33, "color": "attention"},
                    ],
                },
                {
                    "type": "TextBlock",
                    "text": "Single value",
                    "size": "extraLarge",
                    "spacing": "large",
                    "separator": True,
                },
                {
                    "type": "Chart.Gauge",
                    "value": 35,
                    "valueFormat": "fraction",
                    "segments": [
                        {"legend": "Used", "size": 35},
                        {"legend": "Unused", "size": 65, "color": "neutral"},
                    ],
                },
            ],
        }
    )

def adaptiveCardHorizontalBarChart() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Standard with displayMode = AbsoluteWithAxis",
                    "size": "large",
                },
                {
                    "type": "Chart.HorizontalBar",
                    "title": "Sample",
                    "xAxisTitle": "Days",
                    "yAxisTitle": "Sales",
                    "colorSet": "diverging",
                    "data": [
                        {"x": "Pear", "y": 59},
                        {"x": "Banana", "y": 292},
                        {"x": "Apple", "y": 143},
                        {"x": "Peach", "y": 98},
                        {"x": "Kiwi", "y": 179},
                        {"x": "Grapefruit", "y": 20},
                    ],
                },
                {
                    "type": "TextBlock",
                    "text": "Standard with displayMode = AbsoluteNoAxis",
                    "size": "large",
                    "spacing": "large",
                    "separator": True,
                },
                {
                    "type": "Chart.HorizontalBar",
                    "title": "Sample",
                    "displayMode": "AbsoluteNoAxis",
                    "data": [
                        {"x": "Pear", "y": 59},
                        {"x": "Banana", "y": 292},
                        {"x": "Apple", "y": 143},
                        {"x": "Peach", "y": 98},
                        {"x": "Kiwi", "y": 179},
                        {"x": "Grapefruit", "y": 20},
                    ],
                },
                {
                    "type": "TextBlock",
                    "text": "Standard with displayMode = PartToWhole",
                    "size": "large",
                    "spacing": "large",
                    "separator": True,
                },
                {
                    "type": "TextBlock",
                    "text": "Learning - Have you defined your goal for today's Day of learning",
                },
                {
                    "type": "Chart.HorizontalBar",
                    "title": "Sample",
                    "displayMode": "PartToWhole",
                    "color": "categoricalPurple",
                    "data": [
                        {"x": "Yes, I have defined my day of learning goal", "y": 15},
                        {"x": "No, I haven't yet had time to do it", "y": 24},
                        {"x": "I am not interested in learning", "y": 2},
                    ],
                },
            ],
        }
    )

def adaptiveCardHorizontalBarStacked() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "Chart.HorizontalBar.Stacked",
                    "title": "Sample",
                    "data": [
                        {
                            "title": "Outlook",
                            "data": [
                                {"legend": "2023-05-01", "value": 24, "color": "good"},
                                {
                                    "legend": "2023-05-02",
                                    "value": 27,
                                    "color": "warning",
                                },
                                {
                                    "legend": "2023-05-03",
                                    "value": 18,
                                    "color": "attention",
                                },
                            ],
                        },
                        {
                            "title": "Teams",
                            "data": [
                                {"legend": "2023-05-01", "value": 9, "color": "good"},
                                {
                                    "legend": "2023-05-02",
                                    "value": 100,
                                    "color": "warning",
                                },
                                {
                                    "legend": "2023-05-03",
                                    "value": 22,
                                    "color": "attention",
                                },
                            ],
                        },
                    ],
                }
            ],
        }
    )

def adaptiveCardLineChart() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "Chart.Line",
                    "title": "Sample",
                    "xAxisTitle": "Days",
                    "yAxisTitle": "Sales",
                    "colorSet": "categorical",
                    "data": [
                        {
                            "legend": "Outlook",
                            "values": [
                                {"x": "2023-05-01", "y": 99},
                                {"x": "2023-05-02", "y": 6},
                                {"x": "2023-05-03", "y": 63},
                                {"x": "2023-05-04", "y": 64},
                                {"x": "2023-05-05", "y": 63},
                                {"x": "2023-05-06", "y": 78},
                            ],
                        },
                        {
                            "legend": "Teams",
                            "values": [
                                {"x": "2023-05-01", "y": 12},
                                {"x": "2023-05-02", "y": 82},
                                {"x": "2023-05-03", "y": 12},
                                {"x": "2023-05-04", "y": 33},
                                {"x": "2023-05-05", "y": 1},
                                {"x": "2023-05-06", "y": 80},
                            ],
                        },
                        {
                            "legend": "Office",
                            "values": [
                                {"x": "2023-05-01", "y": 66},
                                {"x": "2023-05-02", "y": 93},
                                {"x": "2023-05-03", "y": 65},
                                {"x": "2023-05-04", "y": 13},
                                {"x": "2023-05-05", "y": 90},
                                {"x": "2023-05-06", "y": 48},
                            ],
                        },
                        {
                            "legend": "Windows",
                            "values": [
                                {"x": "2023-05-01", "y": 9},
                                {"x": "2023-05-02", "y": 19},
                                {"x": "2023-05-03", "y": 0},
                                {"x": "2023-05-04", "y": 61},
                                {"x": "2023-05-05", "y": 21},
                                {"x": "2023-05-06", "y": 72},
                            ],
                        },
                        {
                            "legend": "Exchange",
                            "values": [
                                {"x": "2023-05-01", "y": 35},
                                {"x": "2023-05-02", "y": 11},
                                {"x": "2023-05-03", "y": 91},
                                {"x": "2023-05-04", "y": 97},
                                {"x": "2023-05-05", "y": 97},
                                {"x": "2023-05-06", "y": 45},
                            ],
                        },
                        {
                            "legend": "SharePoint",
                            "values": [
                                {"x": "2023-05-01", "y": 26},
                                {"x": "2023-05-02", "y": 99},
                                {"x": "2023-05-03", "y": 16},
                                {"x": "2023-05-04", "y": 26},
                                {"x": "2023-05-05", "y": 91},
                                {"x": "2023-05-06", "y": 22},
                            ],
                        },
                        {
                            "legend": "Copilot",
                            "values": [
                                {"x": "2023-05-01", "y": 96},
                                {"x": "2023-05-02", "y": 37},
                                {"x": "2023-05-03", "y": 27},
                                {"x": "2023-05-04", "y": 5},
                                {"x": "2023-05-05", "y": 45},
                                {"x": "2023-05-06", "y": 59},
                            ],
                        },
                    ],
                }
            ],
        }
    )

def adaptiveCardMediaElements() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.6",
            "fallbackText": "This card requires CaptionSource to be viewed. Ask your platform to update to Adaptive Cards v1.6 for this and more!",
            "body": [
                {"type": "TextBlock", "text": "YouTube video", "wrap": True},
                {
                    "type": "Media",
                    "poster": "https://adaptivecards.io/content/poster-video.png",
                    "sources": [
                        {
                            "mimeType": "video/mp4",
                            "url": "https://www.youtube.com/watch?v=S7xTBa93TX8",
                        }
                    ],
                },
                {"type": "TextBlock", "text": "Vimeo video", "wrap": True},
                {
                    "type": "Media",
                    "poster": "https://adaptivecards.io/content/poster-video.png",
                    "sources": [
                        {"mimeType": "video/mp4", "url": "https://vimeo.com/508683403"}
                    ],
                },
                {"type": "TextBlock", "text": "Dailymotion video", "wrap": True},
                {
                    "type": "Media",
                    "poster": "https://adaptivecards.io/content/poster-video.png",
                    "sources": [
                        {
                            "mimeType": "video/mp4",
                            "url": "https://www.dailymotion.com/video/x8wi5ho",
                        }
                    ],
                },
            ],
        }
    )

def adaptiveCardPieChart() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "Chart.Pie",
                    "colorSet": "categorical",
                    "data": [
                        {"legend": "Pear", "value": 59},
                        {"legend": "Banana", "value": 292},
                        {"legend": "Apple", "value": 143},
                        {"legend": "Peach", "value": 98},
                        {"legend": "Kiwi", "value": 179},
                        {"legend": "Grapefruit", "value": 50},
                        {"legend": "Orange", "value": 212},
                        {"legend": "Cantaloupe", "value": 68},
                    ],
                }
            ],
        }
    )

def AdaptiveCardResponsiveLayout() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "ColumnSet",
                    "columns": [
                        {
                            "type": "Column",
                            "targetWidth": "atLeast:narrow",
                            "items": [
                                {
                                    "type": "Image",
                                    "style": "Person",
                                    "url": "https://aka.ms/AAp9xo4",
                                    "size": "Small",
                                }
                            ],
                            "width": "auto",
                        },
                        {
                            "type": "Column",
                            "spacing": "medium",
                            "verticalContentAlignment": "center",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "weight": "Bolder",
                                    "text": "David Claux",
                                    "wrap": True,
                                },
                                {
                                    "type": "TextBlock",
                                    "targetWidth": "atMost:narrow",
                                    "spacing": "None",
                                    "text": "Platform Architect",
                                    "isSubtle": True,
                                    "wrap": True,
                                },
                            ],
                            "width": "auto",
                        },
                        {
                            "type": "Column",
                            "targetWidth": "atLeast:standard",
                            "spacing": "medium",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "Platform Architect",
                                    "isSubtle": True,
                                    "wrap": True,
                                }
                            ],
                            "width": "stretch",
                            "verticalContentAlignment": "center",
                        },
                    ],
                }
            ],
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
        }
    )

def adaptiveCardRoundedCorners() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Below is a **ColumnSet** with rounded corners:",
                    "wrap": True,
                },
                {
                    "type": "ColumnSet",
                    "roundedCorners": True,
                    "style": "emphasis",
                    "columns": [
                        {
                            "type": "Column",
                            "width": "stretch",
                            "roundedCorners": True,
                            "showBorder": True,
                            "style": "accent",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "This is a **Column** with rounded corners",
                                    "wrap": True,
                                }
                            ],
                        },
                        {
                            "type": "Column",
                            "width": "stretch",
                            "showBorder": True,
                            "style": "good",
                            "items": [
                                {
                                    "type": "TextBlock",
                                    "text": "This is another **Column** without rounded corners",
                                    "wrap": True,
                                }
                            ],
                        },
                    ],
                },
                {
                    "type": "Container",
                    "style": "attention",
                    "roundedCorners": True,
                    "showBorder": True,
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "This is a **Container** with rounded corners",
                            "wrap": True,
                        }
                    ],
                },
                {
                    "type": "Table",
                    "style": "accent",
                    "showGridLines": True,
                    "roundedCorners": True,
                    "columns": [{"width": 1}, {"width": 1}],
                    "rows": [
                        {
                            "type": "TableRow",
                            "cells": [
                                {
                                    "type": "TableCell",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "text": "This **Table**...",
                                            "wrap": True,
                                        }
                                    ],
                                },
                                {
                                    "type": "TableCell",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "text": "...has rounded corners",
                                            "wrap": True,
                                        }
                                    ],
                                },
                            ],
                        }
                    ],
                },
                {
                    "type": "TextBlock",
                    "text": "The below **Image** has rounded corners:",
                    "wrap": True,
                },
                {
                    "type": "Image",
                    "url": "https://picsum.photos/200/200?image=110",
                    "width": "100px",
                    "style": "RoundedCorners",
                },
            ],
        }
    )

def adaptiveCardScrollable() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Below is a scrollable container",
                    "wrap": True,
                    "size": "ExtraLarge",
                    "weight": "Bolder",
                },
                {
                    "type": "Container",
                    "style": "emphasis",
                    "showBorder": True,
                    "maxHeight": "200px",
                    "items": [
                        {"type": "TextBlock", "text": "Item 1", "size": "ExtraLarge"},
                        {"type": "TextBlock", "text": "Item 2", "size": "ExtraLarge"},
                        {"type": "TextBlock", "text": "Item 3", "size": "ExtraLarge"},
                        {"type": "TextBlock", "text": "Item 4", "size": "ExtraLarge"},
                        {"type": "TextBlock", "text": "Item 5", "size": "ExtraLarge"},
                        {"type": "TextBlock", "text": "Item 6", "size": "ExtraLarge"},
                        {"type": "TextBlock", "text": "Item 7", "size": "ExtraLarge"},
                    ],
                },
            ],
            "actions": [{"type": "Action.OpenUrl", "title": "Click me"}],
        }
    )

def adaptiveCardStarRatings() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "body": [
                {"type": "TextBlock", "size": "Large", "text": "Rating input"},
                {
                    "type": "Input.Rating",
                    "id": "rating1",
                    "label": "Pick a rating",
                    "size": "medium",
                    "isRequired": True,
                    "errorMessage": "Please pick a rating",
                },
                {
                    "type": "Input.Rating",
                    "id": "rating2",
                    "label": "Pick a rating",
                    "allowHalfSteps": True,
                    "size": "large",
                    "isRequired": True,
                    "errorMessage": "Please pick a rating",
                    "color": "marigold",
                    "value": 3,
                },
                {
                    "type": "TextBlock",
                    "size": "large",
                    "text": "Read-only ratings",
                    "separator": True,
                    "spacing": "extraLarge",
                },
                {"type": "Rating", "value": 3.2, "size": "medium"},
                {"type": "Rating", "max": 20, "value": 3.2, "color": "marigold"},
                {
                    "type": "Rating",
                    "style": "compact",
                    "value": 3.2,
                    "color": "marigold",
                    "count": 150,
                },
            ],
            "actions": [{"type": "Action.Submit", "title": "Submit"}],
        }
    )

def adaptiveCardVerticalBarChart() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "Chart.VerticalBar",
                    "title": "Sample",
                    "xAxisTitle": "Days",
                    "yAxisTitle": "Sales",
                    "colorSet": "categorical",
                    "data": [
                        {"x": "Pear", "y": 59},
                        {"x": "Banana", "y": 292},
                        {"x": "Apple", "y": 143},
                        {"x": "Peach", "y": 98},
                        {"x": "Kiwi", "y": 179},
                        {"x": "Grapefruit", "y": 20},
                        {"x": "Orange", "y": 212},
                        {"x": "Cantaloupe", "y": 68},
                        {"x": "Grape", "y": 102},
                        {"x": "Tangerine", "y": 38},
                    ],
                }
            ],
        }
    )

def adaptiveCardVerticalBarGroupedChart() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Grouped",
                    "size": "large",
                    "separator": True,
                    "spacing": "large",
                },
                {
                    "type": "Chart.VerticalBar.Grouped",
                    "title": "Sample",
                    "xAxisTitle": "Days",
                    "yAxisTitle": "Sales",
                    "colorSet": "diverging",
                    "data": [
                        {
                            "legend": "Outlook",
                            "values": [
                                {"x": "2023-05-01", "y": 24},
                                {"x": "2023-05-02", "y": 27},
                                {"x": "2023-05-03", "y": 18},
                                {"x": "2023-05-04", "y": 30},
                                {"x": "2023-05-05", "y": 20},
                                {"x": "2023-05-06", "y": 35},
                                {"x": "2023-05-07", "y": 40},
                                {"x": "2023-05-08", "y": 45},
                            ],
                        },
                        {
                            "legend": "Teams",
                            "values": [
                                {"x": "2023-05-01", "y": 9},
                                {"x": "2023-05-02", "y": 100},
                                {"x": "2023-05-03", "y": 22},
                                {"x": "2023-05-04", "y": 40},
                                {"x": "2023-05-05", "y": 30},
                                {"x": "2023-05-06", "y": 45},
                                {"x": "2023-05-07", "y": 50},
                                {"x": "2023-05-08", "y": 55},
                            ],
                        },
                        {
                            "legend": "Office",
                            "values": [
                                {"x": "2023-05-01", "y": 10},
                                {"x": "2023-05-02", "y": 20},
                                {"x": "2023-05-03", "y": 30},
                                {"x": "2023-05-04", "y": 40},
                                {"x": "2023-05-05", "y": 50},
                                {"x": "2023-05-06", "y": 60},
                                {"x": "2023-05-07", "y": 70},
                                {"x": "2023-05-08", "y": 80},
                            ],
                        },
                        {
                            "legend": "Windows",
                            "values": [
                                {"x": "2023-05-01", "y": 10},
                                {"x": "2023-05-02", "y": 20},
                                {"x": "2023-05-03", "y": 30},
                                {"x": "2023-05-04", "y": 40},
                                {"x": "2023-05-05", "y": 50},
                                {"x": "2023-05-06", "y": 60},
                                {"x": "2023-05-07", "y": 70},
                                {"x": "2023-05-08", "y": 80},
                            ],
                        },
                    ],
                },
                {
                    "type": "TextBlock",
                    "text": "Stacked",
                    "size": "large",
                    "separator": True,
                    "spacing": "large",
                },
                {
                    "type": "Chart.VerticalBar.Grouped",
                    "stacked": True,
                    "title": "Sample",
                    "xAxisTitle": "Days",
                    "yAxisTitle": "Sales",
                    "data": [
                        {
                            "legend": "Outlook",
                            "values": [
                                {"x": "2023-05-01", "y": 24},
                                {"x": "2023-05-02", "y": 27},
                                {"x": "2023-05-03", "y": 18},
                                {"x": "2023-05-04", "y": 30},
                                {"x": "2023-05-05", "y": 20},
                                {"x": "2023-05-06", "y": 35},
                                {"x": "2023-05-07", "y": 40},
                                {"x": "2023-05-08", "y": 45},
                            ],
                        },
                        {
                            "legend": "Teams",
                            "values": [
                                {"x": "2023-05-01", "y": 9},
                                {"x": "2023-05-02", "y": 100},
                                {"x": "2023-05-03", "y": 22},
                                {"x": "2023-05-04", "y": 40},
                                {"x": "2023-05-05", "y": 30},
                                {"x": "2023-05-06", "y": 45},
                                {"x": "2023-05-07", "y": 50},
                                {"x": "2023-05-08", "y": 55},
                            ],
                        },
                        {
                            "legend": "Office",
                            "values": [
                                {"x": "2023-05-01", "y": 10},
                                {"x": "2023-05-02", "y": 20},
                                {"x": "2023-05-03", "y": 30},
                                {"x": "2023-05-04", "y": 40},
                                {"x": "2023-05-05", "y": 50},
                                {"x": "2023-05-06", "y": 60},
                                {"x": "2023-05-07", "y": 70},
                                {"x": "2023-05-08", "y": 80},
                            ],
                        },
                        {
                            "legend": "Windows",
                            "values": [
                                {"x": "2023-05-01", "y": 10},
                                {"x": "2023-05-02", "y": 20},
                                {"x": "2023-05-03", "y": 30},
                                {"x": "2023-05-04", "y": 40},
                                {"x": "2023-05-05", "y": 50},
                                {"x": "2023-05-06", "y": 60},
                                {"x": "2023-05-07", "y": 70},
                                {"x": "2023-05-08", "y": 80},
                            ],
                        },
                    ],
                },
            ],
        }
    )

def adaptiveCardWithEmoji() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Adaptive Card with emojis",
                    "size": "Large",
                    "weight": "Bolder",
                },
                {
                    "type": "Container",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Publish Adaptive Card with emojis 🥰 ",
                            "weight": "bolder",
                            "size": "medium",
                        }
                    ],
                },
            ],
        }
    )

def adaptivePeoplePersonaCardIcon() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.6",
            "body": [
                {"type": "TextBlock", "text": "Persona", "weight": "bolder"},
                {
                    "type": "Component",
                    "name": "graph.microsoft.com/user",
                    "view": "compact",
                    "properties": {
                        "id": "{{User-Object-ID}}",
                        "displayName": "{{User-Display-Name}}",
                        "userPrincipalName": "{{User-Principal-Name}}",
                    },
                },
            ],
        }
    )

def adaptivePeoplePersonaCardSetIcon() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.6",
            "body": [
                {"type": "TextBlock", "text": "Persona Set", "weight": "bolder"},
                {
                    "type": "Component",
                    "name": "graph.microsoft.com/users",
                    "view": "compact",
                    "properties": {
                        "users": [
                            {
                                "id": "{{User-Object-ID}}",
                                "displayName": "{{User-Display-Name}}",
                                "userPrincipalName": "{{User-Principal-Name}}",
                            },
                            {
                                "id": "{{User-Object-ID}}",
                                "displayName": "{{User-Display-Name}}",
                                "userPrincipalName": "{{User-Principal-Name}}",
                            },
                        ]
                    },
                },
            ],
        }
    )

def codeBlocksCard() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {"type": "TextBlock", "text": "editor.js", "style": "heading"},
                {"type": "TextBlock", "text": "Lines 61 - 76"},
                {
                    "type": "CodeBlock",
                    "codeSnippet": '/**\n* @author John Smith <john.smith@example.com>\n*/\npackage l2f.gameserver.model;\n\npublic abstract strictfp class L2Char extends L2Object {\n  public static final Short ERROR = 0x0001;\n\n  public void moveTo(int x, int y, int z) {\n    _ai = null;\n    log("Should not be called");\n    if (1 > 5) { // what!?\n      return;\n    }\n  }\n}',
                    "language": "java",
                    "startLineNumber": 61,
                },
            ],
        }
    )

def formatHTMLConnectorCard() -> Attachment:
    return {
        "@type": "MessageCard",
        "@context": "https://schema.org/extensions",
        "summary": "Summary",
        "title": "Connector Card HTML formatting",
        "sections": [
            {"text": "This is some <strong>bold</strong> text"},
            {"text": "This is some <em>italic</em> text"},
            {"text": "This is some <strike>strikethrough</strike> text"},
            {"text": "<h1>Header 1</h1>\r<h2>Header 2</h2>\r <h3>Header 3</h3>"},
            {"text": "bullet list <ul><li>text</li><li>text</li></ul>"},
            {"text": "ordered list <ol><li>text</li><li>text</li></ol>"},
            {"text": 'hyperlink <a href="https://www.bing.com/">Bing</a>'},
            {
                "text": 'embedded image <img src="https://aka.ms/Fo983c" alt="Duck on a rock"></img>'
            },
            {"text": "preformatted text <pre>text</pre>"},
            {"text": "Paragraphs <p>Line a</p><p>Line b</p>"},
            {"text": "<blockquote>Blockquote text</blockquote>"},
        ],
    }

def informationMaskingCard() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "weight": "bolder",
                    "size": "large",
                    "text": "Information Masking Card",
                },
                {"type": "TextBlock", "text": "Enter Password"},
                {"type": "Input.Text", "id": "secretThing", "style": "password"},
                {"type": "TextBlock", "text": "Re-type Password", "wrap": True},
                {"type": "Input.Text", "id": "secretThing", "style": "password"},
            ],
        }
    )

def mentionSupport() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "weight": "bolder",
                    "size": "large",
                    "text": "Mention Support Card",
                },
                {
                    "type": "TextBlock",
                    "text": "Hi <at>{{New-Ids}}</at>, <at>{{New-Ids}}</at>",
                },
            ],
            "msteams": {
                "entities": [
                    {
                        "type": "mention",
                        "text": "<at>{{New-Ids}}</at>",
                        "mentioned": {"id": "{{Email-Id}}", "name": "{{New-Ids}}"},
                    },
                    {
                        "type": "mention",
                        "text": "<at>{{New-Ids}}</at>",
                        "mentioned": {
                            "id": "{{Microsoft-App-Id}}",
                            "name": "{{New-Ids}}",
                        },
                    },
                ]
            },
        }
    )

def overflowMenu() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Overflow Menu Card",
                    "size": "Large",
                    "weight": "Bolder",
                }
            ],
            "actions": [
                {"type": "Action.Submit", "title": "Set due date"},
                {
                    "type": "Action.OpenUrl",
                    "title": "View",
                    "url": "https://adaptivecards.io",
                },
                {"type": "Action.Submit", "title": "Delete", "mode": "secondary"},
            ],
        }
    )

def sampleAdaptiveWithFullWidth() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "Container",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Sample Adaptive Card with full width",
                            "size": "Large",
                            "weight": "Bolder",
                        }
                    ],
                },
                {
                    "type": "Input.ChoiceSet",
                    "label": "Select a user",
                    "id": "fullwidthcard",
                    "isMultiSelect": False,
                    "choices": [
                        {"title": "User 1", "value": "User1"},
                        {"title": "User 2", "value": "User2"},
                    ],
                    "style": "filtered",
                },
                {
                    "type": "Input.ChoiceSet",
                    "id": "myColor2",
                    "style": "expanded",
                    "label": "What color do you want? (isMultiSelect:False, style:expanded)",
                    "isMultiSelect": False,
                    "value": "1",
                    "choices": [
                        {"title": "Red", "value": "1"},
                        {"title": "Green", "value": "2"},
                        {"title": "Blue", "value": "3"},
                    ],
                },
            ],
            "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.5",
            "msteams": {"width": "Full"},
        }
    )

def stageViewForImages() -> Attachment:
    return CardFactory.adaptive_card(
        {
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Stage View Images",
                    "size": "Large",
                    "weight": "Bolder",
                },
                {
                    "type": "Image",
                    "url": "https://picsum.photos/200/200?image=110",
                    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                    "version": "1.5",
                    "msTeams": {"allowExpand": True},
                },
            ],
        }
    )
