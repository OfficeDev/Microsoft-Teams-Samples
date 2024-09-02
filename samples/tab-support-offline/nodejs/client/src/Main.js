import React, { useEffect, useState, createContext } from 'react';
import App from './App';
import { inTeams } from './utilities/TeamsUtils';
import * as msteams from '@microsoft/teams-js';


export const TeamsContext = createContext({ 'context': null, 'token': null });
TeamsContext.displayName = "TeamsContext";

function Main() {
    const [teamsContext, setTeamsContext] = useState(null);
    const [teamsAuthToken, setTeamsAuthToken] = useState(null);
    const [isLoading, setIsLoading] = useState(true);    


    useEffect(() => {
        async function getTeamsContext() {
            setIsLoading(true);
            let isInsideTeams = await inTeams();
            if (isInsideTeams) {
                let ctx = await msteams.app.getContext();
                setTeamsContext(ctx);
            } else {
                if(window.location.href.includes("?purpose=test")) {
                    setTeamsContext({"userId": "testUser"});
                } else {
                    setTeamsContext(null);
                }
            }
            setIsLoading(false);
        }
        getTeamsContext();

    }, []);

    useEffect(() => {
        async function performAuth() {
            setIsLoading(true);
            if (teamsContext == null) {
                return;
            }

            if (teamsContext.userId === "testUser") {
                setTeamsAuthToken(process.env.REACT_APP_TOKEN);
                setIsLoading(false);
                return;
            }
            let token = await msteams.authentication.getAuthToken();
            setTeamsAuthToken(token);
            setIsLoading(false);
        }
        performAuth();

    }, [teamsContext]);

    const WebPage = () => <h1>Not running in MicrosoftTeams</h1>
    
    return (
        <div>
            {
                isLoading ? <h1>Loading...</h1> :
                teamsContext ? <TeamsAppWrapper context={teamsContext} token={teamsAuthToken} /> : WebPage()
            }
        </div>
    );
}

function TeamsAppWrapper(props) {
    return (
        <TeamsContext.Provider value={{ 'context': props.context, 'token': props.token }}>
            <App />
        </TeamsContext.Provider>
    );
}

export default Main;
