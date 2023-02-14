import React from "react";
import "@testing-library/jest-dom";
import { render, screen } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import sinon from "sinon";
import proxyquire from "proxyquire";
import { app } from "@microsoft/teams-js";

const getContextStub = sinon
  .stub(app, "getContext")
  .returns(
    Promise.resolve({
      user: { userPrincipalName: "test@test.com" },
      app: { theme: "default" },
    })
  );
const registerOnThemeChangeHandlerStub = sinon
  .stub(app, "registerOnThemeChangeHandler")
  .returns(Promise.resolve());

const Tab = proxyquire("../Tab", {
  "@microsoft/teams-js": {
    app: {
      getContext: getContextStub,
      registerOnThemeChangeHandler: registerOnThemeChangeHandlerStub,
    },
  },
}).default;

describe("Tab tests", () => {
  it("Tab contains stubbed contest user principal name", async () => {
    await act(async () => {
      await render(<Tab />);
    });
    expect(screen.getByText(/test@test.com/i)).toBeInTheDocument();
  });

  it("Tab contains Congratulation text", async () => {
    await act(async () => {
      await render(<Tab />);
    });
    expect(screen.getByText(/Congratulations/i)).toBeInTheDocument();
  });

  it("registerOnThemeChangeHandler to be called", async () => {
    await act(async () => {
      await render(<Tab />);
    });

    expect(registerOnThemeChangeHandlerStub.called).toBe(true);
  });
});
