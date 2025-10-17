# Contoso Knowledge Hub - AI Academic & Career Guidance Agent

Contoso Knowledge Hub is an intelligent guidance agent built on the [Teams AI library V2](https://aka.ms/teams-ai-library-v2), designed to empower students in their academic and career journeys. The agent offers personalized support for course selection, study strategies, career development planning, and academic roadmap creation.

## Get started with the template

> **Prerequisites**
>
> To run the template in your local dev machine, you will need:
>
> - [Node.js](https://nodejs.org/), supported versions: 20, 22.
> - [Microsoft 365 Agents Toolkit Visual Studio Code Extension](https://aka.ms/teams-toolkit) latest version or [Microsoft 365 Agents Toolkit CLI](https://aka.ms/teamsfx-toolkit-cli).
> - An account with [OpenAI](https://platform.openai.com/).

> For local debugging using Microsoft 365 Agents Toolkit CLI, you need to do some extra steps described in [Set up your Microsoft 365 Agents Toolkit CLI for local debugging](https://aka.ms/teamsfx-cli-debugging).

1. First, select the Microsoft 365 Agents Toolkit icon on the left in the VS Code toolbar.
1. In file *env/.env.playground.user*, fill in your OpenAI key `SECRET_OPENAI_API_KEY=<your-key>`.
1. Press F5 to start debugging which launches your app in Microsoft 365 Agents Playground using a web browser. Select `Debug in Microsoft 365 Agents Playground`.
1. You can try any of these sample prompts to interact with Contoso Knowledge Hub:
   - "Recommend courses for building AI skills"
   - "How can I choose courses that align with my career goals?"
   - "Create a sample course plan for me"
   - "Where can I find more information about the best educational institutions in IT?"
   - "Shortlist the top three courses based on my career goals. Share the next steps for each"
   - "Recommend top two courses from the Who's Who of IT in AI"

**Congratulations**! You are running Contoso Knowledge Hub that can now provide intelligent academic and career guidance in Microsoft 365 Agents Playground:

![ai chat agent](https://github.com/user-attachments/assets/984af126-222b-4c98-9578-0744790b103a)

## What's included in the template

| Folder       | Contents                                            |
| - | - |
| `.vscode`    | VSCode files for debugging                          |
| `appPackage` | Templates for the application manifest        |
| `env`        | Environment files                                   |
| `infra`      | Templates for provisioning Azure resources          |
| `src`        | The source code for the application                 |

The following files can be customized and demonstrate an example implementation to get you started.

| File                                 | Contents                                           |
| - | - |
|`src/index.js`| Application entry point. |
|`src/config.js`| Defines the environment variables.|
|`src/app/instructions.txt`| Defines the AI prompt and capabilities for Contoso Knowledge Hub.|
|`src/app/app.js`| Handles business logics for the Contoso Knowledge Hub agent.|

The following are Microsoft 365 Agents Toolkit specific project files. You can [visit a complete guide on Github](https://github.com/OfficeDev/TeamsFx/wiki/Teams-Toolkit-Visual-Studio-Code-v5-Guide#overview) to understand how Microsoft 365 Agents Toolkit works.

| File                                 | Contents                                           |
| - | - |
|`m365agents.yml`|This is the main Microsoft 365 Agents Toolkit project file. The project file defines two primary things:  Properties and configuration Stage definitions. |
|`m365agents.local.yml`|This overrides `m365agents.yml` with actions that enable local execution and debugging.|
|`m365agents.playground.yml`|This overrides `m365agents.yml` with actions that enable local execution and debugging in Microsoft 365 Agents Playground.|

## Key Features

- **Personalized Course Recommendations:** Suggests courses to build AI skills and other in-demand competencies
- **Career-Aligned Academic Planning:** Guides students in choosing courses that match their career goals
- **Sample Course Plans:** Generates tailored course plans based on individual aspirations
- **Institutional Insights:** Provides information about top educational institutions in IT and related fields
- **Course Shortlisting & Next Steps:** Shortlists top courses aligned with career objectives and outlines actionable next steps
- **Expert-Endorsed Recommendations:** Recommends leading courses from recognized experts in IT and AI

## Extend the template

To extend Contoso Knowledge Hub with more AI capabilities, explore [Teams AI library V2 documentation](https://aka.ms/m365-agents-toolkit/teams-agent-extend-ai).

## Additional information and references

- [Microsoft 365 Agents Toolkit Documentations](https://docs.microsoft.com/microsoftteams/platform/toolkit/teams-toolkit-fundamentals)
- [Microsoft 365 Agents Toolkit CLI](https://aka.ms/teamsfx-toolkit-cli)
- [Microsoft 365 Agents Toolkit Samples](https://github.com/OfficeDev/TeamsFx-Samples)
