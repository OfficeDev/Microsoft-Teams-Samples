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
import AppCacheTab from "../components/app-cache-tab";
import Index from "../components/index";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Index />} />
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/appCacheTab" element={<AppCacheTab />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};