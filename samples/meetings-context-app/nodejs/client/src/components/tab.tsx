import React from 'react';
import { app, meeting } from "@microsoft/teams-js";
import { Text } from '@fluentui/react-components'
import ReactJson from 'react-json-view';
import './tab.css'

const Tab = () => {

    const [tabContext, setTabContext] = React.useState({});

    React.useEffect(() => {
        (async () => {
            try {
                await app.initialize();
                
                const context = await app.getContext();
                
                // Display the entire context object
                setTabContext(context);
            } catch (error) {
                console.error('Error initializing app:', error);
                setTabContext({ error: String(error) });
            }
        })();
    }, [])

    return (
        <div className="tab-container">
            <div className="header-container">
                <Text size={600} weight="bold">{"Meeting context"}</Text> 
            </div>
            <br></br>
            <div>
                <ReactJson src={tabContext} theme="summerfruit:inverted" />
            </div>
        </div>
    )
}

export default (Tab);