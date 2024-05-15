const fs = require("fs");
const content = fs.readFileSync("../appManifest/manifest.json", "utf8");
const manifest = JSON.parse(content);
manifest.configurableTabs[0].scopes = ["groupchat"];
const permissions = [
  {
      "name": "Calls.AccessMedia.Chat",
      "type": "Application"
  },
  {
      "name": "Calls.JoinGroupCalls.Chat",
      "type": "Application"
  },
  {
      "name": "ChatSettings.ReadWrite.Chat",
      "type": "Application"
  },
  {
      "name": "ChatMessage.Read.Chat",
      "type": "Application"
  },
  {
      "name": "ChatMessageReadReceipt.Read.Chat",
      "type": "Application"
  },
  {
      "name": "ChatMember.Read.Chat",
      "type": "Application"
  },
  {
      "name": "Chat.Manage.Chat",
      "type": "Application"
  },
  {
    "name": "TeamsTab.Create.Chat",
    "type": "Application"
  },
  {
    "name": "TeamsTab.Read.Chat",
    "type": "Application"
  },
  {
      "name": "TeamsTab.ReadWrite.Chat",
      "type": "Application"
  },
  {
      "name": "TeamsAppInstallation.Read.Chat",
      "type": "Application"
  },
  {
      "name": "TeamsActivity.Send.Chat",
      "type": "Application"
  }
];
manifest.authorization.permissions.resourceSpecific = permissions;
fs.writeFileSync(
  "../appManifest/manifest.json",
  JSON.stringify(manifest, null, 2)
);
