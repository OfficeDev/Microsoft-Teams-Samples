// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";
import "./App.css";
import * as microsoftTeams from "@microsoft/teams-js";

// This tab component renders the main tab content of your app.
export interface IAllItemsDetail {
  id: string;
  subject: string;
}

export interface ITabProps {}
interface ITabState {
  context?: microsoftTeams.app.Context;
  ssoToken: string;
  consentRequired: boolean;
  consentProvided: boolean;
  graphAccessToken: string;
  error: boolean;
  stateDatetime: string;
  endDatetime: string;
  attendees: string;
  subject: string;
  content: string;
  allItems: IAllItemsDetail[];
  platformIsSupported:boolean;
}

class Tab extends React.Component<ITabProps, ITabState> {
  constructor(props: ITabProps) {
    super(props);
    this.state = {
      context: undefined,
      ssoToken: "",
      consentRequired: false,
      consentProvided: false,
      graphAccessToken: "",
      error: false,
      stateDatetime: "",
      endDatetime: "",
      attendees: "",
      subject: "",
      content: "",
      allItems: [],
      platformIsSupported:false
    };

    // Bind any functions that need to be passed as callbacks or used to React components
    this.attendeesHandleChange = this.attendeesHandleChange.bind(this);
    this.composeMeeting = this.composeMeeting.bind(this);
    this.startHandleChange = this.startHandleChange.bind(this);
    this.endHandleChange = this.endHandleChange.bind(this);
    this.subjectHandleChange = this.subjectHandleChange.bind(this);
    this.contentHandleChange = this.contentHandleChange.bind(this);
    this.consentSuccess = this.consentSuccess.bind(this);
    this.consentFailure = this.consentFailure.bind(this);
    this.unhandledFetchError = this.unhandledFetchError.bind(this);
    this.callGraphFromClient = this.callGraphFromClient.bind(this);
    this.showConsentDialog = this.showConsentDialog.bind(this);
  }

  // React lifecycle method that gets called once a component has finished mounting
  // Learn more: https://reactjs.org/docs/react-component.html#componentdidmount
  componentDidMount() {
    // Initialize the Microsoft Teams SDK
    microsoftTeams.app.initialize().then(() => {
      // Get the user context from Teams and set it in the state
      microsoftTeams.app
        .getContext()
        .then((context: microsoftTeams.app.Context) => {
          this.setState({ context: context });
        });

      microsoftTeams.authentication
        .getAuthToken()
        .then((result: string) => {
          this.setState({ ssoToken: result });
          this.exchangeClientTokenForServerToken(result);
        })
        .catch((error: string) => {
          console.error("SSO failed: ", error);
        });
    });
  }

  // Exchange the SSO access token for a Graph access token
  // Learn more: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow
  exchangeClientTokenForServerToken = async (token: string) => {
    
    let serverURL = `${process.env.REACT_APP_BASE_URL}/getGraphAccessToken?ssoToken=${token}&upn=${this.state.context?.user?.userPrincipalName}`;
    let response = await fetch(serverURL).catch(this.unhandledFetchError); //This calls getGraphAccessToken route in /api-server/app.js
    
    if (response) {
      let data = await response.json().catch(this.unhandledFetchError);
      
      if (!response.ok && data.error === "consent_required") {
        // A consent_required error means it's the first time a user is logging into to the app, so they must consent to sharing their Graph data with the app.
        // They may also see this error if MFA is required.
        this.setState({ consentRequired: true }); //This displays the consent required message.
        this.showConsentDialog(); //Proceed to show the consent dialogue.
      } else if (!response.ok) {
        // Unknown error
        console.error(data);
        this.setState({ error: true });
      } else {
        this.setState({ allItems: data.value });
      }
    }
  };

  // Show a popup dialogue prompting the user to consent to the required API permissions. This opens ConsentPopup.js.
  // Learn more: https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-tab-aad#initiate-authentication-flow
  showConsentDialog() {
    microsoftTeams.authentication
      .authenticate({
        url: window.location.origin + "/auth-start",
        width: 600,
        height: 535,
      })
      .then((result: string) => {
        this.consentSuccess(result ?? "");
      })
      .catch((reason: string) => {
        this.consentFailure(reason ?? "");
      });
  }

  // Callback function for a successful authorization
  consentSuccess(result: string) {
    // Save the Graph access token in state
    this.setState({
      graphAccessToken: result,
      consentProvided: true,
    });
  }

  consentFailure(reason: string) {
    console.error("Consent failed: ", reason);
    this.setState({ error: true });
  }

  // React lifecycle method that gets called after a component's state or props updates
  // Learn more: https://reactjs.org/docs/react-component.html#componentdidupdate
  componentDidUpdate = async (prevProps: ITabProps, prevState: ITabState) => {
    // Check to see if a Graph access token is now in state AND that it didn't exist previously
    if (
      prevState.graphAccessToken === "" &&
      this.state.graphAccessToken !== ""
    ) {
      this.callGraphFromClient();
    }
  };

  // Fetch the user's meeting event from Graph using the access token retrieved either from the server
  // or microsoftTeams.authentication.authenticate
  callGraphFromClient = async () => {
    let upn = this.state.context?.user?.userPrincipalName;
    let graphEndpoint = `https://graph.microsoft.com/v1.0/users/${upn}/events?$select=subject,body,bodyPreview,organizer,attendees,start,end,location`;
    let graphRequestParams = {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        authorization: "bearer " + this.state.graphAccessToken,
      },
    };
    let response = await fetch(graphEndpoint, graphRequestParams).catch(
      this.unhandledFetchError
    );

