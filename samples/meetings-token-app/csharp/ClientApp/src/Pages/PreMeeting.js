import React, { Component } from 'react';

import { withMeetingTokenService } from '../Context/MeetingServiceProvider';
import TokenIndicator from "../Components/TokenIndicator";

class PreMeeting extends Component {
    constructor(props) {
        super(props);

        this.state = {
            currentToken: 0,
            maxTokenIssued: 0,
            participantCount: "0",
        }
    }

    componentDidMount() {
        this.initializePage();
    }

    initializePage = async () => {
        let response = await this.props.meetingTokenService.getMeetingStatusAsync();

        if (!response.success) {
            this.props.onError(response.msg);
            return;
        }
        const { MeetingMetadata: { CurrentToken, MaxTokenIssued }, UserTokens } = response.msg;

        this.setState({
            currentToken: UserTokens.length > 0 ? CurrentToken : "N/A",
            maxTokenIssued: MaxTokenIssued,
            participantCount: (UserTokens.length || "0"),
        });
    }

    render() {
        return (
            <div className="app-container" >
                <TokenIndicator show={true} value={this.state.currentToken} title={"Current Token"} />
                <TokenIndicator show={true} value={this.state.maxTokenIssued} title={"Max Token Issued"} />
                <TokenIndicator show={true} value={this.state.participantCount} title={"Queue Length"} />
            </div>
        );
    }
}

export default withMeetingTokenService(PreMeeting);