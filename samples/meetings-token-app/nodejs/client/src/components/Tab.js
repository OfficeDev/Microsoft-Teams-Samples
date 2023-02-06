// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";
import axios from 'axios';

/**
 * The 'GroupTab' component renders the main tab content
 * of your app.
 */
class Tab extends React.Component {
  constructor(props){
    super(props)
    this.state = {
      context: {},
      token: ''
    }
  }

  
  //React lifecycle method that gets called once a component has finished mounting
  //Learn more: https://reactjs.org/docs/react-component.html#componentdidmount
  componentDidMount(){
    // Get the user context from Teams and set it in the state
    microsoftTeams.app.getContext().then((context) => {
      console.log(context);
      this.setState({
        context: context
      });
    });

    microsoftTeams.authentication.getAuthToken().then((result) => {
        console.log("Success: " + result);
        this.setState({
            token: result
        });
    }).catch((error) => {
        console.log("Failure: " + error);
    });

    setTimeout(
      () => {
        console.log("componentDidMount");
        axios.post('/api/auth/token/', {
          tid: this.state.context.user.tenant.id,
          token: this.state.token
        })
        .then(function (response) {
          console.log(response.data);
          let accessToken = response.data;
          axios.post('/api/user/profile/', {
            accessToken: accessToken
          })
          .then(function (response) {
            console.log(response);            
          })
        })
      }, 
      2000
    );
    
    // Next steps: Error handling using the error object
  }

  render() {

      let userName = Object.keys(this.state.context).length > 0 ? this.state.context['upn'] : "";

      return (
      <div>
        <h3>Hello World!</h3>
        <h1>Congratulations {userName}!</h1> <h3>This is the tab you made :-)</h3>
      </div>
      );
  }
}
export default Tab;