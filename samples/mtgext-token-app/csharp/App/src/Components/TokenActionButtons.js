import React, { Component } from 'react';

import DoneTokenButton from './DoneTokenButton';
import GetTokenButton from './GetTokenButton';
import SkipCurrentTokenButton from './SkipCurrentTokenButton';

import { withMeetingTokenService } from '../Context/MeetingServiceProvider';

class TokenActionButtons extends Component {

    render() {
        return (
            <div style={{ justifyContent: "center", alignContent: "center", display: 'flex', rowGap: 30 }} >
                <GetTokenButton onGetToken={this.props.onGetToken} show={this.props.showTokenButton} />
                <DoneTokenButton onAckToken={this.props.onAckToken} show={this.props.showDoneButton}/>
                <SkipCurrentTokenButton
                    isOrganizer={this.props.isOrganizer && this.props.showSkipButton}
                    onSkipToken={this.props.onSkipToken}
                    onUserInfoFetched={this.props.onUserInfoFetched}
                />
            </div>
        );
    }
}

export default withMeetingTokenService(TokenActionButtons);