import { render, screen } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import sinon from "sinon";
import {
  FrameContexts,
  HostClientType,
  HostName,
  app,
} from "@microsoft/teams-js";
import Tab from "../Tab";

describe("Tab tests", () => {
  var registerOnThemeChangeHandlerStub: sinon.SinonStub<
    [handler: (theme: string) => void],
    void
    >;
  var themeChangeHandler: (theme: string) => void;

  beforeEach(() => {
    sinon.stub(app, "getContext").returns(
      Promise.resolve<app.Context>({
        user: { userPrincipalName: "test@test.com", id: "" },
        app: {
          theme: "default",
          locale: "",
          sessionId: "",
          host: {
            name: HostName.teams,
            clientType: HostClientType.web,
            sessionId: "",
            ringId: "",
          },
        },
        page: { id: "", frameContext: FrameContexts.content },
      })
    );

    registerOnThemeChangeHandlerStub = sinon
      .stub(app, "registerOnThemeChangeHandler")
      .callsFake((handler) => {
        themeChangeHandler = handler;
      });
  });

  afterEach(() => {
    sinon.restore();
  });

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

    // Change the theme
    act(() => {
      themeChangeHandler("dark");
    });
    expect(screen.getByText(/Theme: dark/i)).toBeInTheDocument();
  });
});
