// <copyright file="index.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React from "react";
import './index.css';

// App cache in meeting default page index.tsx
const Index = props => {
    return (
        <>
            <header class="header">
                <div class="header-inner-container">
                    <div class="header-icon" ></div>
                    <div class="header-text" >App Cache Sample In Meetings</div>
                </div>
            </header>
            <div class="row">
                <div class="main-content-area">
                    <div class="content-title">Your App is ready!</div>
                    <div class="main-text main-text-p1">
                        You can test your app<br />
                        by installing the manifest in Teams.
                    </div>
                </div>
            </div>
            <div class="ms-logo-container">
                <div class="ms-logo"></div>
            </div>
        </>
    );
};

export default Index;