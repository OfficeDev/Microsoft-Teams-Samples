import React, { useContext } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import MeetingTokenService from "../Service/MeetingTokenService";
import Constants from "../Constants";

const MeetingSerivceContext = React.createContext({});

const MeetingServiceProvider = (props) => {
    const svc = new MeetingTokenService(microsoftTeams, {timeout: Constants.Service.timeout});
    return (
        <MeetingSerivceContext.Provider value={svc}>
            {props.children}
        </MeetingSerivceContext.Provider>
    );
}

export default MeetingServiceProvider;

export const withMeetingTokenService = Component => props => {
    const meetingTokenService = useContext(MeetingSerivceContext);
    return <Component {...props} meetingTokenService={meetingTokenService} />
}