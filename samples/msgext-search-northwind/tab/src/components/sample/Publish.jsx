import React from "react";
import "./Publish.css";
import { Image } from "@fluentui/react-components";

export function Publish(props) {
  const { docsUrl } = {
    docsUrl: "https://aka.ms/teamsfx-docs",
    ...props,
  };
  return (
    <div className="publish page">
      <h2>Publish to Teams</h2>
      <p>
        Your app's resources and infrastructure are deployed and ready? Publish and register your
        app to Teams app catalog to share your app with others in your organization!
      </p>
      <Image src="publish.png" />
      <p>
        For more information, see the{" "}
        <a href={docsUrl} target="_blank" rel="noreferrer">
          docs
        </a>
        .
      </p>
    </div>
  );
}
