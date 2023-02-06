import React from "react";
import ReactDOM from "react-dom";
import App from "./app/app";
import * as microsoftTeams from "@microsoft/teams-js";
import reportWebVitals from "./report-web-vitals";
import { HashRouter } from "react-router-dom";

// Initialize the Microsoft Teams SDK
microsoftTeams.app.initialize();

ReactDOM.render(
    <App />,
  document.getElementById("root")
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();