import React, { Component, Fragment } from 'react';
import "regenerator-runtime/runtime.js";   //to enable the use of async

import TokenActionButtons from '../Components/TokenActionButtons';
import TokenIndicator from '../Components/TokenIndicator';
import UserList from '../Components/UserList';
import ErrorMessageBar from '../Components/ErrorMessageBar';

import { withMeetingTokenService } from '../Context/MeetingServiceProvider';
import StatusRefresher from '../Components/StatusRefresher';
import Constants from '../Constants';


class SidePanel extends Component {
    constructor(props) {
        super(props);
        this.state = {
            currentToken: null,
            userToken: {
                number: null,
                status: Constants.MeetingTokenStatus.NotUsed
            },
            user: {
                oid: "",
                name: "",
                isOrganizer: false,
            },
            participants: [],
            error: {
                status: false,
                msg: "",
            }
        };

        this.setErrorFactory = (msg) => ({ status: true, msg });
        this.clearErrorFactory = () => ({ status: false, msg: "" });

        this.setError = (msg) => this.setState({ error: this.setErrorFactory(msg) });
    }

    onGetToken = ({ success, msg }) => {
        if (!success) {
            this.setError(msg);
            return;
        }

        const { AadObjectId, Name, Role: { MeetingRole }} = msg.UserInfo;
        this.setState({
            user: { oid: AadObjectId, name: Name, isOrganizer: MeetingRole === Constants.MeetingRoles.Organizer },
            userToken: { number: msg.TokenNumber, status: msg.Status || Constants.MeetingTokenStatus.NotUsed },
            error: this.clearErrorFactory()
        });
    }

    onAckToken = ({ success, msg }) => {
        this._updateDashboard(success, msg);
    }

    onSkipToken = ({ success, msg }) => {
        this._updateDashboard(success, msg);
    }

    onStatusRefresh = ({ success, msg }) => {
        this._updateDashboard(success, msg);
    }

    onUserInfoFetched = ({ success, msg }) => {
        if (!success) {
            this.setError(msg);
            return;
        }
        const { AadObjectId, Name, Role: { MeetingRole }} = msg;
        this.setState({
            user: { oid: AadObjectId, name: Name, isOrganizer: MeetingRole === Constants.MeetingRoles.Organizer },
            error: this.clearErrorFactory()
        });
    }

    render() {
        return (
            <Fragment>
                <StatusRefresher onStatusRefresh={this.onStatusRefresh} />
                <div className="app-container">
                    <ErrorMessageBar msg={this.state.error.msg} show={this.state.error.status} />
                    <TokenIndicator show={true} value={this.state.participants.length && this.state.currentToken} title={"Current Token"} />
                    <TokenActionButtons isOrganizer={this.state.user.isOrganizer}
                        onGetToken={this.onGetToken}
                        onAckToken={this.onAckToken}
                        onSkipToken={this.onSkipToken}
                        onUserInfoFetched={this.onUserInfoFetched}
                        showTokenButton={!this.state.userToken.number}
                        showDoneButton={!!this.state.userToken.number}
                        showSkipButton={!!this.state.participants.length}
                    />
                    <TokenIndicator
                        value={this.state.userToken.number}
                        show={!!this.state.userToken.number}
                        title={`Your Token: ${this.state.userToken.status}`}
                    />
                </div>
                <UserList items={this.state.participants} />
            </Fragment>
        );
    }

    _updateDashboard(success, data) {
        if (!success) {
            this.setError(data);
            return;
        }
        const { MeetingMetadata: { CurrentToken }, UserTokens: participants } = data;
        const currentUser = participants.find(participant => participant.UserInfo.AadObjectId === this.state.user.oid) || {};
        this.setState({
            currentToken: CurrentToken,
            participants,
            userToken: {
                number: currentUser.TokenNumber,
                status: currentUser.Status || Constants.MeetingTokenStatus.NotUsed
            },
            error: this.clearErrorFactory(),
        });
    }
}

export default withMeetingTokenService(SidePanel);