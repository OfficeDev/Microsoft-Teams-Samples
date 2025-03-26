using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using AdaptiveCards.Templating;
using AdaptiveCards;
using Microsoft.Teams.Samples.Bot.FrontlineWorker.Demo.Models;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Options;

namespace Microsoft.Teams.Samples.Bot.FrontlineWorker.Demo
{
    public class OnBehalfOfResult
    {
        //this class allows us to deal with the response from the OnBehalfOf Azure AD call. Allowing us to return both the AuthenticationResult object or the Error details if an exception is thrown.
        //in prod, this should probably go into a seperate class file, in the models folder.
        public bool Success { get; set; }
        public string Error { get; set; }
        public AuthenticationResult Result { get; set; }
    }
    public class FrontLineBot : TeamsActivityHandler
    {        
        readonly UserState _userState;
        readonly IStatePropertyAccessor<string> _tokenAccessor;
        private readonly AppSettings _appSettings;
        public FrontLineBot(UserState userState, IOptions<AppSettings> appSettings)
        {
            //these are the member variables that are used to store the access token in storage for a specific user
            _userState = userState;
            _tokenAccessor = userState.CreateProperty<string>("accessToken");
            //member variable for passing the values in appsetting to the bot
            _appSettings = appSettings.Value;
        }

        public async override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            //this method is called whenever the bot receives a request from a supported channel.
            //process the message by calling the base implementation of this method, which in turn will then call the appropriate method in the SDK (for example, if a text message has been received
            //it would call OnMessageActivityAsync)
            await base.OnTurnAsync(turnContext, cancellationToken);
            //once finished, it saves the information about this user (stored in the turnContext) in iStorage, as a userState object
            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        protected override Task OnInstallationUpdateAddAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            //when the bot is installed, send the welcome message...
            var name = turnContext.Activity.From.Name;
            var replyText = $"Hello {name}, I'm the consent bot. Ask me to 'get profile' and I'll try and make a connection to Graph to get some information about you. If I need consent, I'll walk you through the process, using a sign-in card, and a modal window.";
            turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            return Task.CompletedTask;

        }

        protected async override Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            //this method is called by the SDK when an invoke is received by the bot. Typically this is because a button has been pressed in an adaptive card, that sends some data back to the bot
            //or it could be because the Teams SSO was invoked

            if (turnContext.Activity.Name == "signin/tokenExchange")
            {
                //tokenExchange is Teams SSO, so it provides a user level (delegated) access token. We will now attempt to swap this for a Graph API token, and make some calls to find some information out about the user, such as presence, profile photo, etc...
                string accessToken = Convert.ToString(turnContext.Activity.Value);
                accessToken = GetValue(accessToken, "signin");
                //this method takes the Access Token, and stores it in userState, for us to reference later
                await _tokenAccessor.SetAsync(turnContext, accessToken);

                //reports the success back down to the user...
                string message = "You have succesfully signed in, and the bot has now saved your access token to user state. Please send your command again to try again.";
                await turnContext.SendActivityAsync(MessageFactory.Text(message));
                return;

            }

            //processes the value of the invoke, to allow you to make decisions on what to do next...
            string value = Convert.ToString(turnContext.Activity.Value);
            value = GetValue(value, "invoke");

            //if the AdminConsent is successful, then this will update the existing message, with a hero card, that reports the success to the user, and provides them
            //with either the ability to make a call to Graph API using App Permissions or Delegated Permissions
            if (turnContext.Activity.Name == "signin/verifyState" && value == "AdminConsent")
            {
                var card = new HeroCard()
                {
                    Title = "Admin Consent Successful",
                    Text = "Admin Consent was successful! Click Get Profile to test Graph API functionality now consent has been granted for all users in this tenant.",
                    Buttons = new List<CardAction>()
                        {
                            new CardAction() {
                                Type = ActionTypes.ImBack,
                                Title = "Get Profile (app permissions)",
                                Text = "Get Profile - app",
                                Value = "Get Profile - app",
                                DisplayText = "Get My Profile (app permissions)"
                            },
                            new CardAction() {
                                Type = ActionTypes.ImBack,
                                Title = "Get Profile (delegated permissions)",
                                Text = "Get Profile - delegated",
                                Value = "Get Profile - delegated",
                                DisplayText = "Get My Profile (delegated permissions)"
                            }
                        }
                };
                var activity = MessageFactory.Attachment(card.ToAttachment());
                activity.Id = turnContext.Activity.ReplyToId;
                await turnContext.UpdateActivityAsync(activity, cancellationToken);
                return;
            }

