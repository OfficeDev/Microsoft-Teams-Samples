import * as React from "react";
import {
    BrowserRouter,
    Route,
    Routes
} from 'react-router-dom';
import Configure from "../components/configure";
import ToggleAudioCall from "../components/ToggleAudioCall";

export const AppRoute = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/configure" element={<Configure />}/>
                    <Route path="/ToggleAudioCall" element={<ToggleAudioCall />}/>
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};