import {
  AccountInfo,
  IPublicClientApplication,
  createNestablePublicClientApplication,
} from "@azure/msal-browser";
// Import the Microsoft Teams JavaScript client SDK for use in the browser bundle
import * as microsoftTeams from '@microsoft/teams-js';

const naaLogin = (result) => {
  microsoftTeams.app.initialize().then(() => {
             microsoftTeams.authentication.authenticate({
            url: `https://dev-zvtwcdg4lg5a82cq.us.auth0.com/authorize?connection=Microsoftentracustom&audience=https://dev-zvtwcdg4lg5a82cq.us.auth0.com/api/v2/&response_type=code&scope=update:current_user_identities%20openid%20profile%20email&client_id=4Ccf0XImjVVt96wKX3mUllgWaVUh2qxB&redirect_uri=https://teams-auth.ngrok.io/Auth0Success&state=${(new Date()).getTime()%1000}`,
            width: 600,
            height: 535,
            successCallback: (result) => {
const parsedResult = JSON.parse(result);
console.log("notify success ", parsedResult);

              fetch(`https://teams-auth.ngrok.io/api/linkAccounts`, {
              method:"POST",
              headers: {
                "Content-Type": "application/json",
                //"Authorization": `Bearer ${accessToken}`
              },
              body: JSON.stringify({
                naaAuth0Payload: parsedResult.response.data,
              })
            }).then((response) => {
              console.log("Accounts linked successfully", response);
              microsoftTeams.tasks.submitTask();            
        });
      
}
  })
});
}  
        

const naaSuccessCallback = (accessTokenResponse) => {
  let accessToken = accessTokenResponse.accessToken;
      console.log("Access Token acquired silently:", accessToken);
    fetch(`https://teams-auth.ngrok.io/api/setAuthToken?accessToken=${accessToken}`, {
      method: "GET"
    })
    .then((response) => {
      console.log("Token set successfully", response);
       // Acquire token interactive success
            let accessToken = accessTokenResponse.accessToken;
            // Call your API with token
            console.log("Access Token acquired interactively:", accessToken);
             naaLogin();
    }).catch((error) => {
      console.error("Error setting token:", error); 
    });
};

 
 const linkEntraAccount = () => {
 
  let pca;
  
   function initializePublicClient(context) {
      const msalConfig = {
    auth: {
      clientId: "b70a6f76-2210-4e61-8727-313cbc26fe98",
      authority: `https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47`,
      supportsNestedAppAuth: true,
      redirectUri: "brk-multihub://teams-auth.ngrok.io/AuthSuccess"
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
      naaSuccessCallback(accessTokenResponse);
      
    })
    .catch(function (error) {
      console.error("Error acquiring token silently:", error);
      //Acquire token silent failure, and send an interactive request
      if (error.errorCode === 'InteractionRequired') {
        pca
          .acquireTokenPopup(accessTokenRequest)
          .then(function (accessTokenResponse) {

            naaSuccessCallback(accessTokenResponse);
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

}

document.getElementById("linkEntraAccount").addEventListener("click", linkEntraAccount);
document.getElementById("naaLogin").addEventListener("click", naaLogin);

console.log('entra script');