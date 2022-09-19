// <copyright file="error-page.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import { Text } from "@fluentui/react-northstar";
import { useTranslation } from 'react-i18next';

import "../styles/site.scss";

// Renders error page with generic error message.
const ErrorPage: React.FunctionComponent = () => {
    const localize = useTranslation().t;

    /**
    * Renders the component
    */
    return (
        <div className="container-div">
            <div className="container-subdiv">
                <div className="error-message">
                    <Text content={localize("errorMessage")} error size="medium" />
                </div>
            </div>
        </div>
    );
}

export default ErrorPage;