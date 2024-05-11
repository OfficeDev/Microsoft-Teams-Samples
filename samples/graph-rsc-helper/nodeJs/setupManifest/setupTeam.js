const fs = require("fs");
const content = fs.readFileSync("../appManifest/manifest.json", "utf8");
const manifest = JSON.parse(content);
manifest.configurableTabs[0].scopes = ["team"];
const permissions = [
  {
    name: "Channel.Create.Group",
    type: "Application",
  },
  {
    name: "Channel.Delete.Group",
    type: "Application",
  },
  {
    name: "ChannelMeeting.ReadBasic.Group",
    type: "Application",
  },
  {
    name: "ChannelMeetingParticipant.Read.Group",
    type: "Application",
  },
  {
    name: "ChannelMeetingRecording.Read.Group",
    type: "Application",
  },
  {
    name: "ChannelMeetingTranscript.Read.Group",
    type: "Application",
  },
  {
    name: "ChannelMeetingNotification.Send.Group",
    type: "Application",
  },
  {
    name: "ChannelMessage.Read.Group",
    type: "Application",
  },
  {
    name: "ChannelSettings.ReadWrite.Group",
    type: "Application",
  },
  {
    name: "TeamsActivity.Send.Group",
    type: "Application",
  },
  {
    name: "TeamsAppInstallation.Read.Group",
    type: "Application",
  },
  {
    name: "TeamMember.Read.Group",
    type: "Application",
  },
  {
    name: "TeamSettings.ReadWrite.Group",
    type: "Application",
  },
  {
    name: "TeamsTab.ReadWrite.Group",
    type: "Application",
  },
];
manifest.authorization.permissions.resourceSpecific = permissions;
fs.writeFileSync(
  "../appManifest/manifest.json",
  JSON.stringify(manifest, null, 2)
);
