# Bot Formatting Cards - Teams SDK Implementation

## Overview
This bot demonstrates various Adaptive Card formatting styles and features in Microsoft Teams using the **Teams SDK (Microsoft.Teams.Api 2.0)**. It showcases 27 different types of adaptive cards including charts, layouts, interactive elements, and modern UI components.

## Features
* **Teams SDK 2.0** - Uses the latest Microsoft Teams SDK for C# (not Bot Framework)
* **27 Adaptive Card Types** - Comprehensive demonstration of card formatting capabilities
* **Interactive Selection** - Text-based card selection system
* **Modern UI** - Leverages Fluent UI components, borders, rounded corners, and more
* **Data Visualization** - Multiple chart types including donut, gauge, pie, bar, and line charts

## Included Card Types

### Basic Formatting
- **MentionSupport** - Demonstrates @mention functionality
- **InfoMasking** - Shows password/sensitive information masking
- **FullWidthCard** - Full-width card layout
- **CardWithEmoji** - Emoji support in cards
- **HTMLConnector** - HTML connector card formatting

### Visual Elements
- **StageViewImages** - Stage view for images
- **OverflowMenu** - Overflow menu implementation
- **Persona** - Single user persona icon
- **PersonaSet** - Multiple user persona icons
- **FluentIcons** - Fluent UI icon integration
- **MediaElements** - Audio/video media elements

### Layout & Styling
- **Layout** - Responsive layout with targetWidth
- **Border** - Card border styling
- **RoundedCorners** - Rounded corner elements
- **ContainerLayout** - Various container layouts
- **ScrollableCard** - Scrollable container for long content

### Interactive Elements
- **ConditionalCard** - Conditional button enablement
- **CompoundButton** - Compound button controls
- **StarRatings** - Star rating input and display

### Data Visualization
- **DonutChart** - Donut chart visualization
- **GaugeChart** - Gauge chart display
- **HorizontalChart** - Horizontal bar chart
- **HorizontalStacked** - Stacked horizontal bar chart
- **LineChart** - Line chart visualization
- **PieChart** - Pie chart display
- **VerticalBarChart** - Vertical bar chart
- **VerticalGroupedChart** - Grouped vertical bar chart

## Architecture

### Teams SDK Approach
This implementation uses the **Microsoft Teams SDK 2.0** which provides:
- `Microsoft.Teams.Api` - Core Teams API functionality
- `Microsoft.Teams.Apps` - Application framework
- `Microsoft.Teams.Cards` - Type-safe card builders with IntelliSense
- Event-driven architecture with attributes (`[Message]`, `[Conversation.MembersAdded]`, etc.)

### Key Components

#### Controller.cs
Uses Teams SDK event handlers:
```csharp
[TeamsController]
public class Controller
{
    [Message] // Handles incoming messages
    public async Task OnMessage([Context] MessageActivity activity, [Context] IContext.Client client)
    
    [Conversation.MembersAdded] // Handles new members
    public async Task OnMembersAdded(IContext<ConversationUpdateActivity> context)
    
    [AdaptiveCard.Action] // Handles card actions
    public async Task OnAdaptiveCardAction([Context] AdaptiveCard.ActionActivity activity)
}
```

#### AllCards.cs
Helper class that:
- Loads adaptive card JSON templates from Resources folder
- Deserializes JSON into `Microsoft.Teams.Cards.AdaptiveCard` objects
- Uses `AdaptiveCards.Templating` for dynamic data binding
- Returns type-safe card objects for Teams SDK

#### Resources Folder
Contains 28 JSON files with adaptive card definitions following the Adaptive Cards 1.5 schema.

## How It Works

1. **Welcome Message**: When a user joins, they receive a welcome message
2. **Card Selection**: User types anything to see the card selection menu
3. **Display Card**: User types a card name (e.g., "DonutChart") to view that specific card
4. **Interactive Feedback**: Some cards (like StarRatings) support user input and feedback

## Usage

### Running the Bot
1. Configure `appsettings.json` with your bot credentials:
   - ClientId
   - ClientSecret (if using Client Secret authentication)
   - BotType ("UserAssignedMsi" or other)

2. Run the application

3. Add the bot to Teams using the app package in M365Agent folder

### Viewing Cards
Simply type any card name from the list:
```
MentionSupport
InfoMasking
DonutChart
StarRatings
FluentIcons
... etc
```

## Differences from Bot Framework

This implementation **does not use** the traditional Bot Framework SDK. Instead it uses the modern Teams SDK which:

### ✅ Uses
- `Microsoft.Teams.Api` and `Microsoft.Teams.Apps`
- Attribute-based event handlers
- `IContext.Client` for sending messages
- Type-safe card builders from `Microsoft.Teams.Cards`

### ❌ Does Not Use
- `Microsoft.Bot.Builder`
- `BotAdapter` or `CloudAdapter`
- `ITurnContext`
- `DialogBot` or `ComponentDialog`
- `BotController` with HTTP POST endpoints

## Configuration

The bot uses the Teams SDK's built-in authentication and configuration:
- No manual adapter configuration needed
- Automatic credential management
- Simplified dependency injection with `builder.AddTeams()`

## Development

### Adding New Cards
1. Create adaptive card JSON in `Resources` folder
2. Add a new method in `AllCards.cs`:
   ```csharp
   public static AdaptiveCard SendMyNewCard()
   {
       var json = File.ReadAllText(Path.Combine(".", "Resources", "myNewCard.json"));
       return JsonSerializer.Deserialize<AdaptiveCard>(json) ?? new AdaptiveCard();
   }
   ```
3. Add case in `Controller.GetCardByName()` switch statement
4. Add card name to the selection menu in `SendCardSelectionMenu()`

## References
- [Teams SDK Documentation](https://learn.microsoft.com/en-us/microsoftteams/platform/teams-ai-library/)
- [Teams SDK C# Guide](https://learn.microsoft.com/en-us/microsoftteams/platform/teams-ai-library/csharp/)
- [Adaptive Cards Documentation](https://adaptivecards.microsoft.com/)
- [Adaptive Cards C# SDK](https://learn.microsoft.com/en-us/microsoftteams/platform/teams-ai-library/csharp/in-depth-guides/adaptive-cards/overview)

## License
Copyright(c) Microsoft. All Rights Reserved.
Licensed under the MIT License.
