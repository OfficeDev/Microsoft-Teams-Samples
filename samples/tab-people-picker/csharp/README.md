---
page_type: sample
description: This is an tab app which shows the feature of client sdk people picker.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
contentType: samples
createdDate: "06-10-2021 01:48:56"
---

# Tab people picker

This is an tab app which shows the feature of client sdk people picker.

![tab](TabPeoplePicker/Images/Tab.PNG)

![scope vise search](TabPeoplePicker/Images/ScopeSearch.PNG)

![All memberes of organisation search](TabPeoplePicker/Images/AllMemberesOfOrganisationSearch.PNG)

![Single select](TabPeoplePicker/Images/SingleSelect.PNG)

![Set selected search](TabPeoplePicker/Images/SetSelectedSearch.PNG)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## To try this sample
  
- Clone the repository 
   ```bash
   git clone https://github.com/OfficeDev/microsoft-teams-samples.git
   ```

- Build your solution

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/tab-people-picker/csharp/TabPeoplePicker/` folder
  - Select `TabPeoplePicker/.csproj` file
  - Press `F5` to run the project

- Setup ngrok
  ```bash
  ngrok http -host-header=rewrite 3978
  ```

- Config changes
   - Press F5 to run the project
   - Update the ngrok in manifest
   - Zip all three files present in manifest folder

- [Upload app manifest file](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/deploy-and-publish/apps-upload#load-your-package-into-teams) (zip file) to your team
