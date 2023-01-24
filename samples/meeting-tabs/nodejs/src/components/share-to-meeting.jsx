// <copyright file="share-to-meeting.jsx" company="Microsoft Corporation">
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
                <h3 id="tabheader"> Welcome Meeting Tab</h3>
		    </div>
		</>
    );
};

export default ShareToMeeting;