import React from "react";
import sinon from "sinon";
import { app } from "@microsoft/teams-js";

import Tab from "./Tab";

sinon.stub(app, "getContext").returns(
  Promise.resolve({
    user: { userPrincipalName: "test@test.com" },
    app: { theme: "default" },
  })
);
sinon.stub(app, "registerOnThemeChangeHandler").returns(Promise.resolve());

export default {
  title: "Tab",
  component: Tab,
};

export const Template = () => <Tab />;

