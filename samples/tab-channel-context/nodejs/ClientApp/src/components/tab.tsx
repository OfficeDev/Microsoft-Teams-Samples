import React from 'react';
import { app } from "@microsoft/teams-js";
import { Text, Flex } from '@fluentui/react-northstar'
import ReactJson from 'react-json-view';
import * as tabConstants from './constants/tab-constants';
import './tab.css'

let tabHeader = "";
let tabLink = "";

const Tab = () => {

    React.useEffect(() => {
        app.initialize();
        app.getContext().then((context) => {
            if (context.channel?.membershipType == "Private") {
                tabHeader = tabConstants.tabHeaderPrivateChannel;
                tabLink = tabConstants.privateChannelLink;
            }
            else if (context.channel?.membershipType == "Shared") {
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
                <Text weight="bold" size="larger" content={tabHeader} />
            </div>
            <br></br>
            <Flex>
                <Text weight="regular" content="Click on the url to know more details about the contextv object." /><a href={tabLink} target="_blank" rel="noopener noreferrer"><Text className="link-text" weight="regular" content="Microsoft Teams Context Object." /></a>
            </Flex>
            <br></br>
            <div>
                <ReactJson src={tabContext} theme="summerfruit:inverted" />
            </div>
        </div>
    )
}

export default (Tab);