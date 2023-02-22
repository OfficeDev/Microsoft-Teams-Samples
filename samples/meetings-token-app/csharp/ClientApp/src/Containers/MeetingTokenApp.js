import React, { Component, Fragment } from 'react';

import SurfaceSelector from './SurfaceSelector';
import MeetingServiceProvider from '../Context/MeetingServiceProvider';
import TeamsContextProvider from '../Context/TeamsContextProvider';

class MeetingTokenApp extends Component {

    render() {
        return (
            <Fragment>
                <MeetingServiceProvider>
                    <TeamsContextProvider>
                        <SurfaceSelector />
                    </TeamsContextProvider>
                </MeetingServiceProvider>
            </Fragment>
        );
    }
}

export default MeetingTokenApp;