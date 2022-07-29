import React from 'react';
import { app, monetization } from "@microsoft/teams-js";
import { Text, Button, Flex } from '@fluentui/react-northstar'
import './tab.css'

const Tab = () => {

    React.useEffect(() => {
        app.initialize();
    }, [])


    var planInfo = {
        planId: "<Plan id>", // Plan Id of the published SAAS Offer
        term: "<Plan Term>" // Term of the plan.
    }

    const handlePurchaseDialog = () => {
        // This method will invoke the purchase
        monetization.openPurchaseExperience(planInfo);
    }

    return (
        <div className="tab-container">
            <div className="header-container">
                <Text weight="bold" size="larger" content="App monetization" />
            </div>
            <br></br>
            <Flex>
                <Text weight="regular" content="This app showcases how to trigger the purchase experience within the app. Refer below code. Click on the upgrade button to open purchase dialog." />
            </Flex>
            <br></br>
            <pre className="code-block">monetization.openPurchaseExperience(planInfo) where planInfo is an object with properties planId and term.
 </pre>
            <Button onClick={handlePurchaseDialog} content="Upgrade" />
        </div>
    )
}

export default (Tab);