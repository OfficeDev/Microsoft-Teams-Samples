# Account linking flows
This document outlines the various (sub) flows that are used in account linking

## Authorization flow
The authorization flow is effectively and [OAuth2.0 authorization code grant](https://datatracker.ietf.org/doc/html/rfc6749#section-4.1) nested inside an [OAuth2.0 PKCE public client grant](https://datatracker.ietf.org/doc/html/rfc7636). 

The assumption is there is an existing trust relationship between the `Client` and the `Account Linking Service`. In the existing sample they are in the same process but in the abstract this could be done via any authentication of the client on the requests to the service.

```mermaid
sequenceDiagram
    participant C as Client
    participant UA as UserAgent
    participant SVC as Account Linking Service
    participant DS as Downstream Auth Service <br> (GitHub)
    C ->> C: Generate PKCE code_challenge & code_verifier
    C ->> UA: Direct User to GET /oauth/start<br>?state={client_state}<br>&code_challenge={code_challenge}
    activate UA
    UA ->> SVC: GET /oauth/start<br>?state={client_state}<br>&code_challenge={code_challenge}
    SVC ->> SVC: Get symmetric signing key 'k'
    SVC ->> SVC: Create state_token = <br> (code_challenge, client_state, sign_k(code_challenge, client_state))
    SVC ->> UA: Redirect to /authorize<br> ?state={state_token}<br>&client_id={client_id}<br>&redirect_uri={/oauth/end}
    UA ->> DS: Authorize client
    DS ->> UA: Redirect to /oauth/end<br> ?state={state_token}<br>&code={oauth_code}
    UA ->> SVC: GET /oauth/end<br> ?state={state_token}<br>&code={oauth_code}
    SVC ->> SVC: Validate state
    SVC ->> SVC: Create state_token = <br> (code_challenge, oauth_code, sign_k(code_challenge, oauth_code))
    SVC ->> UA: Redirect to client with state_token, client_state
    UA ->> C: state_token & client_state
    deactivate UA
    C ->> SVC: PUT /link?<br>code={state_token}<br>&code_verifier={code_verifier}<br> Authorization: Azure AD user-delegated bearer token
    SVC ->> SVC: Validate state (code)
    SVC ->> DS: POST /token<br>?client_id={client_id}<br>&client_secret={client_secret}<br>&code={code}
    DS ->> SVC: access_token, refresh_token
```

In the above sample the open & close of the `User Agent` change based on capability. For Tab, this is the authentication popup, for conversational bot it is the OAuthCard and for Messaging Extension it is the auth invoke response. 

## Credential Storage flow
The storage flow is involved in securely storing the token credentials.

The goal is to ensure that the database is untrusted in case of a security breach. To that end each value is [encrypted with a symmetric derived key](https://datatracker.ietf.org/doc/html/rfc2898#section-5.2) from the user/tenant ids.

```mermaid
sequenceDiagram
    participant C as Client
    participant SVC as Account Linking Service
    participant DS as Downstream Auth Service <br> (GitHub)
    participant KV as Key/Value Database
    C ->> SVC: PUT /link?<br>code={state_token}<br>&code_verifier={code_verifier}<br> Authorization: Azure AD user-delegated bearer token
    SVC ->> DS: POST /token<br>?client_id={client_id}<br>&client_secret={client_secret}<br>&code={code}
    DS ->> SVC: access_token, refresh_token
    SVC ->> SVC: Derive encryption key 'K' from (client_id,tenant_id,user_id) [Pbkdf2]
    SVC ->> SVC: enc_k(access_token,refresh_token)
    SVC ->> KV: Store (key: (client_id,tenant_id,user_id), value: enc_k(access_token,refresh_token))
```

## Credential retrieval flow
The retrieval flow is the reverse of the storage flow. It performs the [OAuth2.0 refresh flow](https://datatracker.ietf.org/doc/html/rfc6749#section-6) if necessary.

```mermaid
sequenceDiagram
    participant C as Client
    participant SVC as Account Linking Service
    participant DS as Downstream Auth Service <br> (GitHub)
    participant KV as Key/Value Database
    C ->> SVC: Get /{tenant_id/{user_id}/token<br> Authorization: Azure AD user-delegated bearer token
    SVC ->> SVC: ACL on client
    SVC ->> KV: Get (client_id, tenant_id, user_id)
    KV ->> SVC: enc_k(access_token,refresh_token)
    SVC ->> SVC:Derive encryption key 'K' from (client_id,tenant_id,user_id) [Pbkdf2]
    SVC ->> SVC: Decrypt<br>dec_k(enc_k(access_token,refresh_token))
    SVC ->> DS: Refresh token<br>POST /token<br>?client_id={client_id}<br>&client_secret={client_secret}<br>&refresh_token={refresh_token}
    DS ->> SVC: access_token, refresh_token
    SVC ->> C: access_token
```
