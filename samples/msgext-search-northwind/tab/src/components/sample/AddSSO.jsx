import React from "react";

export function AddSSO(props) {
  const { ssoDocUrl, fluentDocUrl, readmeUrl } = {
    ssoDocUrl: "https://aka.ms/teamsfx-sso-doc",
    fluentDocUrl: "https://aka.ms/teamsfax-fluent-doc",
    readmeUrl: "https://aka.ms/teamsfx-add-sso",
    ...props,
  };

  return (
    <div>
      <h2>Add Single Sign On feature to retrieve user profile</h2>
      <p>
        One of the advantages of building tab application in Teams is to leverage the user's
        identity provided through Microsoft Teams (Known as{" "}
        <a href={ssoDocUrl} target="_blank" rel="noreferrer">
          SSO support
        </a>
        ) and interact with Microsoft Graph for building rich and seamless user experience. With
        Teams Toolkit, authentication is further simplified by automatic AAD app registration and
        configuration.
      </p>
      <p>
        Starting with this simple static tab app, you can follow few steps to add SSO logics to
        authenticate users, retrieve user's profile photo, and build UI layers with{" "}
        <a href={fluentDocUrl} target="_blank" rel="noreferrer">
          Fluent
        </a>
        .
      </p>
      <p>
        See instructions ({""}
        <a href={readmeUrl} target="_blank" rel="noreferrer">
          here
        </a>
        ) to learn how to enable Authentication component with Teams Toolkit and code snippets.
      </p>
    </div>
  );
}
