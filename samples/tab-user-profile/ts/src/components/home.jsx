// <copyright file="home.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React from "react";
import './home.css';

// Mail Module in Teams Outlook default page home.tsx
const Home = props => {
    return (
        <>
            <header class="header">
                <div class="header-inner-container">
                    <div class="header-icon" ></div>
                    <div class="header-text" >Tab Profile Module</div>
                </div>
            </header>
            <div class="row">
                <div class="main-content-area">
                    <div class="content-title">Your App is ready!</div>
                    <div class="main-text main-text-p1">
                        Install the manifest in Micorosft Teams<br />
                        Test your app in Outlook web/desktop.
                    </div>
                </div>
            </div>
            <div class="ms-logo-container">
                <div class="ms-logo"></div>
            </div>
        </>
    );
};

export default Home;