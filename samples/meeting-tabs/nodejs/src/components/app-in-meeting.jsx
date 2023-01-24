import React, { Component } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, FlexItem, Button } from "@fluentui/react-northstar";

class AppInMeeting extends Component {
    constructor(props) {
        super(props)
        this.state = {
            errorCode: "",
            result: ""
        }
    }
    componentDidMount() {
        microsoftTeams.app.initialize();
    }

    callback = (errcode, bln) => {
        alert("callback method called");
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

    // Share the content to meeting stage view.
    shareSpecificPart = () => {
        var appContentUrl = "";
        microsoftTeams.app.getContext().then((context) => {
            appContentUrl = `${window.location.origin}/shareview?meetingId=${context.meeting.id}`;
            microsoftTeams.meeting.shareAppContentToStage((err, result) => {
                if (result) {
                    // handle success
                    alert("Shared successfully!");
                }
                if (err) {
                    // handle error
                    alert(+JSON.stringify(err))
                }
            }, appContentUrl);
        });
    };

    render() {
        return (
            <>
                <Flex>
                    <FlexItem push>
                        <div className="tag-container">
                            <h3>Share To Stage View</h3>
                            <Button primary content="Share" onClick={this.shareSpecificPart} />
                        </div>
                    </FlexItem>
                </Flex>
                <Flex>
                    <FlexItem push>
                        <div className="tag-container">
                            <h3>Mute/Unmute Audio Call </h3>
                            <Button primary content="Mute/Un-Mute" onClick={this.toggleState} />
                            <li className="break"> Mute State: <b>{this.state.result}</b></li>
                        </div>
                    </FlexItem>
                </Flex>
            </>
        )
    }
}
export default AppInMeeting