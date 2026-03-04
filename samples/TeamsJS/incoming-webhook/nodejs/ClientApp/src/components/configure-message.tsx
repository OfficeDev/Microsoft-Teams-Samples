import React from 'react';
import { app } from "@microsoft/teams-js";
import { Flex, Button, Text, Input } from '@fluentui/react-northstar'
import { SendIcon } from '@fluentui/react-icons-northstar'
import "./configure-message.css"
import { DEFAULT_CARD_PAYLOAD } from "./card-constant"
import { sendCard } from './services/send-card.service';
import Editor from '@monaco-editor/react';

const ConfigureMessage = () => {

    React.useEffect(() => {
        app.initialize();
    }, [])

    const [url, setUrl] = React.useState("");
    const [card, setCard] = React.useState(DEFAULT_CARD_PAYLOAD);

    // Method to set webhook url value.
    const urlHandler = (event: any) => {
        setUrl(event.target.value);
    }

    function handleEditor(value: any) {
        setCard(value);
    }

    const cardSubmitHandler = () => {
        var formattedCardPayload = card.replace(/[\r\n]+/gm, "");
        const cardDetails: any = {
            webhookUrl: url,
            cardBody: formattedCardPayload,
        };

        sendCard(cardDetails);
    }

    return (
        <>
            <div className="config-container">
                <Flex
                    styles={({ theme: { siteVariables } }) => ({
                        backgroundColor: siteVariables.colorScheme.default.background2,
                        padding: '20px',
                    })}
                >
                    <Text className="text-label" content="Enter webhook url" />
                    <Input className="input-box" fluid inverted placeholder="Type webhook url" onChange={urlHandler} />
                    <Button className="send-btn" icon={<SendIcon />} text content="Send" onClick={cardSubmitHandler} />
                </Flex>
                <Flex>
                    <Text className="ed-label" content="Card payload editor" />
                </Flex>
                <Flex className="ed-container">
                    <Editor
                        height="70vh"
                        defaultLanguage="json"
                        defaultValue={card}
                        onChange={handleEditor}
                    />
                </Flex>
            </div>
        </>
    )
}

export default (ConfigureMessage);