import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import AppCacheTab from "../components/app-cache-tab";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/appCacheTab" element={<AppCacheTab />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};