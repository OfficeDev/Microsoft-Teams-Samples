using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Teams.TemplateBotCSharp.Properties;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Teams.TemplateBotCSharp.Dialogs
{
    /// <summary>
    /// This is Fetch Teams Info Dialog Class main purpose of this dialog class is to display Team Name, TeamId and AAD GroupId.
    /// </summary>
    [Serializable]
    public class FetchTeamsInfoDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var team = context.Activity.GetChannelData<TeamsChannelData>().Team;

            if (team != null)
            {
                var connectorClient = new ConnectorClient(new Uri(context.Activity.ServiceUrl));

                // Handle for channel conversation, AAD GroupId only exists within channel
                TeamDetails teamDetails = await connectorClient.GetTeamsConnectorClient().Teams.FetchTeamDetailsAsync(team.Id);

                var message = context.MakeMessage();
                message.Text = GenerateTable(teamDetails);

                await context.PostAsync(message);
            }
            else
            {
                // Handle for 1 to 1 bot conversation
                await context.PostAsync(Strings.TeamInfo1To1ConversationError);
            }

            //Set the Last Dialog in Conversation Data
            context.UserData.SetValue(Strings.LastDialogKey, Strings.LastDialogFetchTeamInfoDialog);

            context.Done<object>(null);
        }

        /// <summary>
        /// Generate HTML dynamically to show TeamId, TeamName and AAD GroupId in table format 
        /// </summary>
        /// <param name="teamDetails"></param>
        /// <returns></returns>
        private string GenerateTable(TeamDetails teamDetails)
        {
            if (teamDetails == null)
            {
                return string.Empty;
            }

            string tableHtml = $@"<table border='1'>
                                    <tr><td> Team id </td><td>{HttpUtility.HtmlEncode(teamDetails.Id)}</td><tr>
                                    <tr><td> Team name </td><td>{HttpUtility.HtmlEncode(teamDetails.Name)}</td></tr>
                                    <tr><td> AAD group id </td><td>{HttpUtility.HtmlEncode(teamDetails.AadGroupId)}</td><tr>
                                  </table>";
            return tableHtml;
        }
    }
}