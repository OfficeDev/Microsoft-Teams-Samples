---
page_type: sample
description: Enable and configure your apps for Teams meetings to use in stage 
products:
- office-teams
- office
- office-365
languages:
-  csharp
extensions:
 contentType: samples
 createdDate: "10/01/2023 19:03:46"
urlFragment: officedev-microsoft-teams-samples-meeting-stage-reaction-raisehand-api-csharp
---

# Meeting Stage Reaction Raisehand API

## Reaction API

The Reaction API allows you to react in the meeting stage. The types of reactions include like, heart, laugh, applause, and surprised.

## Raise Hand API

The Raise Hand API allows your app to show if the user has raised hand during the meeting.


## Interaction with app- Web


## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account


## Setup.

**This capability is currently available in developer preview only**


1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
    > NOTE: When you create your app registration, you will create an App ID and App password - make sure you keep these for later.


2. Setup NGROK
- Run ngrok - point to port 3978

```bash
# ngrok http -host-header=rewrite 3978
```

3. Setup for code

- Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

 - In a terminal, navigate to `samples/meeting-stage-reaction-raisehand-api/csharp`

    ```bash
    # change into project folder
    cd # ReactionRaisehandAPI
    ```

- Inside ClientApp folder execute the below command.

    ```bash
    npm install
    ```
    
- Run the app from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the app
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `ReactionRaisehandAPI` folder
  - Select `ReactionRaisehandAPI.csproj` file
  - Press `F5` to run the project


## Further Reading.

[Meeting Stage](https://review.learn.microsoft.com/en-us/microsoftteams/platform/apps-in-teams-meetings/build-apps-for-teams-meeting-stage?branch=pr-en-us-7630#reaction-api)
