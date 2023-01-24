// <copyright file="share-to-meeting.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from "react";

// Default browser page from where content can directly shared to meeting.
const ShareToMeeting = props => {

    useEffect(() => {
        const script = document.createElement("script");
        script.src = "https://teams.microsoft.com/share/launcher.js";
        document.body.appendChild(script);
    }, []);

    return (
        <>
            <div class="surface">
                <h3 id="tabheader"> Share to Meeting Page</h3>
                <img id="reportimg" src="/report.png" width="500" height="425" />
                <div
                    class="teams-share-in-meeting-button"
                    data-href="<Application-Base-URL>"
                    data-app-id="<Application-ID>"
                    data-entity-name="meeting-test-app"
                    data-button-type="medium"
                    data-icon-px-size="160"
                    data-entity-description="meeting test page to showcase the use of share to meeting functionality"
                >
                </div>
            </div>
        </>
    );
};

export default ShareToMeeting;