import React, { Component } from 'react';

import ErrorMessageBar from '../Components/ErrorMessageBar';
import SidePanelPage from '../Pages/SidePanel';
import PreMeetingPage from '../Pages/PreMeeting';
import { withTeamsContext } from '../Context/TeamsContextProvider';
import Constants from '../Constants';
import * as microsoftTeams from "@microsoft/teams-js";

class SurfaceSelector extends Component {
    constructor(props) {
        super(props);
        this.state = {
            error: {
                status: false,
                msg: "",
            },
            context: {}
        };
    }

    componentDidMount() {
        microsoftTeams.app.getContext().then((context) => {
            this.setState({ context });
        });
    }

    render() {
        if (this.state.error.status) {
            return <ErrorMessageBar msg={this.state.error.msg} show={this.state.error.status} />
        }

        const { SidePanel, PreMeeting } = Constants.Surfaces;
        
        if (this.state.context?.page) {
            const frameContext = this.state.context.page.frameContext;
            switch (frameContext) {
                case SidePanel:
                    return <SidePanelPage />

                case PreMeeting:
                    return <PreMeetingPage onError={msg => this.setState({ error: { status: true, msg } })} />

                default:
                    return <div>Please open this application in Meeting Context.</div>
            }
        }
        return <div></div>
    }
}

export default withTeamsContext(SurfaceSelector);