            //If the user user consent was succesful then this will display a card that reports success and provide the ability to invoke the graph API call (delegated).
            if (turnContext.Activity.Name == "signin/verifyState" && value == "UserConsent")
            {


                var card = new HeroCard()
                {
                    Title = "User Consent Successful",
                    Text = "User Consent was successful! Click Get Profile to test Graph API functionality now consent has been granted for this user!",
                    Buttons = new List<CardAction>()
                        {
                            new CardAction() {
                                Type = ActionTypes.ImBack,
                                Title = "Get Profile (delegated permissions)",
                                Text = "Get Profile - delegated",
                                Value = "Get Profile - delegated",
                                DisplayText = "Get My Profile (delegated permissions)"
                            }
                        }
                };
                var activity = MessageFactory.Attachment(card.ToAttachment());
                activity.Id = turnContext.Activity.ReplyToId;
                await turnContext.UpdateActivityAsync(activity, cancellationToken);
                return;


            }

        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var text = turnContext.Activity.Text.Trim();
            bool consentRequired = false;
            string replyText = string.Empty;

            if (text.Equals("hello", StringComparison.OrdinalIgnoreCase) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                //this is the start of the bot flow. It sends a hero card down that advertises what the bot can do, and has some buttons to start the various flows off that demonstrate
                //how Team SSO and consent works for Teams bots!
                var card = new HeroCard()
                {
                    Title = "Hi, I'm the Azure AD Consent Bot!",
                    Text = "I can get information about you from Graph API, and display it in a hero card. To do this, click Get Profile... Or if you want to provide Consent, click Provide Consent. If you click Get Profile, and consent isn't already in place, I'm smart enough to deal with this and I'll walk you through that process!",
                    Buttons = new List<CardAction>()
                    {
                        new CardAction() {
                            Type = ActionTypes.Signin,
                            Title = "Provide Admin Consent",
                            Text = "Provide Admin Consent",
                            Value = $"{_appSettings.BaseUrl}loginstart?appId={_appSettings.MicrosoftAppId}"
                        },
                        new CardAction() {
                            Type = ActionTypes.Signin,
                            Title = "Provide User Consent",
                            Text = "Provide User Consent",
                            Value = $"{_appSettings.BaseUrl}loginstart?userConsent=true&appId={_appSettings.MicrosoftAppId}"
                        },
                        new CardAction() {
                            Type = ActionTypes.ImBack,
                            Title = "Get Profile (app permissions)",
                            Text = "Get Profile - app",
                            Value = "Get Profile - app",
                            DisplayText = "Get My Profile (app permissions)"
                        },
                        new CardAction() {
                            Type = ActionTypes.ImBack,
                            Title = "Get Profile (delegated permissions)",
                            Text = "Get Profile - delegated",
                            Value = "Get Profile - delegated",
                            DisplayText = "Get My Profile (delegated permissions)"
                        }
                    }
                };
                await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
                return;
            }

