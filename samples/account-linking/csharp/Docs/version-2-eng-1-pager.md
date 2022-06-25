# V2 Eng 1 Pager
The second iteration of the account linking sample is going to be focused on the 'reverse' path where the account-linking is bidirectional so the partner developer can
use their native identifier for the user to perform actions in M365. 

The implementation is just going to be repeating the work in v1 just with AzureAd as the target resource that we maintain refresh and access tokens for. It will maintain two tables,
one that keeps the user's M365 identity as the key and the refresh/access tokens as the value (what we have in v1) and another that keeps their partner identity as the key and the AzureAD refresh/access tokens as the value. 

## Goals
1. 'Reverse' path (external identity => Azure AD access token)
2. Show how to 'silently' try SSO and raise consent iff necessary
3. Show how to chain together AzureAD consent & partner consent in a single auth popup / flow.

### Additional nice-to-have
1. Allow for "client" configuration / multiple clients

## Implementation
Given that we need the reverse path from the partner system back into AzureAD we need to add the concept of the user's identity in the partner system. We need to be able to perform 4 operations

1. Authorize client in both AzureAd and partner system (link the identities)
2. Get the AzureAd access token given the partner id for the user
3. Get the partner auth token given the azure id for the user
4. Unlink the two accounts

Use [phantauth](phantauth.net/doc/) and demonstrate this using the OIDC system. Need to do this since we need a stable way to know the 
'partner' system's id. The most "standards" compliant way would be OIDC.

### Auth Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant UA as UserAgent
    participant SVC as Account Linking Service
    participant PAS as Partner Auth Service <br> (PhantAuth)
    participant AAD as Azure Active Directory
    C ->> C: Generate PKCE code_challenge & code_verifier
    C ->> UA: Direct User to GET /oauth/start<br>?state={client_state}<br>&code_challenge={code_challenge}
    activate UA
    UA ->> SVC: GET /oauth/start<br>?state={client_state}<br>&code_challenge={code_challenge}
    SVC ->> SVC: Get symmetric signing key 'k'
    SVC ->> SVC: Create state_token = <br> (code_challenge, client_state, sign_k(code_challenge, client_state))

    SVC ->> UA: Redirect to Partner /authorize<br> ?state={state_token}<br>&client_id={partner_client_id}<br>&redirect_uri={/oauth/end}
    UA ->> PAS: Authorize client
    PAS ->> UA: Redirect to /oauth/end<br> ?state={state_token}<br>&code={partner_oauth_code}
    UA ->> SVC: GET /oauth/end<br> ?state={state_token}<br>&code={partner_oauth_code}
    SVC ->> SVC: Validate state
    SVC ->> SVC: Create state_token = <br> (code_challenge, partner_oauth_code, sign_k(code_challenge, partner_oauth_code))

    SVC ->> UA: Redirect to AzureAd /authorize<br> ?state={state_token}<br>&client_id={aad_client_id}<br>&redirect_uri={/oauth/end}
    UA ->> AAD: Authorize client
    AAD ->> UA: Redirect to /oauth/end<br> ?state={state_token}<br>&code={aad_oauth_code}
    UA ->> SVC: GET /oauth/end<br> ?state={state_token}<br>&code={aad_oauth_code}
    SVC ->> SVC: Validate state
    SVC ->> SVC: Create state_token = <br> (code_challenge, partner_oauth_code, aad_oauth_code, sign_k(code_challenge, aad_oauth_code))

    SVC ->> UA: Redirect to Azure AD authorization
    SVC ->> UA: Redirect to client with state_token, client_state
    UA ->> C: state_token & client_state
    deactivate UA
    C ->> SVC: PUT /link?<br>code={state_token}<br>&code_verifier={code_verifier}<br> Authorization: Azure AD user-delegated bearer token
    SVC ->> SVC: Validate state (code)

    SVC ->> PAS: POST /token<br>?client_id={client_id}<br>&client_secret={partner_client_secret}<br>&code={partner_oauth_code}
    PAS ->> SVC: partner_access_token, partner_refresh_token, id_token
    SVC ->> SVC: Extract parter_user_id from id_token

    SVC ->> AAD: POST /token<br>?client_id={client_id}<br>&client_secret={aad_client_secret}<br>&code={aad_oauth_code}
    AAD ->> SVC: access_token, refresh_token, id_token
    SVC ->> SVC: Extract aad_user_id from id_token (tid/oid claims)

    SVC ->> SVC: Store {key: aad_user_id, value: (partner_access_token, partner_refresh_token)} into 'forward' mapping
    SVC ->> SVC: Store {key: partner_user_id, value: (aad_access_token, aad_refresh_token)} into 'reverse' mapping
```

### Client implementations
#### Teams App
Need Tab / ME / Conversational Bot

Call /user with PhantAuth? 

#### External App
Need 'login with PhantAuth'

Post notification? Just get the /me endpoint result?

### Open Items
1. How to pass login hint(s) to the Azure Flow
2. What to do w/o id_token (or usable claims from id token)
3. Token bloat, how big is too big? 
4. What 'action' should be demonstrated on the Azure side. Notification would be ideal but that is a pretty involved scenario in its own right - /me is easiest but hard to demonstrate the value add.
5. Does it make sense to factor this out into 3 separate processes (one Teams app, one public app, account linking service) or leave as a single monolith
    pro breakout: clearly demonstrates responsibility & dependencies
    con breakout: setup gets more complex, registration more complex, includes (likely minimal) S2S auth concerns.


