import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import Dashboard from "../components/dashboard";
import CreateTag from "../components/create-tag";
import UploadFile from "../components/FileUpload";
import CreateEvent from "../components/create-event";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/configure" element={<Configure />} />
                    <Route path="/dashboard" element={<Dashboard />} />
                    <Route path="/create-new-tag" element={<CreateTag />} />
                    <Route path="/fileupload" element={<UploadFile />} />
                    <Route path="/create-event" element={<CreateEvent />} />
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};