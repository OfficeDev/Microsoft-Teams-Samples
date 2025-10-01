// <copyright file="home.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect } from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import $ from "jquery";

// Handles redirection after successful/failure sign in attempt.
const Home = props => {

    useEffect(() => {
        (async function () {
            await microsoftTeams.app.initialize();
            })();
        }, []);

    // Method to check validate form and add the entered details.
    const sendTaskDetails = (event) => {
        var taskDescription = document.querySelector(".task-description")
        var userName = document.querySelector(".user-name")
        var isValid = true;

        $('.task-description,.user-name').each(function (e) {
            if ($.trim($(this).val()) == '') {
                isValid = false;
                $(this).css({
                    "border": "1px solid red"
                });
            }
            else {
                $(this).css({
                    "border": "",
                    "background": ""
                });
            }
        });

        if (isValid == false) {
            event.preventDefault();
            return false;
        }

        // Meeting participant's entered details.
        const taskDetails = {
            "taskDescription": taskDescription.value,
            "userName": userName.value
        };

        microsoftTeams.dialog.url.submit(taskDetails);
        return true;
    }

    return (
        <form className="chat-form" onSubmit={(event) => { return sendTaskDetails(event) }}>
            <div className="chat-label">
                Assigned To:
                <input type="text" className="user-name" />
            </div>
            <div className="chat-label">
                Description:&nbsp;
                <input type="text" className="task-description" />
            </div>
            <input type="submit" id="addDetails" value="Add" />
        </form>
    );
};

export default Home;