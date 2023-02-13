import React from "react";
import { render, screen } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import sinon from "sinon";
import proxyquire from "proxyquire";
import { app } from "@microsoft/teams-js";

const appStub = sinon
  .stub(app, "getContext")
  .returns(Promise.resolve({ user: { userPrincipalName: "test@test.com" } }));

const Tab = proxyquire("./Tab", {
  "@microsoft/teams-js": {
    app: appStub,
  },
}).default;

it("renders Tab", async () => {
  await act(async () => {
    await render(<Tab />);
  });
  expect(screen.getByText(/test@test.com/i)).toBeInTheDocument();
});
