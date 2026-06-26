/// <summary>
/// This class is responsible for handling the messaging extension code and SSO auth inside copilot.
/// </summary>

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const config = require("./config");
const azure = require("azure-storage");

const PROFILE_IMAGE_URL = "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg";

const getParameterByName = (parameters, name) => {
  const param = parameters.find((parameter) => parameter.name === name);
  return param ? param.value : "";
};

const parseAvailability = (value) => {
  if (typeof value !== "string") {
    return undefined;
  }

  switch (value.trim().toLowerCase()) {
    case "true":
    case "yes":
      return true;
    case "false":
    case "no":
      return false;
    default:
      return undefined;
  }
};

const buildSearchObject = (skills, country, availability) => {
  const filterObject = {};

  if (country) {
    filterObject.country = country;
  }

  if (skills) {
    filterObject.skills = skills;
  }

  if (availability !== undefined) {
    filterObject.availability = availability;
  }

  return filterObject;
};

const toSkillList = (value) =>
  value
    .split(",")
    .map((skill) => skill.trim().toLowerCase())
    .filter(Boolean);

const matchesRequestedSkills = (candidate, requestedSkills) => {
  if (!requestedSkills) {
    return true;
  }

  const candidateSkills = toSkillList(candidate.skills._);
  const requestedSkillList = toSkillList(requestedSkills);

  return requestedSkillList.some((requestedSkill) =>
    candidateSkills.some((candidateSkill) => candidateSkill.includes(requestedSkill))
  );
};

const fetchCandidates = (tableService, tableName, queryParameters) =>
  new Promise((resolve, reject) => {
    const query = new azure.TableQuery();
    const conditions = [];

    Object.entries(queryParameters).forEach(([key, value]) => {
      if (key === "skills" || key === "availability") {
        return;
      }

      conditions.push(`(${key} eq '${value}')`);
    });

    if (queryParameters.availability !== undefined && queryParameters.availability !== null) {
      conditions.push(`(availability eq ${queryParameters.availability})`);
    }

    query.where(conditions.length > 0 ? conditions.join(" and ") : "PartitionKey ne ''");

    tableService.queryEntities(tableName, query, null, (error, result) => {
      if (error) {
        reject(error);
        return;
      }

      const candidates = (result?.entries || []).filter((candidate) =>
        matchesRequestedSkills(candidate, queryParameters.skills)
      );

      resolve(candidates);
    });
  });

const buildAttachment = (result) => {
  const availability = result.availability._ ? "Yes" : "No";
  const resultCard = CardFactory.adaptiveCard({
    type: "AdaptiveCard",
    $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
    version: "1.4",
    body: [
      {
        type: "TextBlock",
        text: "Expert Finder",
        wrap: true,
        size: "Large",
        weight: "Bolder",
        separator: true,
      },
      {
        type: "ColumnSet",
        columns: [
          {
            type: "Column",
            items: [
              {
                type: "Image",
                url: PROFILE_IMAGE_URL,
                altText: "profileImage",
                size: "Small",
                style: "Person",
              },
            ],
            width: "auto",
          },
          {
            type: "Column",
            items: [
              {
                type: "TextBlock",
                weight: "Bolder",
                text: `${result.name._}`,
                wrap: true,
                spacing: "None",
                horizontalAlignment: "Left",
                maxLines: 0,
                size: "Medium",
              },
            ],
            width: "stretch",
            spacing: "Medium",
            verticalContentAlignment: "Center",
          },
        ],
      },
      {
        type: "FactSet",
        facts: [
          {
            title: "Skills:",
            value: `${result.skills._}`,
          },
          {
            title: "Location:",
            value: `${result.country._}`,
          },
          {
            title: "Available:",
            value: availability,
          },
        ],
      },
    ],
  });

  const previewCard = CardFactory.heroCard(result.name._, result.skills._);

  return { ...resultCard, preview: previewCard };
};

class SearchApp extends TeamsActivityHandler {
  
  async run(context) {
    await super.run(context);
  }

  async handleTeamsMessagingExtensionQuery(context, query) {
    const { parameters } = query;

    const skills = getParameterByName(parameters, "Skill");
    const country = getParameterByName(parameters, "Location");
    const availabilityParam = getParameterByName(parameters, "Availability");
    const availability = parseAvailability(availabilityParam);
    const searchObject = buildSearchObject(skills, country, availability);

    // Define your Azure Table Storage connection string or credentials
    const storageConnectionString = config.storageConnectionString;

    // Create a table service object using the connection string
    const tableService = azure.createTableService(storageConnectionString);

    // Define the name of the table you want to store data in
    const tableName = config.storageTableName;

    // When the Bot Service Auth flow completes, the query.State will contain a magic code used for verification.
    const magicCode =
      query.state && Number.isInteger(Number(query.state))
        ? query.state
        : '';

    const tokenResponse = await context.adapter.getUserToken(
      context,
      config.oauthConnectionName,
      magicCode
    );

    if (!tokenResponse || !tokenResponse.token) {
      //     // There is no token, so the user has not signed in yet.
      //     // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
      const signInLink = await context.adapter.getSignInLink(
        context,
        config.oauthConnectionName
      );

      return {
        composeExtension: {
          type: 'auth',
          suggestedActions: {
            actions: [
              {
                type: 'openUrl',
                value: signInLink,
                title: 'Bot Service OAuth'
              },
            ],
          },
        },
      };
    }

    // Fetch candidates based on applied filters.
    const candidates = await fetchCandidates(tableService, tableName, searchObject);
    const attachments = candidates.map(buildAttachment);

    console.log("Candidates:", candidates);

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: attachments,
      },
    };
  }
}

module.exports = { SearchApp };