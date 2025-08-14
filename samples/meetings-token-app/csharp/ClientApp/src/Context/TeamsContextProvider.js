import React, { Component, useContext } from 'react';

import * as microsoftTeams from "@microsoft/teams-js";
import ContextService from '../Service/Context';
import Constants from '../Constants';

const TeamsContext = React.createContext({});

class TeamsContextProvider extends Component {
    constructor(props) {
        super(props);
        this.getContext = new ContextService(microsoftTeams);
        this.state = {
            teamsContext: {}
        };
    }

    componentDidMount() {
        const { PreMeeting, SidePanel } = Constants.Surfaces;
        this.getContext()
            .then((context = {}) => {
                const frameContext = context.page.frameContext || "";
                if ([PreMeeting, SidePanel].includes(frameContext)) {
                    this.setState({
                        teamsContext: context,
                    })
                    microsoftTeams.app.notifySuccess();
                    return;
                }
                return Promise.reject("Error: Please make sure to run the app within teams as a tab app");
            })
            .catch(msg => {
                microsoftTeams.app.notifyFailure(msg);
                this.setState({
                    error: {
                        status: true,
                        msg,
                    }
                })
            });
    }

    render() {
        return (
            <TeamsContext.Provider value={this.state.teamsContext}>
                {this.props.children}
            </TeamsContext.Provider>
        );
    }
}

export default TeamsContextProvider;

export const withTeamsContext = Component => props => {
    const teamsContext = useContext(TeamsContext);
    return <Component {...props} teamsContext={teamsContext} />
}