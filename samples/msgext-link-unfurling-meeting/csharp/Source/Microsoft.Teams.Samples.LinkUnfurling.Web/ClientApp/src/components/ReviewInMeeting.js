import React, { Component } from "react";
import "./App.css";
import * as microsoftTeams from "@microsoft/teams-js";
import { Loader } from "@fluentui/react-northstar";

export class ReviewInMeeting extends Component {
  static displayName = ReviewInMeeting.name;

  constructor(props) {
    super(props);
    this.state = {
      context: {},
      ssoToken: "",
      loading: true,
      loadingLabel: "Processing...",
      consentProvided: false,
      error: false,
    };

    // bind functions.
    this.reviewDashboard = this.reviewDashboard.bind(this);
    this.ssoLoginSuccess = this.ssoLoginSuccess.bind(this);
    this.initiateSSO = this.initiateSSO.bind(this);
  }

  componentDidMount() {
    // Initialize teams library and set context.
    this.initialize();

    // SSO.
    this.initiateSSO();
  }

  componentDidUpdate = async (prevProps, prevState) => {
    // Check to see if SSO token is set.
    if (prevState.ssoToken === "" && this.state.ssoToken !== "") {
      this.reviewDashboard();
    }

    // Check to see if consent was granted.
    if (
      prevState.consentProvided === false &&
      this.state.consentProvided === true
    ) {
      this.reviewDashboard();
    }
  };

  render() {
    let loader = this.state.loading ? (
      <h1>
        <Loader label={this.state.loadingLabel} />
      </h1>
    ) : null;
    let errorMessage = this.state.error ? (
      <h1> {this.state.errorMessage}</h1>
    ) : null;
    return (
      <div className="container">
        {loader}
        {errorMessage}
      </div>
    );
  }

  initialize() {
    // Initialize the Microsoft Teams SDK
    microsoftTeams.initialize(() => {
      // Get the user context from Teams and set it in the state
      microsoftTeams.getContext((context, error) => {
        this.setState({ context: context });
      });

      // Notify sucess.
      microsoftTeams.appInitialization.notifySuccess();
    });
  }

  initiateSSO() {
    // Get SSO token.
    let authTokenRequestOptions = {
      successCallback: (ssoToken) => {
        this.ssoLoginSuccess(ssoToken);
      },
      failureCallback: (errorMessage) => {
        this.ssoLoginFailure(errorMessage);
      },
    };

    microsoftTeams.authentication.getAuthToken(authTokenRequestOptions);
  }

  ssoLoginSuccess = async (ssoToken) => {
    console.log("SSO Completed");
    this.setState({ ssoToken: ssoToken });
  };

  ssoLoginFailure(errorMessage) {
    console.error("Failed to get Teams SSO token. Error: ", errorMessage);
    this.setState({
      error: true,
      loading: false,
      errorMessage: "Failed to fetch SSO token. Error: " + errorMessage,
    });
  }

  reviewDashboard = async () => {
    // Its either a group chat or a channel.
    let teamId = "";
    let conversationType = "groupChat";
    let conversationId = this.state.context["chatId"];
    if (this.state.context["teamId"]) {
      conversationType = "channel";
      conversationId = this.state.context["channelId"];
      teamId = this.state.context["teamId"];
    }

    let meetingId = this.state.context["meetingId"];
    let resourceId = this.props.id;
    let requestObject = {
      conversationType: conversationType,
      conversationId: conversationId,
      teamId: teamId,
      meetingId: meetingId,
      resourceId: resourceId,
    };

    this.setState({ loadingLabel: "Almost done..." });

    let serverURL = `${process.env.REACT_APP_BASE_URL}/api/setupMeeting`;
    var response = await fetch(serverURL, {
      method: "post",
      headers: {
        "Content-Type": "application/json",
        authorization: "bearer " + this.state.ssoToken,
      },
      body: JSON.stringify(requestObject),
    });

    if (!response.ok) {
      // error.
      if (response.status === 403) {
        this.askUserToConsent();
      } else {
        this.setState({
          error: true,
          loading: false,
          errorMessage: "Failed to setup meeting. Error:" + response,
        });
      }
    } else {
      let responseJson = await response.json();
      this.executeLinkAndClose(responseJson.joinMeetingLink);
    }
  };

  executeLinkAndClose(deepLink) {
    // Execute deeplink.
    microsoftTeams.executeDeepLink(deepLink);

    // Close task module.
    microsoftTeams.tasks.submitTask(undefined);
  }

  // Show a popup dialogue prompting the user to consent to the required API permissions. This opens AuthStart.js.
  // Learn more: https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-tab-aad#initiate-authentication-flow
  askUserToConsent() {
    microsoftTeams.authentication.authenticate({
      url: window.location.origin + "/auth-start",
      width: 600,
      height: 535,
      successCallback: (result) => {
        this.consentSuccess(result);
      },
      failureCallback: (reason) => {
        this.consentFailure(reason);
      },
    });
  }

  // Callback function for a successful authorization
  consentSuccess(result) {
    this.setState({
      consentProvided: true,
    });
  }

  consentFailure(reason) {
    console.error("Consent failed: ", reason);
    this.setState({
      error: true,
      loading: false,
      errorMessage:
        "Error: App doesn't have permissions to setup a meeting for you.",
    });
  }
}

export default ReviewInMeeting;
