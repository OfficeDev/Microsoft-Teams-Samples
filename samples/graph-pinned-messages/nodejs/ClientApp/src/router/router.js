import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import Dashboard from "../components/dashboard";

export const AppRoute = () => {

    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/dashboard" element={<Dashboard />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};