// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './tab.css';
import { app, authentication } from "@microsoft/teams-js";
// Bootstrap CSS
import "bootstrap/dist/css/bootstrap.min.css";
// Bootstrap Bundle JS
import "bootstrap/dist/js/bootstrap.bundle.min";

/**
 * The 'PersonalTab' component renders the main tab content
 * of your app.
 */
class Tab extends React.Component {
  constructor(props) {
    super(props)
    this.state = {
      name: "",
      photo: "https://media.istockphoto.com/vectors/profile-placeholder-image-gray-silhouette-no-photo-vector-id1016744034?k=20&m=1016744034&s=612x612&w=0&h=kjCAwH5GOC3n3YRTHBaLDsLIuF8P3kkAJc9RvfiYWBY=",
      email: ""
    }
    this.getGoogleIdToken = this.getGoogleIdToken.bind(this);
    this.googleAuth = this.googleAuth.bind(this);
    this.getGoogleServerSideToken = this.getGoogleServerSideToken.bind(this);
  }

  async getGoogleServerSideToken(clientSideToken) {

    let serverURL = `${window.location.origin}/getGraphAccessToken?idToken=${clientSideToken}`;
    let response = await fetch(serverURL).catch(this.unhandledFetchError); //This calls getGraphAccessToken route in /api-server/app.js
    let data = await response.json().catch(this.unhandledFetchError);
    this.setState({
      name: data.name,
      photo: data.photo,
      email: data.email
    });
  }

  getGoogleIdToken() {
    var googleId = process.env.REACT_APP_GOOGLE_APP_ID; // Google app client id

    // Redirect url path
    var url = window.location.origin + "/auth-end";
    return new Promise((resolve, reject) => {
      app.initialize().then(() => {
        authentication.authenticate({
          url: `${window.location.origin}/auth-start?oauthRedirectMethod={oauthRedirectMethod}&authId={authId}&hostRedirectUrl=${url}&googleId=${googleId}`,
          isExternal: true
        }).then((result) => {
          this.getGoogleServerSideToken(result);
        }).catch((reason) => {
          console.log("failed" + reason);
          reject(reason);
        })
      })
    })
  }

  googleAuth() {
    this.getGoogleIdToken()
      .then((result) => {
        return this.getGoogleServerSideToken(result);
      })
      .catch((error) => {
        console.log(error);
      });
  }

  render() {
    return (
      <div className="surface">
        <h3> Welcome to Tab External Auth Sample</h3>
        <div className="card-container-div">
          <div>
            <div className="signin-header"><b>Sign in to Google</b></div>
            <div className="card">
              <div className="container">
                <img src={this.state.photo}
                  alt="Avatar" id="userImgGoogle" width="100px" height="100px" />
                <div className="profile">
                  <span id="gname"><b>Name: {this.state.name}</b></span><br />
                  <span id="gemail"><b>Email: {this.state.email}</b></span><br />
                </div>
              </div>
              <div id="divError" style={{ display: "none" }}></div>
              <button type="button" className="btn btn-outline-info" onClick={this.googleAuth} id="googlelogin">
                Get
                Details
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }
}
export default Tab;