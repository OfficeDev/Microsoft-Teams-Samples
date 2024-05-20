/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { useEffect, useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import io from "socket.io-client";

const PersonalTab = (props) => {
    const [serverMessage, setServerMessage] = useState("");
    const [Color, setColor] = useState("");
    const [notification, setNotification] = useState("");

    // Declare new state variables that are required to get and set the connection.
    const [socket, setSocket] = useState(io());
    // Builds the socket connection, mapping it to /io
    useEffect(() => {
        microsoftTeams.app.initialize();
        setColor('circle_gray');
        setSocket(io());
    }, []);

    // subscribe to the socket event
    useEffect(() => {
        microsoftTeams.app.initialize();
        if (!socket) return;
        socket.on('connection', () => {
            socket.connect();
        });

        // receive a message from the server
        socket.on("message", data => {
            if (data) {
                let color = data[0].toLowerCase();
                setServerMessage(data[0]);

                switch (color) {
                    case 'red':
                        setColor('circle_red');
                        break;
                    case 'blue':
                        setColor('circle_blue');
                        break;
                    case 'green':
                        setColor('circle_green');
                        break;
                    default:
                        return setColor('theme-black');
                }
            }
        });

    }, [socket]);

    // Send Message To Server
    const sendMessage = () => {
        // Get Textbox value
        let msgToBot = document.getElementById("msgToBot").value;
        if (msgToBot != '') {
            setNotification("");
            // send a message to the client
            const socket = io();
            if (socket) {
                socket.emit('message', msgToBot);
                setNotification("Successfully Message Send");
            }
        }
        else {
            setNotification("Please Enter Some text");
        }
    }

    // Clear notification message 
    const onTextChange = () => {
        setNotification("");
    }

    return (
        <>
            <div className="container">
                <h2>Unified Communication Between Bot and Tab</h2>
                <div className="row">
                    <div>
                        <h3>
                            Send Color To Bot
                        </h3>
                        <input type="text" id="msgToBot" onChange={onTextChange} placeholder="Write Message To Bot" required></input>
                        <button id="sendMessageButton" onClick={sendMessage}>Send Message</button>
                        <p>{notification}</p>
                    </div>

                    <div className="row">
                        <div>
                            <h3>Selected Color From Bot : <b>{serverMessage}</b></h3>
                            <div>
                                <span className={Color}></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default PersonalTab;