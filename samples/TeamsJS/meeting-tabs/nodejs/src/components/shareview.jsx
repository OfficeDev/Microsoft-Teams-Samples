// <copyright file="shareview.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from 'react'
import * as microsoftTeams from "@microsoft/teams-js";

class ShareView extends Component {
    constructor(props) {
        super(props);
        this.state = {
            seconds: 0,
            appTheme: ""
        };
    }

    componentDidMount() {
        let app = microsoftTeams.app;
        app.initialize().then(app.getContext).then((context) => {
            // Applying default theme from app context property
            switch (context.app.theme) {
                case 'dark':
                    this.setState({ appTheme: "timerCount-dark" })
                    break;
                case 'contrast':
                    this.setState({ appTheme: "timerCount-contrast" })
                    break;
                case 'default':
                    this.setState({ appTheme: "timerCount-light" })
                    break;
                default:
                    return this.setState({ appTheme: "timerCount-dark" })
            }
        });

        // counts time
        this.timer();
    }

    //The onStart function increments the value of the state variable, seconds, by one and updates it using setState.
    onStart = () => {
        this.setState({ seconds: this.state.seconds + 1 });
    }

    timer = () => {
        this.f = setInterval(this.onStart, 1000);
    }

    render() {
        return (
            <div className={this.state.appTheme}>
                <h1>{this.state.seconds}</h1>
            </div>
        )
    }
}

export default ShareView