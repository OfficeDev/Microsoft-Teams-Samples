
# Fallback URL Support

From February 2018 (when this project was first released) to April 2019, the Microsoft Teams mobile clients did not fully support the `signin` action protocol. Instead, they supported a `fallbackUrl` approach which is described below. The code for this is still in the project, but the codepath is never executed with Teams mobile clients released after April 2019 and version 1.4.1 or later of the [Microsoft Teams JavaScript library](https://www.npmjs.com/package/@microsoft/teams-js).

*************************************

* If the URL provided to the `signin` action has a `fallbackUrl` query string parameter, Teams will launch that URL in the browser.
* Otherwise, Teams will show an error saying that the action is not yet supported on mobile.

In the example, the mobile signin flow works the same way as on desktop, until the point where the OAuth callback page tries to send the verification code back to the bot. The bot sets the `fallbackUrl` query string parameter to be the same as the original url to the auth start page, so that the user goes to the same page on all platforms. ([View code](https://github.com/OfficeDev/microsoft-teams-sample-auth-node/blob/469952a26d618dbf884a3be53c7d921cc580b1e2/src/dialogs/BaseIdentityDialog.ts#L173-L178))

When the OAuth callback runs in a mobile browser, the call to `notifySuccess()` will fail silently because it's not running inside the Teams client. The window will not close and the bot won't get the verification code. To handle this case, the page has a timer that checks if it's still open after 5 seconds. If so, it asks the user to manually send the verification code via chat. The bot code is able to receive the verification code from either the `signin/verifyState` callback or a chat message. ([View code](https://github.com/OfficeDev/microsoft-teams-sample-auth-node/blob/469952a26d618dbf884a3be53c7d921cc580b1e2/src/dialogs/BaseIdentityDialog.ts#L106-L117))

To limit signing in to web and desktop clients only, you can either omit the `fallbackUrl` parameter, or point it to an error page that asks the user to sign in with Teams on web or desktop.

When the Teams mobile clients support the complete signin protocol, including passing the verification code via `notifySuccess()`, they will launch the auth start page in a popup window and ignore `fallbackUrl`.