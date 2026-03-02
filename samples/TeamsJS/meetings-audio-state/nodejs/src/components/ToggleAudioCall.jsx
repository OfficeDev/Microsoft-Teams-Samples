import React, { Component } from "react";
import { Flex, FlexItem, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";

class ToggleAudioCall extends Component {

    constructor(props) {
        super(props)
        this.state = {
            errorCode: "",
            result: "",
            appTheme: ""
        }
    }

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {

                // Applying default theme from app context property
                switch (context.app.theme) {
                    case 'dark':
                        this.setState({ appTheme: "theme-dark" });
                        break;
                    case 'contrast':
                        this.setState({ appTheme: "theme-contrast" });
                        break;
                    case 'default':
                        this.setState({ appTheme: "theme-light" });
                        break;
                }
            }).bind(this);
        });
    }

    componentDidUpdate() {
        // handle theme when Teams theme changes to dark,light and contrast.
        microsoftTeams.app.registerOnThemeChangeHandler(function (theme) {
            switch (theme) {
                case 'dark':
                    this.setState({ appTheme: "theme-dark" });
                    break;
                case 'contrast':
                    this.setState({ appTheme: "theme-contrast" });
                    break;
                case 'default':
                    this.setState({ appTheme: "theme-light" });
                    break;
            }
        }.bind(this));
    }

    callback = (errcode, bln) => {
        if (errcode) {
            this.setState({ errorCode: JSON.stringify(errcode) })
        }
        else {
            this.setState({ result: JSON.stringify(bln) })
        }
    }

    /// <summary>
    /// This method getIncomingClientAudioState returns the current state of client audio.
    /// The incoming audio is muted if the result is true and unmuted if the result is false.
    /// </summary>
    ClientAudioState = () => {
        microsoftTeams.meeting.getIncomingClientAudioState(this.callback);
    }

    /// <summary>
    /// This method toggleIncomingClientAudio which toggles mute/unmute to client audio.
    /// Setting for the meeting user from mute to unmute or vice-versa.
    /// </summary>
    toggleState = () => {
        microsoftTeams.meeting.toggleIncomingClientAudio(this.callback);
    }

    render() {
        return (
            <Flex>
                <FlexItem push>
                    <div className={this.state.appTheme}>
                        <h3>Mute/Unmute Audio Call </h3>
                        <Button primary content="Mute/Un-Mute" onClick={this.toggleState} />
                        <li className="break"> Mute State : <b>{this.state.result}</b></li>
                    </div>
                </FlexItem>
            </Flex>
        )
    }
}

export default ToggleAudioCall