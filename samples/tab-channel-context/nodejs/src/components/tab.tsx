import React from 'react';
import { app } from "@microsoft/teams-js";
import { Text } from "@fluentui/react-components"
import ReactJson from 'react-json-view';
import * as tabConstants from './constants/tab-constants';
import './tab.css'

let tabHeader = "";
let tabLink = "";

const Tab = () => {

    React.useEffect(() => {
        app.initialize();
        app.getContext().then((context) => {
            if (context.channel?.membershipType === "Private") {
                tabHeader = tabConstants.tabHeaderPrivateChannel;
                tabLink = tabConstants.privateChannelLink;
            }
            else if (context.channel?.membershipType === "Shared") {
                tabHeader = tabConstants.tabHeaderSharedChannel;
                tabLink = tabConstants.sharedChannelLink;
            }
            else {
                tabHeader = tabConstants.tabHeaderPublicChannel;
                tabLink = tabConstants.publicChannelLink;
            }
            
            setTabContext(context);
        })
    }, [])

    const [tabContext, setTabContext] = React.useState({});

    return (
        <div className="tab-container">
            <div className="header-container">
                <Text weight="bold" size={600}>{tabHeader}</Text>
            </div>
            <br></br>
            <div>
                <Text weight="regular">{"Click on the url to know more details about the context object."}</Text>
                <a href={tabLink} target="_blank" rel="noopener noreferrer">
                    <Text className="link-text" weight="regular" >{"Microsoft Teams Context Object." }</Text>
                </a>
            </div>
            <br></br>
            <div>
                <ReactJson src={tabContext} theme="summerfruit:inverted" />
            </div>
        </div>
    )
}

export default (Tab);