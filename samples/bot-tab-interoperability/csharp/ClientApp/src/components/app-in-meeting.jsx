import React, { useEffect, useState } from 'react';
import { app } from "@microsoft/teams-js";

function App() {
    const [messages, setMessages] = useState([]);
    const [inputValue, setInputValue] = useState('');
    const [ws, setWs] = useState(null);

    useEffect(() => {
        const webSocket = new WebSocket('ws://localhost:56806/ws');

        webSocket.onopen = () => {
            console.log('WebSocket connected');
        };

        webSocket.onmessage = (event) => {
            setMessages((prev) => [...prev, event.data]);
        };

        webSocket.onclose = () => {
            console.log('WebSocket disconnected');
        };

        setWs(webSocket);

        return () => {
            webSocket.close();
        };
    }, []);

    async function sendMessage() {
        if (ws) {
            const context = await app.getContext();
            const userAadId = context.user?.id;
            const threadId =
                context.chat?.id
                ?? context.channel?.id
                // Personal apps do not return chat.id, must construct manually
                ?? `19:${userAadId}_${process.env.REACT_APP_MICROSOFT_APP_ID}@unq.gbl.spaces`;

            const message = {
                threadId: threadId,
                inputValue: inputValue,
                appID: process.env.REACT_APP_MICROSOFT_APP_ID,
                appPassword: process.env.REACT_APP_MICROSOFT_PASSWORD
            };

            ws.send(JSON.stringify(message));
            setInputValue('');
        }
    };

    return (
        <>
            <div className="container">
                <h2>Unified Communication Between Bot and Tab</h2>
                <div className="row">

                    {/*<div>*/}
                    {/*    {messages.map((msg, index) => (*/}
                    {/*        <p key={index}>{msg}</p>*/}
                    {/*    ))}*/}
                    {/*</div>*/}

                    <div>
                        <h3>
                            Send Color To Bot
                        </h3>
                        <input type="text" value={inputValue} id="msgToBot" onChange={(e) => setInputValue(e.target.value)} placeholder="Write Message To Bot" required></input>
                        <button id="sendMessageButton" onClick={sendMessage}>Send Message</button>
                    </div>

                    <div className="row">
                        <div>
                            <h3>Selected Color From Bot : </h3>
                            <div>
                                <span className="circle_red"></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default App;