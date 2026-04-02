# Messaging Extension with Link Unfurling Sample for Reddit Links (With Login)
This project contains the implementation of the 3-legged version of the reddit link unfurler. 
This uses the user-delegated access to the Reddit API and demonstrates how to perform a login and logout flow for link unfurling. 

This is also a demonstration of how to use the [Bot Framework Token Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-authentication?view=azure-bot-service-4.0#user-authentication-in-a-conversation) to store and manage user credentials
for generic OAuth2.0 providers.

## Getting Started
### Configuration
The `appsettings.json` has a skeleton of the variables required to run the applicaiton. 

#### Dotnet Secret Manager
To avoid checking in secrets, the project uses the [Dotnet Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows#secret-manager) to keep these values outside the repository.

```dotnetcli
dotnet user-secrets set "App:Id" "{{Your Bot Channel App Id}}"
```

```dotnetcli
dotnet user-secrets set "App:Password" "{{Your Bot Channel App Password}}"
```

```dotnetcli
dotnet user-secrets set "Reddit:AppId" "{{Your Reddit App Id}}"
```

```dotnetcli
dotnet user-secrets set "Reddit:AppPassword" "{{Your Reddit App Password}}"
```

The following lines in the `Startup.cs` file loads the configuration from the secret manager.

```cs
if (env.IsDevelopment())
{
    // Using dotnet secrets to store the settings during development
    // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows
    configBuilder.AddUserSecrets<Startup>();
}
```

### Running the application
```dotnetcli
dotnet run
```

This should start the application on port 5000 by default. 

To test locally use [ngrok](https://ngrok.com/) and configure the BotFramework registration's messaging endpoint.

```shell
ngrok http 5000
```