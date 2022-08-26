// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.ConversationalTabs.Web.Bot;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Models;
using Microsoft.Teams.Samples.ConversationalTabs.Domain.Services;

/// <summary>
/// Teams Bot Activity Handler.
/// </summary>
public class BotActivityHandler : TeamsActivityHandler
{
    private readonly IRepositoryObjectService<MsTeamsBotData, MsTeamsBotData> _proactiveBotDataService;

    private readonly ILogger<BotActivityHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BotActivityHandler"/> class.
    /// </summary>
    /// <param name="proactiveBotDataService"></param>
    /// <param name="logger">Logger.</param>
    public BotActivityHandler(IRepositoryObjectService<MsTeamsBotData, MsTeamsBotData> proactiveBotDataService, ILogger<BotActivityHandler> logger)
    {
        _proactiveBotDataService = proactiveBotDataService;
        _logger = logger;
    }

    /// <summary>
    /// Called when a message is sent to this bot.
    /// This bot will echo back the incoming message if it begins with 'echo'
    /// </summary>
    /// <param name="turnContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        turnContext.Activity.RemoveRecipientMention();
        var text = turnContext.Activity.Text.Trim();

        var echoText = "ECHO";

        if (text.ToUpperInvariant().StartsWith(echoText))
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(text.Substring(echoText.Length).Trim()), cancellationToken);
        }

        if (turnContext.Activity.TryGetChannelData(out TeamsChannelData channelData))
        {
            await StoreTeamsChannelData(channelData.Team.Id, turnContext.Activity.ServiceUrl);
        }
        else
        {
            _logger.LogWarning("Activity does not contains TeamsChannelData. Not storing the activity details.");
        }
    }

    /// <summary>
    /// Called when a new user is joins the team, or when the application is first installed
    /// </summary>
    /// <param name="teamsMembersAdded"></param>
    /// <param name="teamInfo"></param>
    /// <param name="turnContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        if (turnContext.Activity.TryGetChannelData(out TeamsChannelData channelData))
        {
            await StoreTeamsChannelData(teamInfo.Id, turnContext.Activity.ServiceUrl);
        }
        else
        {
            _logger.LogWarning("Activity does not contains TeamsChannelData. Not storing the activity details.");
        }
    }

    private Task StoreTeamsChannelData(string teamId, string serviceUrl)
    {
        return _proactiveBotDataService.Create(new(teamId, new Uri(serviceUrl)));
    }
}
