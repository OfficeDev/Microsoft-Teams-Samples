const config = {
  // This is the name of the packaged manifest file
  manifestFileName: "microsoft-teams-app-sample.zip",
  // Supported Schemas
  SCHEMAS: [
    {
      version: "1.3",
      schema:
        "https://developer.microsoft.com/en-us/json-schemas/teams/v1.3/MicrosoftTeams.schema.json",
    },
    {
      version: "1.4",
      schema:
        "https://developer.microsoft.com/en-us/json-schemas/teams/v1.4/MicrosoftTeams.schema.json",
    },
    {
      version: "devPreview",
      schema:
        "https://raw.githubusercontent.com/OfficeDev/microsoft-teams-app-schema/preview/DevPreview/MicrosoftTeams.schema.json",
    },
    {
      version: "1.5",
      schema:
        "https://developer.microsoft.com/en-us/json-schemas/teams/v1.5/MicrosoftTeams.schema.json",
    },
    {
      version: "1.6",
      schema:
        "https://developer.microsoft.com/en-us/json-schemas/teams/v1.6/MicrosoftTeams.schema.json",
    },
    {
      version: "1.7",
      schema:
        "https://developer.microsoft.com/en-us/json-schemas/teams/v1.7/MicrosoftTeams.schema.json",
    },
    {
      version: "1.8",
      schema:
        "https://developer.microsoft.com/en-us/json-schemas/teams/v1.8/MicrosoftTeams.schema.json",
    },
  ],
};

module.exports = config;
