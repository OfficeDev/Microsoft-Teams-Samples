import React, { useState } from "react";
import { Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import "../style/style.css";

const ToggleAudioCall = () => {
    let app = microsoftTeams.app;
    app.initialize();

    const [appTheme, setAppTheme] = useState("");
    const [result, setResult] = useState(false);

    const callback = (errcode, bln) => {
        if (errcode) {
            setResult(JSON.stringify(errcode))
        }
        else {
            setResult(JSON.stringify(bln))
        }
    }

    /// <summary>
    /// This method getIncomingClientAudioState returns the current state of client audio.
    /// The incoming audio is muted if the result is true and unmuted if the result is false.
    /// </summary>
    const ClientAudioState = () => {
        microsoftTeams.meeting.getIncomingClientAudioState(callback);
    }

    /// <summary>
    /// This method toggleIncomingClientAudio which toggles mute/unmute to client audio.
    /// Setting for the meeting user from mute to unmute or vice-versa.
    /// </summary>
    const toggleState = () => {
        console.log("callback---->", callback);
        microsoftTeams.meeting.toggleIncomingClientAudio(callback);
    }

    React.useEffect(() => {
        app.initialize().then(app.getContext).then((context) => {
            app.notifySuccess();

            // Applying default theme from app context property
            let defaultTheme = context.app.theme;

            switch (defaultTheme) {
                case 'dark':
                    setAppTheme("theme-dark");
                    break;
                default:
                    setAppTheme('theme-light');
            }

            // Handle app theme when 'Teams' theme changes
            microsoftTeams.app.registerOnThemeChangeHandler(function (theme) {
                switch (theme) {
                    case 'dark':
                        setAppTheme('theme-dark');
                        break;
                    case 'default':
                        setAppTheme('theme-light');
                        break;
                    case 'contrast':
                        setAppTheme('theme-contrast');
                        break;
                    default:
                        return setAppTheme('theme-light');
                }
            });
        }).catch(function (error) {
            console.log(error, "Could not register handlers.");
        });

    }, [])

    return (
        <div className={appTheme}>
            <h3>Mute/Unmute Audio Call </h3>
            <Button primary content="Mute/Un-Mute" onClick={toggleState} />
            <li className="break"> Mute State : <b>{result}</b></li>
        </div>
    );
}

export default ToggleAudioCall