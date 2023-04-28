import React from 'react';
import { app, monetization } from "@microsoft/teams-js";
import { Text, Button } from '@fluentui/react-components'
import './tab.css'

const Tab = () => {

    React.useEffect(() => {
        app.initialize();
    }, [])


    var planInfo = {
        planId: "", // Plan Id of the published SAAS Offer
        term: "" // Term of the plan.
    }

    const handlePurchaseDialog = () => {
        // This method will invoke the purchase
        monetization.openPurchaseExperience(planInfo);
    }

    return (
        <div className="tab-container">
            <div className="header-container">
                <Text weight="bold" size={500} >{"App monetization"}</Text>
            </div>
            <br></br>
            <div>
                <Text weight="regular">
                    {"This app showcases how to trigger the purchase experience within the app. Refer below code. Click on the upgrade button to open purchase dialog."}
                </Text>
            </div>
            <br></br>
            <pre className="code-block">monetization.openPurchaseExperience(planInfo) where planInfo is an object with properties planId and term.
 </pre>
            <Button onClick={handlePurchaseDialog}>{"Upgrade"}</Button>
        </div>
    )
}

export default (Tab);