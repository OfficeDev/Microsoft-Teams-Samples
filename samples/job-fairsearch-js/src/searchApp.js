const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const azure = require("azure-storage");

class SearchApp extends TeamsActivityHandler {
  async handleTeamsMessagingExtensionQuery(context, query) {
    const { parameters } = query;

    const skills = getParameterByName(parameters, "Skills");
    const location = getParameterByName(parameters, "Location");
    const availability = getParameterByName(parameters, "Availability");

    // Define your Azure Table Storage connection string or credentials
    const connectionString =
      "DefaultEndpointsProtocol=https;AccountName=sampleappsstorage;AccountKey=x88LLO2VEL6uR/E4769pBE75tfiFYK9KkSdLLuyL9p1X52GoLQ1sjegcOjGlhmvjjIZWp6LEE2uVIt+LzwjYtQ==;EndpointSuffix=core.windows.net";

    // Create a table service object using the connection string
    const tableService = azure.createTableService(connectionString);

    var candiDateData = [];

    // Define the name of the table you want to store data in
    const tableName = "JobFairSearchLog";

    // Define the partition key and row key of the entity to be retrieved
    const partitionKey = "1"; // Replace with the actual partition key
    const rowKey = "1"; // Replace with the actual row key

    // Define a function to fetch candidates based on parameters
    function fetchCandidates(queryParameters) {
      return new Promise((resolve, reject) => {
        const query = new azure.TableQuery();

        let whereClause = "";
        let skillsAdded = false;

        // Construct the where clause dynamically based on provided parameters
        Object.keys(queryParameters).forEach((key, index) => {
          if (key !== "availability") {
            if (key === "skills") {
              skillsAdded = true; // Mark that skills are added
            } else {
              const condition = `${key} eq '${queryParameters[key]}'`;
              if (skillsAdded) {
                whereClause += ` and (${condition})`;
              } else {
                whereClause += `(${condition})`;
              }
            }
          }
        });

        // If no parameters provided, select all
        if (whereClause === "") {
          whereClause = "PartitionKey ne ''"; // Dummy condition to select all incase parameters are null or empty
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
                .map((skill) => skill.trim());
              filteredCandidates = filteredCandidates.filter((candidate) => {
                const candidateSkills = candidate.skills._.split(",").map(
                  (skill) => skill.trim()
                );
                return skills.every((skill) => candidateSkills.includes(skill));
              });
            }

            resolve(filteredCandidates);
          }
        );
      });
    }

    // Fetch candidates based on parameters. For demo purpose parameters are preconfigured
    var candidates = await fetchCandidates({
      country: "India",
      skills: "Node.js",
    });
    const attachments = [];
    candiDateData = candidates;
    console.log("Candidates:", candiDateData);
    candiDateData.map((result) => {
      const heroCard = CardFactory.heroCard(
        result.name._,
        result.skills._,
        null
      );

      attachments.push(heroCard);
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