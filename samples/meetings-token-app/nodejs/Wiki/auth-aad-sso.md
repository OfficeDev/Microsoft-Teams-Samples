#### Registering your app through the Azure Active Directory portal in-depth:

1. Go to Azure Portal -> Azure Active Directory -> App Registrations.
2. Select same AD application created in previous step (Bot registration).
3. Under **Manage**, select **Expose an API**. 
4. Select the **Set** link to generate the Application ID URI in the form of `api://{AppID}`. Insert your fully qualified domain name (with a forward slash "/" appended to the end) between the double forward slashes and the GUID. The entire ID should have the form of: `api://fully-qualified-domain-name.com/{AppID}` ²
    * ex: `api://subdomain.example.com/00000000-0000-0000-0000-000000000000`.
    
    The fully qualified domain name is the human readable domain name from which your app is served. If you are using a tunneling service such as ngrok, you will need to update this value whenever your ngrok subdomain changes.
    * ex: `api://f631****.ngrok-free.app/9051a142-901a-4384-a83c-556c2888b071`.
 
5. Select the **Add a scope** button. In the panel that opens, enter `access_as_user` as the **Scope name**.
6. Set **Who can consent?** to `Admins and users`
7. Fill in the fields for configuring the admin and user consent prompts with values that are appropriate for the `access_as_user` scope:
    * **Admin consent display name:** Teams can access the user’s profile.
    * **Admin consent description**: Allows Teams to call the app’s web APIs as the current user.
    * **User consent display name**: Teams can access the user profile and make requests on the user's behalf.
    * **User consent description:** Enable Teams to call this app’s APIs with the same rights as the user.
8. Ensure that **State** is set to **Enabled**
9. Select the **Add scope** button to save 
    * The domain part of the **Scope name** displayed just below the text field should automatically match the **Application ID** URI set in the previous step, with `/access_as_user` appended to the end:
        * `api://subdomain.example.com/00000000-0000-0000-0000-000000000000/access_as_user`
10. In the **Authorized client applications** section, identify the applications that you want to authorize for your app’s web application. Select *Add a client application*. Enter each of the following client IDs and select the authorized scope you created in the previous step:
    * `1fec8e78-bce4-4aaf-ab1b-5451cc387264` (Teams mobile/desktop application)
    * `5e3ce6c0-2b1f-4285-8d4b-75ee78787346` (Teams web application)
11. Navigate to **API Permissions**. Select *Add a permission* > *Microsoft Graph* > *Delegated permissions*, then add the following permissions:
    * User.Read (enabled by default)
    * email
    * offline_access
    * OpenId
    * profile

12. Navigate to **Authentication**

    If an app hasn't been granted IT admin consent, users will have to provide consent the first time they use an app.

    Set a redirect URI:
    * Select **Add a platform**.
    * Select **web**.
    * Enter the **redirect URI** for your app. This will be the page where a successful implicit grant flow will redirect the user. This will be same fully qualified domain name that you entered in step 5 followed by the API route where a authentication response should be sent. If you are following any of the Teams samples, this will be: `https://subdomain.example.com/auth-end`

    Next, enable implicit grant by checking the following boxes:  
    ✔ ID Token  
    ✔ Access Token  
    
Congratulations! You have completed the app registration prerequisites to proceed with your tab SSO app.     

> **NOTE:**
>
> * ¹ If your Azure AD app is registered in the _same_ tenant where you're making an authentication request in Teams, the user won't be asked to consent and will be granted an access token right away. Users only need to consent to these permissions if the Azure AD app is registered in a different tenant.
> * ² If you get an error stating that the domain is already owned and you are the owner, follow the procedure at [Quickstart: Add a custom domain name to Azure Active Directory](https://docs.microsoft.com/en-us/azure/active-directory/fundamentals/add-custom-domain) to register the domain, and then repeat step 5, above. (This error can also occur if you aren't signed in with Admin credentials in the Office 365 tenancy).
> * If you are not receiving the UPN (User Principal Name) in the returned access token, you can add it as an [optional claim](https://docs.microsoft.com/azure/active-directory/develop/active-directory-optional-claims) in Azure AD.
