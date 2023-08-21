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
import TabToNavigation from "../components/tabToNavigate";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Index />} />
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/tabAppNavigation" element={<TabAppNavigation />}/>
                    <Route path="/navigatedTab" element={<TabToNavigation />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};