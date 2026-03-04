import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';

import Login from "../components/login";
import UserScopeTestApp from "../components/userScopeTestApp";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/userScopeTestApp/" element={<UserScopeTestApp />}/>
                    <Route path="/" element={<Login />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};