## Teams Tab using Microsoft Teams App Template

This sample demonstarte the use of Microsoft Teams App template to create teams app leveraging teams capabilities like Tabs

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- [Teams Toolkit](https://aka.ms/teams-toolkit)  
  

## To try this sample

1. Clone the repository 
   ```bash
   git clone https://github.com/OfficeDev/microsoft-teams-samples.git
   ```

1. Build your solution
      - Launch Visual Studio
      - File -> Open -> Project/Solution
      - Navigate to `samples/tabs-graph-toolkit/csharp` folder
      - Select `TabToolkit.sln` file
      - Build the solution

1. Setup ngrok
      ```bash
      ngrok http -host-header=rewrite 3978
      ```

1. [Upload app package](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/deploy-and-publish/apps-upload#load-your-package-into-teams) to Teams.

1. Further Learning
      - [Build Apps using Microsoft Teams Toolkit](https://docs.microsoft.com/en-us/microsoftteams/platform/toolkit/visual-studio-code-overview)






