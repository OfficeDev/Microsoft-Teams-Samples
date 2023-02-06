import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import Dashboard from "../components/dashboard";
import Start from "../components/start";
import End from "../components/end";

export const AppRoute = () => {

    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/dashboard" element={<Dashboard />} />
                    <Route path="/auth-start" element={<Start />} />
                    <Route path="/auth-end" element={<End />} />
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};