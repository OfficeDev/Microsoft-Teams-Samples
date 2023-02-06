import React, { Component, Fragment } from 'react';

import SurfaceSelector from './SurfaceSelector';
import MeetingServiceProvider from '../context/MeetingServiceProvider';
import TeamsContextProvider from '../context/TeamsContextProvider';

class MeetingTokenApp extends Component {
    constructor(props) {
        super(props);
    }

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