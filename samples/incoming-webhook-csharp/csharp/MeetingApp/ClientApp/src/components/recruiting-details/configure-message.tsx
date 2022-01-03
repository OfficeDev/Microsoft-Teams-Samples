import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Menu, Button, Text, Input, TextArea, Grid, Divider } from '@fluentui/react-northstar'
import { SendIcon } from '@fluentui/react-icons-northstar'
import "./recruiting-details.css"
import { sendCard } from './services/recruiting-detail.service';
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
        alert(url);
        alert(card);
        var str2 = card.replace(/[\r\n\s]+/gm, "");
        const cardDetails: any = {
            webhookUrl: url,
            cardBody: str2,
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
                        })}
                >
                    <Text content="Enter webhook url" />
                    <Input inverted placeholder="Type webhook url" onChange={urlHandler} />
                    <Button icon={<SendIcon />} text content="Send" onClick={cardSubmitHandler} />
                </Grid>
                <Divider />
                <Flex>
                    <Text content="CARD PAYLOAD EDITOR" styles={{ marginLeft: "2rem" }} />
                </Flex>
                <Flex styles={{ marginTop: "1rem" }}>
                    {/*<TextArea fluid placeholder="Paste adaptive card json here" onChange={cardHandler} />*/}
                   
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