            if (text.Equals("showaccesstoken", StringComparison.OrdinalIgnoreCase) || text.Equals("show access token", StringComparison.OrdinalIgnoreCase))
            {
                //if showaccesstoken is sent to the bot, then attempt to get the access token from userState
                var valueAccess = await _tokenAccessor.GetAsync(turnContext, cancellationToken: cancellationToken);

                //if there are no access tokens in userState for that user, then send down a sign-in card, that will request the access token using Teams SSO
                if (valueAccess == null)
                {
                    var attachment = new Microsoft.Bot.Schema.Attachment
                    {
                        Name = "Sign-In",
                        Content = new OAuthCard
                        {
                            ConnectionName = "AAD",
                            Text = "Sign-in!",
                            TokenExchangeResource = new TokenExchangeResource
                            {
                                Id = "showAccessToken"
                            }
                        },
                        ContentType = OAuthCard.ContentType,
                    };
                    var activity = MessageFactory.Attachment(attachment);
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
                else
                //access token exists in user state, so send that back down to the user
                //keep in mind that this doesn't guarantee that the access token is valid, it simply just sends what is in user-state down to the user
                {
                    var activity = MessageFactory.Text($"Here is the Delegated Permissions Access Token I have stored in User State for this user, that will be swapped for a Graph API access token when required: {valueAccess}");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }

                
                return;
            }

            if (text.Equals("sign-in", StringComparison.OrdinalIgnoreCase) || text.Equals("signin", StringComparison.OrdinalIgnoreCase) || text.Equals("sign in", StringComparison.OrdinalIgnoreCase))
            {
                //if a sign-in text message is received by the bot, it will send down a sign-in card, that invokes the Teams SSO process. This will result in a new request
                //being sent to the Teams SSO service, to get a Access Token, and replace the access token that exists in userState for that user (if one already exists)
                var attachment = new Microsoft.Bot.Schema.Attachment
                {
                    Name = "Sign-In",
                    Content = new OAuthCard
                    {
                        ConnectionName = "AAD",
                        Text = "Sign-in!",
                        TokenExchangeResource = new TokenExchangeResource
                        {
                            Id = "signIn"
                        }
                    },
                    ContentType = OAuthCard.ContentType,
                };
                var activity = MessageFactory.Attachment(attachment);
                // NOTE: This activity needs to be sent in the 1:1 conversation between the bot and the user. 
                // If the bot supports group and channel scope, this code should be updated to send the request to the 1:1 chat. 

                await turnContext.SendActivityAsync(activity, cancellationToken);
                return;
            }

            

            if (text.Equals("get profile", StringComparison.OrdinalIgnoreCase))
            {
                var card = new HeroCard()
                {
                    Title = "I can help with that!",
                    Text = "Do you want me to get your profile using an app permissions or a delegated permissions token?",
                    Buttons = new List<CardAction>()
                    {
                        new CardAction() {
                            Type = ActionTypes.ImBack,
                            Title = "Get Profile (app permissions)",
                            Text = "Get Profile - app",
                            Value = "Get Profile - app",
                            DisplayText = "Get My Profile (app permissions)"
                        },
                        new CardAction() {
                            Type = ActionTypes.ImBack,
                            Title = "Get Profile (delegated permissions)",
                            Text = "Get Profile - delegated",
                            Value = "Get Profile - delegated",
                            DisplayText = "Get My Profile (delegated permissions)"
                        }
                    }
                };
                await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
                return;
            }

            

            if (text.Equals("get profile - app", StringComparison.OrdinalIgnoreCase))
            {
                //this will use an App Permissions token to pull data from Graph, and then populate an adaptive card with these details and send it back down to the user.
                var userAadId = turnContext.Activity.From.AadObjectId;
                string channelData = Convert.ToString(turnContext.Activity.ChannelData);
                string tenantId = GetTenantIdFromChannelData(channelData);

                //gets an App Permissions access token
                var accessToken = await GetTokenForApp(tenantId);
                //provisions and Graph client, using the Graph SDK, to allow calls to Graph to be made
                var graphClient = GetGraphServiceClient(accessToken);

                try
                {

                    //tries to discover information about the user, using the app permissions token...
                    var userInfo = await graphClient.Users[userAadId]
                        .Request()
                        .Select("displayName,department,jobTitle,state,officeLocation")
                        .GetAsync(cancellationToken);

                    //to get the photo of the user, it's a bit tricker. We need to get the bits of the image, and then convert it to a URL with the image base64 in that URL
                    Stream stream = await graphClient.Users[userAadId].Photo.Content.Request().GetAsync();
                    var data = new byte[stream.Length];
                    await stream.ReadAsync(data, 0, data.Length);
                    var base64 = Convert.ToBase64String(data);
                    var url = "data:image/png;base64," + base64;
                    //if you add a breakpoint here, and then inspect the value of url. Copy that value into your browser, you should see the image onf the user

                    //note: it is not possible to discover availability using app permission token, so we pass "N/A" as a string to populate the card with a value
                    var payload = GetProfileAdaptiveCard(url, userInfo.DisplayName, "N/A", userInfo.OfficeLocation, userInfo.JobTitle, userInfo.Department);

                    var card = AdaptiveCard.FromJson(payload).Card;
                    var attachment = CreateAdaptiveCardAttachment(card);
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
                    

                    replyText = $"Here is the App Permissions Access Token I used for authentication with Graph API: {accessToken}";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
                    return;
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    if (ex.Error.Code == "Authorization_IdentityNotFound")
                        {
                        //IdentityNotFound means that there is no Enterprise App that has been established in the customer tenant, therefore we need to go through a Consent Process to get this setup...
                        replyText = $"Here is the token I used for authentication with Graph API: {accessToken}";
                        await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
                        replyText = $"Unfortunately I couldn't pull information on your user via Graph API, as you need to provide consent. Please decode this token by visiting https://jwt.ms/ and review the roles claim. Let's use a sign-in card to get consent, and then try sending 'get profile' to me again please.";
                        await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken); 
                        consentRequired = true;
                    }
                    if (ex.Error.Code == "Authorization_RequestDenied")
                    {
                        //RequestDenied means that there are no 'roles' (scopes) on the access token, therefore we need to go through a Consent Process to get the customer to consent to these roles being available on your access token...
                        replyText = $"Here is the token I used for authentication with Graph API: {accessToken}";
                        await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
                        replyText = $"Unfortunately I couldn't pull information on your user via Graph API, as you need to provide consent. Please decode this token by visiting https://jwt.ms/ and review the roles claim. Let's use a sign-in card to get consent, and then try sending 'get profile' to me again please.";
                        await turnContext.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
                        consentRequired = true;
                    }
                    if (ex.Error.Code == "ImageNotFound")
                    {
                        var noImageMessage = "I can't generate a profile card, as there is no profile image for this user. Please add an image for this user via the Office 365 profile screen and try again.";
                        await turnContext.SendActivityAsync(MessageFactory.Text(noImageMessage));
                        return;
                    }

                }

                if (consentRequired == true)
                {

                    //if a graph service exception is thrown, that requires consent to resolve, then send down the sign-in card, that invokes the consent process
                    var card1 = SigninCard.Create("Consent Required!", "Provide Consent", $"{_appSettings.BaseUrl}loginstart?appId={_appSettings.MicrosoftAppId}");

                    await turnContext.SendActivityAsync(MessageFactory.Attachment(card1.ToAttachment()), cancellationToken);
                    return;
                }
                

                

                
            }
            if (text.Equals("get profile - delegated", StringComparison.OrdinalIgnoreCase))
            {
                //this flow will attempt to get information from Graph API using Delegated Permissions
                //firstly, it checks if there is an existing user access token in userState
                var valueAccess = await _tokenAccessor.GetAsync(turnContext, cancellationToken: cancellationToken);

                //if no access token is in user state, an OAuthCard is sent down to the user to initialise the SSO, and then once complete, user state will have an Access Token in it for this user...
                if (valueAccess == null)
                {
                    var attachment = new Microsoft.Bot.Schema.Attachment
                    {
                        Name = "Sign-In",
                        Content = new OAuthCard
                        {
                            ConnectionName = "AAD",
                            Text = "Sign-in!",
                            TokenExchangeResource = new TokenExchangeResource
                            {
                                Id = "getProfileDelegated"
                            }
                        },
                        ContentType = OAuthCard.ContentType,
                    };
                    var activity = MessageFactory.Attachment(attachment);
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                    return;
                }
                else
                {
                    //this means that there is an access token in user state... it doens't mean that the access token in user state is valid though!
                    //let's try and swap it for a Graph API token with the scopes we need.



                    //this method swaps the Access Token provided by Teams SSO for a GraphAPI Access Token, using the On-Behalf-Of Flow. In Prod, you could pass the scopes you require dyanmically, but for this sample, we have hard-coded them in the method.
                    var result = await GetTokenOnBehalfOfUser(valueAccess);
                    if (!result.Success)
                    {
                        if (result.Error == "invalid_grant" || result.Error == "invalid_resource")
                        {
                            //this means that the scopes we are asking for have not been consented to. therefore we need to walk through an interactive consent process
                            //to do this, we'll send a card with a sign-in action. That then walks the end-user through the process of consenting to the scopes we need
                            var card = new HeroCard()
                            {
                                Title = "User Consent Required!",
                                Text = $"ERROR: {result.Error}. I need you to provide Consent so I can make some calls to the Graph API to find out some information about you. Please click the button below to complete the Consent process...",
                                Buttons = new List<CardAction>()
                            {
                            new CardAction() {
                                Type = ActionTypes.Signin,
                                Title = "Provide Consent For User Scopes",
                                Text = "Provide Consent For User Scopes",
                                Value = $"{_appSettings.BaseUrl}loginstart?userConsent=true&appId={_appSettings.MicrosoftAppId}"
                                }
                            }
                            };

                            await turnContext.SendActivityAsync(MessageFactory.Attachment(card.ToAttachment()), cancellationToken);
                            return;


                        }
                        if (result.Error == "invalid_assertion" || result.Error == "invalid_jwt_token")
                        {
                            //this means that the token in user state has expired, or is not valid. In that case, we need to send down the oAuth card to get a new access token in userState...
                            var attachment = new Microsoft.Bot.Schema.Attachment
                            {
                                Name = "Sign-In",
                                Content = new OAuthCard
                                {
                                    ConnectionName = "AAD",
                                    Text = "Sign-in!",
                                    TokenExchangeResource = new TokenExchangeResource
                                    {
                                        Id = "getProfileDelegated"
                                    }
                                },
                                ContentType = OAuthCard.ContentType,
                            };
                            var activity = MessageFactory.Attachment(attachment);
                            await turnContext.SendActivityAsync(activity, cancellationToken);
                            return;
                        }
                    }
                    else
                    {
                        //we can assume the scopes we asked for have been issued on the accessToken. Feel free to check the result.Result object to confirm this though if you'd like in the code and if not, ask for them to be consented to...
                        //we now have 2 Access Tokens that have been issued via Azure AD.
                        //valueAccess variable contains the Azure AD Access Token that has an audience of our app. We can't use this to call Graph API, but we can use it for authentication into our own app
                        //result.Result.AccessToken contains the Azure AD Access Token that has an audience of Graph API, and contains scopes that dictate what resources we are/aren't allowed to use in Graph API
                        //feel free to inspect these tokens by going to https://jwt.ms/ to see the claims and learn more about them
                        //if you want to validate that they are authentic, and the tokens haven't been tampered with (i.e. they can be trusted), you can use libraries to do this, or you can swap value2 for a Graph API token (and Microsoft will check if it's valid for you and return a Graph API token as a result)
                        //alternatively, if you have a Graph API token, just make a call to the /me endpoint, and if it returns data, you know that it can be trusted
                        //if you'd rather use a library to check the validity, see the following repo that Scott Perham created - https://github.com/scottperham/MSAL-Token-Validation

                        //we will now make some Graph API calls to get information about the user, so we can populate an adaptive card with this data and send it back down to the user
                        try
                        {
                            //creates the Graph Client using the Graph API SDK, by passing the Graph API access token into the Graph Client
                            var graphClient = GetGraphServiceClient(result.Result.AccessToken);

                            //uses the Graph Client to make calls to Graph API. In this case, it is discovering the presence of the user
                            var userPresence = await graphClient.Me.Presence
                                .Request()
                                .GetAsync(cancellationToken);

                            var userInfo = await graphClient.Me
                                .Request()
                                .Select("displayName,department,jobTitle,state,officeLocation")
                                .GetAsync(cancellationToken);

                            //to get the photo of the user, it's a bit tricker. We need to get the bits of the image, and then convert it to a URL with the image base64 in that URL
                            Stream stream = await graphClient.Me.Photo.Content.Request().GetAsync();
                            var data = new byte[stream.Length];
                            await stream.ReadAsync(data, 0, data.Length);
                            var base64 = Convert.ToBase64String(data);
                            var url = "data:image/png;base64," + base64;
                            //if you add a breakpoint here, and then inspect the value of url. Copy that value into your browser, you should see the image onf the user

                            var payload = GetProfileAdaptiveCard(url, userInfo.DisplayName, userPresence.Availability, userInfo.OfficeLocation, userInfo.JobTitle, userInfo.Department);

                            var card = AdaptiveCard.FromJson(payload).Card;
                            var attachment = CreateAdaptiveCardAttachment(card);
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
                            return;

                        }
                        catch (ServiceException ex)
                        {
                            var error = ex.Error.Code;
                            if (error == "ImageNotFound")
                            {
                                var noImageMessage = "I can't generate a profile card, as there is no profile image for this user. Please add an image for this user via the Office 365 profile screen and try again.";
                                await turnContext.SendActivityAsync(MessageFactory.Text(noImageMessage));
                                return;
                            }
                        }

                    }
                }

                   


            }

