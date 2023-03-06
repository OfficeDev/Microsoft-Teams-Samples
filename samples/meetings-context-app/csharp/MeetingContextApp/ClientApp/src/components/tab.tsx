import React from 'react';
import { app, meeting } from "@microsoft/teams-js";
import { Text } from '@fluentui/react-northstar'
import ReactJson from 'react-json-view';
import './tab.css'

const Tab = () => {

    React.useEffect(() => {
        app.initialize();
        meeting.getMeetingDetails((error, meetingDetails: any) => {
            console.log(JSON.stringify(meetingDetails));
            setTabContext(meetingDetails);
        });
    }, [])

    const [tabContext, setTabContext] = React.useState({});

    return (
        <div className="tab-container">
            <div className="header-container">
                <Text weight="bold" size="larger" content="Meeting context" />
            </div>
            <br></br>
            <div>
                <ReactJson src={tabContext} theme="summerfruit:inverted" />
            </div>
        </div>
    )
}

export default (Tab);
