// <copyright file="router.js" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import Index from "../components/index";
import TabAppNavigation from "../components/tabAppNavigation";
import TabOne from "../components/tabOne";
import TabTwo from "../components/tabTwo";
import TabThree from "../components/tabThree";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Index />} />
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/default_tab" element={<TabAppNavigation />}/>
                    <Route path="/tab_one" element={<TabOne />}/>
                    <Route path="/tab_two" element={<TabTwo />}/>
                    <Route path="/tab_three" element={<TabThree />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};