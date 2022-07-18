import React, { Component, Fragment, useState, useEffect } from 'react';
import "regenerator-runtime/runtime.js";   //to enable the use of async

import TokenActionButtons from '../components/TokenActionButtons';
import TokenIndicator from '../components/TokenIndicator';
import UserList from '../components/UserList';
import ErrorMessageBar from '../components/ErrorMessageBar';

import { withMeetingTokenService } from '../context/MeetingServiceProvider';
import { TeamsFluidClient } from "@microsoft/live-share";
import { SharedMap } from "fluid-framework";
import Constants from '../constants';
import * as microsoftTeams from "@microsoft/teams-js";

let containerValue;
const SidePanel = props => {
    const editorValueKey = "meeting-meta-data";
    const [currentToken, setCurrentToken] = useState(0);
    const [userToken, setUserToken] = useState({
        number: null,
        status: Constants.MeetingTokenStatus.NotUsed
    });
    const [user, setUser] = useState({
        oid: "",
        name: "",
        isOrganizer: false,
    });
    const [teamsContext, setTeamsContext] = useState({});
    const [participants, setParticipants] = useState([]);
    const [customError, setCustomError] = useState({
        status: false,
        msg: "",
    });

    const setErrorFactory = (msg) => ({ status: true, msg });

    const clearErrorFactory = () => ({ status: false, msg: "" });

    const setError = (msg) => {
        setCustomError(setErrorFactory(msg))
    }

    useEffect(() => {
        onInitializeContianer();
    }, [])

    const onInitializeContianer = () => {
        (async function () {
            window.localStorage.debug = "fluid:*";

            // Define Fluid document schema and create container
            const client = new TeamsFluidClient();

            try {

                const containerSchema = {
                    initialObjects: { tokenMap: SharedMap }
                };

                let token = await updateCurrentToken(false);

                // Joining the container with default schema defined.
                const { container } = await client.joinContainer(containerSchema, (cont) => cont.initialObjects.tokenMap.set(editorValueKey, token));
                containerValue = container;
                containerValue.initialObjects.tokenMap.on("valueChanged", updateEditorState);
            }
            catch (err) {
                console.log(err)
            };
        })();
    }

    // This method is called whenever the shared state is updated.
    const updateEditorState = async () => {
        const currentToken = containerValue.initialObjects.tokenMap.get(editorValueKey);

        if (typeof (currentToken) === "number") {
            let res = await props.meetingTokenService.getMeetingStatusAsync()
            _updateDashboard(res.success, res.msg, false);
        }
    };

    // Handler called when user types on the editor.
    const updateState = (currentToken) => {
        var tokenMap = containerValue.initialObjects.tokenMap;
        tokenMap.set(editorValueKey, currentToken);
    }

    const onGetToken = ({ success, msg }) => {
        if (!success) {
            setError(msg);
            return;
        }

        const { AadObjectId, Name, Role: { MeetingRole } } = msg.UserInfo;

        setUser({ oid: AadObjectId, name: Name, isOrganizer: MeetingRole === Constants.MeetingRoles.Organizer });
        setUserToken({ number: msg.TokenNumber, status: msg.Status || Constants.MeetingTokenStatus.NotUsed });
        setCustomError(clearErrorFactory());

        updateCurrentToken(true);
    }

    const updateCurrentToken = async (shouldUpdate) => {
        let res = await props.meetingTokenService.getMeetingStatusAsync();

        _updateDashboard(res.success, res.msg, shouldUpdate);
        const { MeetingMetadata: { CurrentToken } } = res.msg;

        return CurrentToken;
    }

    const onAckToken = ({ success, msg }) => {
        _updateDashboard(success, msg, true);
    }

    const onSkipToken = ({ success, msg }) => {
        _updateDashboard(success, msg, true);
    }

    const onUserInfoFetched = ({ success, msg }) => {
        if (!success) {
            setError(msg);
            return;
        }

        const { AadObjectId, Name, Role: { MeetingRole } } = msg;
        setUser({ oid: AadObjectId, name: Name, isOrganizer: MeetingRole === Constants.MeetingRoles.Organizer });
        setCustomError(clearErrorFactory());
    }

    const _updateDashboard = (success, data, shouldUpdateState) => {
        if (!success) {
            setError(data);
            return;
        }
        const { MeetingMetadata: { CurrentToken }, UserTokens: participants } = data;
        microsoftTeams.app.getContext().then((context) => {
            const currentUser = participants.find(participant => participant.UserInfo.AadObjectId === context.user.id) || {};
            setCurrentToken(CurrentToken);
            setParticipants(participants);
            setUserToken({
                number: currentUser.TokenNumber,
                status: currentUser.Status || Constants.MeetingTokenStatus.NotUsed
            });
            setCustomError(clearErrorFactory());
            if (shouldUpdateState) {
                updateState(CurrentToken);
            }
        });
    }

    return (
        <Fragment>
            <div className="app-container">
                <ErrorMessageBar msg={customError.msg} show={customError.status} />
                <TokenIndicator show={true} value={participants.length && currentToken} title={"Current Token"} />
                <TokenActionButtons isOrganizer={user.isOrganizer}
                    onGetToken={onGetToken}
                    onAckToken={onAckToken}
                    onSkipToken={onSkipToken}
                    onUserInfoFetched={onUserInfoFetched}
                    showTokenButton={!userToken.number}
                    showDoneButton={!!userToken.number}
                    showSkipButton={!!participants.length}
                />
                <TokenIndicator
                    value={userToken.number}
                    show={!!userToken.number}
                    title={`Your Token: ${userToken.status}`}
                />
            </div>
            <UserList items={participants} />
        </Fragment>
    );
}

export default withMeetingTokenService(SidePanel);