const { CardFactory, TeamsInfo, TeamsActivityHandler, MessageFactory } = require('botbuilder');
const GraphHelper = require('../helpers/graphHelper');
const ACData = require("adaptivecards-templating");
const WelcomeCard = require('../resources/welcome-card.json');
const AttendanceSummaryCard = require('../resources/attendance-summary-card.json');

class MeetingAttendanceBot extends TeamsActivityHandler {
  constructor() {
    super();

    // Activity handler for meeting end event.
    this.onTeamsMeetingEndEvent(async (meeting, context, next) => {
      var meetingDetails = await TeamsInfo.getMeetingInfo(context);
      var graphHelper = new GraphHelper();

      var result = await graphHelper.getNewestMeetingAttendanceRecordsAsync(meetingDetails.details.msGraphResourceId);

      if (result != null) {
        const template = new ACData.Template(AttendanceSummaryCard);

        const cardPayload = template.expand({
          $root: result
        });

        await context.sendActivity({ attachments: [CardFactory.adaptiveCard(cardPayload)] });
      }
    });
  }

  async onInstallationUpdateAddActivity(context) {
    try {
      var meetingDetails = await TeamsInfo.getMeetingInfo(context);
      await context.sendActivity({ attachments: [CardFactory.adaptiveCard(WelcomeCard)] });
    }
    catch (ex) {
      console.error(ex.message);
      await context.sendActivity(MessageFactory.text("Please make sure bot is installed in meeting chat."));
    }
  };

}
module.exports.MeetingAttendanceBot = MeetingAttendanceBot;