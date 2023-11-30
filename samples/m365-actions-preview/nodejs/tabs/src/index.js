// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./components/App";
import { FluentProvider, teamsLightTheme } from "@fluentui/react-components";

const container = document.getElementById("root");
const root = createRoot(container);
root.render(
  <FluentProvider theme={teamsLightTheme}>
    <App />
  </FluentProvider>
);