    if (response) {
      let data = await response.json().catch(this.unhandledFetchError);
      
      if (response.ok) {
        this.setState({ allItems: data.value });
      } else {
        console.error("ERROR: ", response);
        this.setState({ error: true });
      }
    }
  };

  // Generic error handler ( avoids having to do async fetch in try/catch block )
  unhandledFetchError(err: string) {
    console.error("Unhandled fetch error: ", err);
    this.setState({ error: true });
  }

  startHandleChange(ev: any) {
    if (!ev.target["validity"].valid) return;
    const dt = ev.target["value"] + ":00Z";
    this.setState({ stateDatetime: dt });
  }

  endHandleChange(ev: any) {
    if (!ev.target["validity"].valid) return;
    const dt = ev.target["value"] + ":00Z";
    this.setState({ endDatetime: dt });
  }

  subjectHandleChange(ev: any) {
    const subjectData = ev.target.value;
    this.setState({ subject: subjectData });
  }

  attendeesHandleChange(ev: any) {
    const attendeesData = ev.target.value;
    this.setState({ attendees: attendeesData });
  }
  
  contentHandleChange(ev: any) {
    const setContentData = ev.target.value;
    this.setState({ content: setContentData });
  }

  openCalendar(itemIds: any) {
    var OpenCalendarItemParams = {
      itemId: itemIds
    };
    if (microsoftTeams.calendar.isSupported()) {
      microsoftTeams.calendar.openCalendarItem(OpenCalendarItemParams);
    } else {
      this.setState({ platformIsSupported: true });
    }
  }
  
  composeMeeting() {
    if (microsoftTeams.calendar.isSupported()) {
    var isValidation = false;
    if (this.state.subject !== undefined &&
      this.state.subject !== null &&
      this.state.subject !== "" &&
      this.state.content !== undefined &&
      this.state.content !== null &&
      this.state.content !== "" &&
      this.state.attendees !== undefined &&
      this.state.attendees !== null &&
      this.state.attendees !== "" &&
      this.state.stateDatetime !== undefined &&
      this.state.stateDatetime !== null &&
      this.state.stateDatetime !== "" &&
      this.state.endDatetime !== undefined &&
      this.state.endDatetime !== null &&
      this.state.endDatetime !== "" ) 
      {
        isValidation = true;
      }
      if (isValidation == true) {
        var ComposeMeetingParams = {
          attendees: [this.state.attendees],
          startTime: this.state.stateDatetime, // MM/DD/YYYY HH:MM:SS format
          endTime: this.state.endDatetime, // MM/DD/YYYY HH:MM:SS format
          subject: this.state.subject,
          content: this.state.content,
        };
          microsoftTeams.calendar.composeMeeting(ComposeMeetingParams);
      }
    }
    else{
      this.setState({ platformIsSupported: true });
    }
  }
  render() {
    return (
      <div className="moduleDiv">
      <h3>Compose Meeting Module</h3>
      <form>
        <table className="tblMain">
          <tr>
            <td>
            <label className="lblText">Attendees:</label>
              <input
                type="text"
                className="inputValue"
                placeholder="Attendees"
                required
                value={(this.state.attendees || "").toString()}
                onChange={this.attendeesHandleChange.bind(this)}
              />
            </td>
          </tr>
          <tr>
            <td>
            <label className="lblText">Start Time:</label>
              <input
                type="datetime-local"
                className="inputValue"
                required
                value={(this.state.stateDatetime || "")
                  .toString()
                  .substring(0, 16)}
                onChange={this.startHandleChange.bind(this)}
              />
            </td>
          </tr>
          <tr>
            <td>
            <label className="lblText">End Time:</label>
              <input
                type="datetime-local"
                className="inputValue"
                required
                value={(this.state.endDatetime || "")
                  .toString()
                  .substring(0, 16)}
                onChange={this.endHandleChange.bind(this)}
              />
            </td>
          </tr>
          <tr>
            <td>
            <label className="lblText">Subject:</label>
              <input
                type="text"
                className="inputValue"
                placeholder="Subject"
                value={(this.state.subject || "").toString()}
                onChange={this.subjectHandleChange.bind(this)}
                required
              />
            </td>
          </tr>
          <tr>
            <td>
            <label className="lblText">Content:</label>
              <textarea
                required
                className="inputValue"
                placeholder="Type your meeting content..."
                rows={4}
                cols={40}
                value={(this.state.content || "").toString()}
                onChange={this.contentHandleChange.bind(this)}
              />
            </td>
          </tr>
          <tr>
            <td>
              <button className="btnSubmit" onClick={this.composeMeeting.bind(this)}>
                Compose Meeting!
              </button>
            </td>
          </tr>
        </table>
        </form>
      <div>
        <h3>Opens calendar Module</h3>
        <table id="calendar">
          <tr>
            <th>Subject</th>
            <th>Event</th>
          </tr>
          {this.state.allItems
            .slice(0, 3)
            .map((value: IAllItemsDetail, index) => {
              return (
                <tr>
                  <td className="tdSubject">
                    {value.subject.substring(0, 30)}
                  </td>
                  <td>
                    <button className="btnSubmit" onClick={() => this.openCalendar(value.id)}>
                      View!
                    </button>
                  </td>
                </tr>
              );
            })}
        </table>
        {this.state.platformIsSupported ? <span style={{ color: 'red',marginLeft:2 }}>Sorry, This app is currently not supported on this platform.</span> : ""}
      </div>
    </div>
    );
  }
}
export default Tab;
