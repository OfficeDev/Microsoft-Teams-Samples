import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Button, Text, Input, Grid, Divider } from '@fluentui/react-northstar'
import { SendIcon } from '@fluentui/react-icons-northstar'
import { sendCard } from './services/send-card.service';
import Editor from '@monaco-editor/react';

const ConfigureMessage = () => {
    React.useEffect(() => {
        microsoftTeams.initialize();
    }, [])

    const [url, setUrl] = React.useState("");
    const [card, setCard] = React.useState("");

    const urlHandler = (event: any) => {
        setUrl(event.target.value);
    }

    const cardHandler = (event: any) => {
        setCard(event.target.value);
    }

    function handleEditor(value: any) {
        setCard(value);
    }

    const cardSubmitHandler = () => {
        var cardBody = card.replace(/[\r\n]+/gm, "");
        const cardDetails: any = {
            webhookUrl: url,
            cardBody: cardBody,
        };

        sendCard(cardDetails);
    }

    return (
        <>
            <div className="edcontainer">
                <Grid
                    styles={({ theme: { siteVariables } }) => ({
                        backgroundColor: siteVariables.colorScheme.default.background2,
                        padding: '20px',
                    })}>
                    <Text content="Enter webhook url" />
                    <Input inverted fluid placeholder="Type webhook url" onChange={urlHandler} />
                    <Button icon={<SendIcon />} text content="Send" onClick={cardSubmitHandler} />
                </Grid>
                <Divider />
                <Flex>
                    <Text content="CARD PAYLOAD EDITOR" styles={{ marginLeft: "2rem" }} />
                </Flex>
                <Flex styles={{ marginTop: "1rem" }}>
                    <Editor
                        height="70vh"
                        defaultLanguage="json"
                        defaultValue="Paste card json"
                        onChange={handleEditor}
                    />
                </Flex>
            </div>
        </>
    )
}

export default (ConfigureMessage);