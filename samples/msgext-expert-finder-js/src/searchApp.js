/// <summary>
/// This class is responsible for handling the messaging extension code and SSO auth inside copilot.
/// </summary>

const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const config = require("./config");
const azure = require("azure-storage");

class SearchApp extends TeamsActivityHandler {
  
  async run(context) {
    await super.run(context);
  }

  async handleTeamsMessagingExtensionQuery(context, query) {
    const { parameters } = query;

    const skills = getParameterByName(parameters, "Skill");
    const country = getParameterByName(parameters, "Location");
    const availabilityParam = getParameterByName(parameters, "Availability");

    var availability;

    if (availabilityParam == "true") {
      availability = true;
    }
    else if (availabilityParam == "false") {
      availability = false;
    }
    else {
      availability = undefined;
    }

    function constructSearchObject(skills, country, availability) {
      const filterObject = {};

      if (country) {
        filterObject.country = country;
      }

      if (skills) {
        filterObject.skills = skills;
      }

      if (availability != undefined) {
        filterObject.availability = availability;
      }

      return filterObject;
    }

    const searchObject = constructSearchObject(skills, country, availability);

    // Define your Azure Table Storage connection string or credentials
    const storageConnectionString = config.storageConnectionString;

    // Create a table service object using the connection string
    const tableService = azure.createTableService(storageConnectionString);

    var candidateData = [];

    // Define the name of the table you want to store data in
    const tableName = config.tableName;

    // When the Bot Service Auth flow completes, the query.State will contain a magic code used for verification.
    const magicCode =
      query.state && Number.isInteger(Number(query.state))
        ? query.state
        : '';

    const tokenResponse = await context.adapter.getUserToken(
      context,
      "authbot",
      magicCode
    );

    if (!tokenResponse || !tokenResponse.token) {
      //     // There is no token, so the user has not signed in yet.
      //     // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
      const signInLink = await context.adapter.getSignInLink(
        context,
        "authbot"
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

    // Define a function to fetch candidates based on parameters
    function fetchCandidates(queryParameters) {
      return new Promise((resolve, reject) => {
        const query = new azure.TableQuery();

        let whereClause = "";
        let skillsAdded = false;

        // Construct the where clause dynamically based on provided parameters
        Object.keys(queryParameters).forEach((key, index) => {
          if (key === "skills" || key === "availability") {
            return; // Skip skills and availability for now, handle separately below
          }

          const condition = `${key} eq '${queryParameters[key]}'`;
          if (whereClause !== "") {
            whereClause += " and ";
          }
          whereClause += `(${condition})`;
        });

        // Add availability filter if provided
        if (queryParameters.availability !== undefined && queryParameters.availability !== null) {
          const availabilityCondition = `availability eq ${queryParameters.availability}`;
          if (whereClause !== "") {
            whereClause += " and ";
          }
          whereClause += `(${availabilityCondition})`;
        }

        // If no parameters provided, select all
        if (whereClause === "") {
          whereClause = "PartitionKey ne ''"; // Dummy condition to select all in case parameters are null or empty
        }

        query.where(whereClause);

        tableService.queryEntities(
          tableName,
          query,
          null,
          (error, result, response) => {
            if (error) {
              reject(error);
              return;
            }

            let filteredCandidates = result.entries;

            // Filter candidates based on skills
            if (queryParameters.skills) {
              const skills = queryParameters.skills
                .split(",")
                .map((skill) => skill.trim().toLowerCase());
              filteredCandidates = filteredCandidates.filter((candidate) => {
                const candidateSkills = candidate.skills._.split(",").map(
                  (skill) => skill.trim().toLowerCase()
                );
                return candidateSkills.some((candidateSkills) => candidateSkills.includes(skills));
              });
            }

            resolve(filteredCandidates);
          }
        );
      });
    }

    // Fetch candidates based on applied filters.
    var candidates = await fetchCandidates(searchObject);

    var attachments = [];
    candidateData = candidates;
    console.log("Candidates:", candidateData);

    // Create Adaptive Card object

    candidateData.map((result) => {
      var availability = result.availability._ ? "Yes" : "No"
      const resultCard = CardFactory.adaptiveCard({
        "type": "AdaptiveCard",
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "version": "1.4",
        "body": [
          {
            "type": "TextBlock",
            "text": "Expert Finder",
            "wrap": true,
            "size": "Large",
            "weight": "Bolder",
            "separator": true
          },
          {
            "type": "ColumnSet",
            "columns": [
              {
                "type": "Column",
                "items": [
                  {
                    "type": "Image",
                    "url": "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg",
                    "altText": "profileImage",
                    "size": "Small",
                    "style": "Person"
                  }
                ],
                "width": "auto"
              },
              {
                "type": "Column",
                "items": [
                  {
                    "type": "TextBlock",
                    "weight": "Bolder",
                    "text": `${result.name._}`,
                    "wrap": true,
                    "spacing": "None",
                    "horizontalAlignment": "Left",
                    "maxLines": 0,
                    "size": "Medium"
                  }
                ],
                "width": "stretch",
                "spacing": "Medium",
                "verticalContentAlignment": "Center"
              }
            ]
          },
          {
            "type": "FactSet",
            "facts": [
              {
                "title": "Skills:",
                "value": `${result.skills._}`
              },
              {
                "title": "Location:",
                "value": `${result.country._}`,
              },
              {
                "title": "Available:",
                "value": `${availability}`,
              }
            ]
          }
        ]
      });

      const previewCard = CardFactory.heroCard(
        result.name._,
        result.skills._
      );

      attachments.push({ ...resultCard, preview: previewCard });
    });

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: attachments,
      },
    };
  }
}

const getParameterByName = (parameters, name) => {
  const param = parameters.find((p) => p.name === name);
  return param ? param.value : "";
};

module.exports = { SearchApp };