            //this is what happens if the bot doesn't understand the command it receives...
            var unknownMessage = "Sorry, I didn't understand that, try sending 'hello' or 'help' to get started :)";
            await turnContext.SendActivityAsync(MessageFactory.Text(unknownMessage));


        }

        private async Task<OnBehalfOfResult> GetTokenOnBehalfOfUser(string accessToken)
        {
            //this method allows you to swap an Azure AD Access token, where the audience is your app registration, for a Graph API access token, with the scopes defined in this method.
            //if you wanted to, you could make the scopes more dynamic, by passing in an string array, which contains the scopes, into the method
            //once this is complete, you can return this access token back to the client, as it's not the same as an app token, or you can keep it server side and make the graph API calls from the server and just pass the 
            //results of those graph calls back to the client. It's up to you!

            var assertion = new UserAssertion(accessToken);
            var builder = ConfidentialClientApplicationBuilder.Create(_appSettings.MicrosoftAppId)
                .WithClientSecret(_appSettings.MicrosoftAppPassword);

            var client = builder.Build();


            try
            {
                var tokenBuilder = await client.AcquireTokenOnBehalfOf(new[] { "https://graph.microsoft.com/User.Read", "https://graph.microsoft.com/Presence.Read" }, assertion)
                    .ExecuteAsync();

                return new OnBehalfOfResult
                {
                    Success = true,
                    Result = tokenBuilder
                };
            }
            catch (MsalUiRequiredException ex)
            {
                var errorCode = ex.ErrorCode;
                return new OnBehalfOfResult
                {
                    Success = false,
                    Error = errorCode
                };
            }

        }

        private async Task<string> GetTokenForApp(string tenantId)
        {
            //this method gets a token on behalf of the app (not the user). App tokens are priviliged and must be protected (not passed down to the client).
            //remember that most app permission scopes require an administrator to approve them, so in production scenarios, it may make more sense to avoid using this, and instead try to work with delegated permissions where possible
            //when working with Graph API, app permissions that you require on the scope must be defined in your app registration, as the scopes cannot be defined in the call make to Azure AD, instead you must use the /.default scope
            
            var builder = ConfidentialClientApplicationBuilder.Create(_appSettings.MicrosoftAppId)
                .WithClientSecret(_appSettings.MicrosoftAppPassword)
                .WithTenantId(tenantId)
                .WithRedirectUri("msal" + _appSettings.MicrosoftAppId + "://auth");

            var client = builder.Build();

            var tokenBuilder = await client.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                .ExecuteAsync();

            return tokenBuilder.AccessToken;
        }


        private GraphServiceClient GetGraphServiceClient(string token)
        {
            return new GraphServiceClient(
               //this method creates a new Graph Service client, and requires you to pass in a token (app or delegated). Use GetTokenForApp or GetTokenOnBehalfOfUser to get an access token for Graph.
               new DelegateAuthenticationProvider(
                   requestMessage =>
                   {
                       requestMessage.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, token);
                       return Task.CompletedTask;
                   }));
        }

        private string GetTenantIdFromChannelData(string channelData)
        {
            //this method pulls out the TenantId of the user who made the request to the bot, from ChannelData, and returns it as a string
            using JsonDocument doc = JsonDocument.Parse(channelData);
            JsonElement root = doc.RootElement;

            var u1 = root;
            var u3 = u1.GetProperty("tenant");
            var u4 = u3.GetProperty("id");
            return u4.ToString();
        }


        private string GetValue(string value, string typeOf)
        {
            //this method pulls out the value of the invoke request sent to the bot and returns it as a string
            if (typeOf == "invoke")
            {
                using JsonDocument doc = JsonDocument.Parse(value);
                JsonElement root = doc.RootElement;

                var u1 = root;
                var u3 = u1.GetProperty("state");
                return u3.ToString();
            }

            if (typeOf == "signin")
            {
                using JsonDocument doc = JsonDocument.Parse(value);
                JsonElement root = doc.RootElement;

                var u1 = root;
                var u3 = u1.GetProperty("token");
                return u3.ToString();
            }
            return null;

        }

        public string GetProfileAdaptiveCard(string imageUrl, string profileName, string availability, string officeLocation, string jobtitle, string department)
        {
            //this method provisions and returns the Profile adaptive card when called
            //it uses the Adaptive Card templating, to replace placeholders in the card with data
            var templateJson = @"
            {
              ""type"": ""AdaptiveCard"",
              ""body"": [
                {
                            ""type"": ""TextBlock"",
                  ""size"": ""Medium"",
                  ""weight"": ""Bolder"",
                  ""text"": ""My Profile""
                },
                {
                            ""type"": ""ColumnSet"",
                  ""columns"": [
                    {
                                ""type"": ""Column"",
                      ""items"": [
                        {
                                    ""size"": ""Small"",
                          ""style"": ""Person"",
                          ""type"": ""Image"",
                          ""url"": ""${imageUrl}""
                        }
                      ],
                      ""width"": ""auto""
                    },
                    {
                                ""type"": ""Column"",
                      ""items"": [
                        {
                                    ""type"": ""TextBlock"",
                          ""weight"": ""Bolder"",
                          ""text"": ""${name}"",
                          ""wrap"": true
                        },
                        {
                                    ""type"": ""TextBlock"",
                          ""spacing"": ""None"",
                          ""text"": ""${availability}"",
                          ""isSubtle"": true,
                          ""wrap"": true
                        }
                      ],
                      ""width"": ""stretch""
                    }
                  ]
                },
                {
                            ""type"": ""TextBlock"",
                  ""text"": ""This Profile Card was generated by making Graph API calls"",
                  ""wrap"": true
                },
                {
                            ""type"": ""FactSet"",
                  ""facts"": [
                    {
                                ""title"": ""Office Location: "",
                      ""value"": ""${office}""
                    },
                    {
                                ""title"": ""Job Title:"",
                      ""value"": ""${jobtitle}""
                    },
                    {
                                ""title"": ""Department:"",
                      ""value"": ""${department}""
                    }
                  ]
                }
              ],
              ""actions"": [
                {
                    ""type"": ""Action.Submit"",
                    ""title"": ""Show User (delegated) Access Token"",
                    ""data"": {
                        ""msteams"": {
                            ""type"": ""messageBack"",
                            ""text"": ""showAccessToken""
                                    }
                                }
                }
              ],
              ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
              ""version"": ""1.4""
            }
            ";

            var template = new AdaptiveCardTemplate(templateJson);

            var payload = template.Expand(new
            {
                imageUrl = imageUrl,
                name = profileName,
                availability = "Availability: " + availability,
                office = officeLocation,
                jobtitle = jobtitle,
                department = department
            });

            return payload;


        }

        public static Microsoft.Bot.Schema.Attachment CreateAdaptiveCardAttachment(AdaptiveCard card)
        {
            //helper method to change an adaptive card to an attachment.
            var adaptiveCardAttachment = new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return adaptiveCardAttachment;
        }
    }
}
