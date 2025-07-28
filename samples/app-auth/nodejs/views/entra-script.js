import {
  AccountInfo,
  IPublicClientApplication,
  createNestablePublicClientApplication,
} from "@azure/msal-browser";
// Import the Microsoft Teams JavaScript client SDK for use in the browser bundle
import * as microsoftTeams from '@microsoft/teams-js';

  

  
  let pca;
  
   function initializePublicClient(context) {
      const msalConfig = {
    auth: {
      clientId: "b70a6f76-2210-4e61-8727-313cbc26fe98",
      authority: `https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47`,
      supportsNestedAppAuth: true,
      redirectUri: "brk-multihub://dummy.com/AuthSuccess"
    },
  };
    console.log("Starting initializePublicClient");
    return createNestablePublicClientApplication(msalConfig).then(
      (result) => {
        console.log("Client app created");
        pca = result;
        return pca;
      }
    );
  }



  microsoftTeams.app.initialize().then(() => {
    microsoftTeams.getContext((context) => {
    console.log("Microsoft Teams SDK initialized");
  initializePublicClient(context).then((pca) => {
    console.log("Public client initialized", pca);
     // MSAL.js exposes several account APIs, logic to determine which account to use is the responsibility of the developer
  const account = pca.getActiveAccount();

  const accessTokenRequest = {
  scopes: ["user.read"],
  account: account,
  };

  pca
    .acquireTokenSilent(accessTokenRequest)
    .then(function (accessTokenResponse) {
      // Acquire token silent success
      let accessToken = accessTokenResponse.accessToken;
      console.log("Access Token acquired silently:", accessToken);
    fetch(`https://teams-auth.ngrok.io/api/setAuthToken?accessToken=${accessToken}`, {
      method: "GET"
    })
    .then((response) => {
      console.log("Token set successfully", response);
    
      //window.location.href = "https://teams-auth.ngrok.io/api/authorize?redirectUri=https://teams-auth.ngrok.io/Auth0Success";
      // fetch(`https://teams-auth.ngrok.io/api/testAuthToken`, {
      // method: "GET",
      // });
      // Call your API with token
      //callApi(accessToken);
    }).catch((error) => {
      console.error("Error setting token:", error); 
    });
    })
    .catch(function (error) {
      console.error("Error acquiring token silently:", error);
      //Acquire token silent failure, and send an interactive request
      if (error.errorCode === 'InteractionRequired') {
        pca
          .acquireTokenPopup(accessTokenRequest)
          .then(function (accessTokenResponse) {
            // Acquire token interactive success
            let accessToken = accessTokenResponse.accessToken;
            // Call your API with token
            console.log("Access Token acquired interactively:", accessToken);
            //callApi(accessToken);
          })
          .catch(function (error) {
            // Acquire token interactive failure
            console.log(error);
          });
      }
      console.log(error);
    });
  }).catch((error) => {
    console.error("Error initializing public client:", error);
  });
})
});


  

console.log('entra script');