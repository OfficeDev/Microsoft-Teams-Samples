import React, { Component, Fragment } from 'react';

import ErrorMessageBar from '../Components/ErrorMessageBar';
import SidePanelPage from '../Pages/SidePanel';
import PreMeetingPage from '../Pages/PreMeeting';
import { withTeamsContext } from '../Context/TeamsContextProvider';
import Constants from '../Constants';

class SurfaceSelector extends Component {
    constructor(props) {
        super(props);
        this.state = {
            error: {
                status: false,
                msg: "",
            }
        };
    }

    render() {
        if (this.state.error.status) {
            return <ErrorMessageBar msg={this.state.error.msg} show={this.state.error.status} />
        }

        const { SidePanel, PreMeeting } = Constants.Surfaces;
        if (this.props.teamsContext?.page) {
            const frameContext = this.props.teamsContext.page.frameContext;
            switch (frameContext) {
                case SidePanel:
                    return <SidePanelPage />
    
                case PreMeeting:
                    return <PreMeetingPage onError={msg => this.setState({ error: { status: true, msg } })} />
    
                default:
                    return null;
            }
        }
        
    }
}

export default withTeamsContext(SurfaceSelector);