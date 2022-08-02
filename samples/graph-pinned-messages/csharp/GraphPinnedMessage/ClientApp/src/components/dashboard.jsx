// <copyright file="configure.tsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import { Text, Flex, FlexItem, RadioGroup, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
import "../style/style.css";


var accessToken;

// Dashboard where user can manage the pinned message
class Dashboard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            context: undefined,
            messageList: [],
            newMessageId: "",
            pinnedMessage: "",
            pinnedMessageId: ""
        }
    }

    componentDidMount() {
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.app.getContext().then((context) => {
                console.log(context.chat.id);
                this.setState({ context: context });
            }).then(() => {
                // Fetch id token
                microsoftTeams.authentication.getAuthToken().then((result) => {
                    console.log(this.state.context);
                    accessToken = result;
                    this.ssoLoginSuccess(result);
                }).catch((error) => {
                    this.ssoLoginFailure(error)
                });
            })
        });
    }

    // Success callback for getAuthtoken method
    ssoLoginSuccess = async (result) => {
        accessToken = result;
        this.exchangeClientTokenForServerToken(result);
    }

    // Failure callback for getAuthtoken method
    ssoLoginFailure(error) {
        alert("SSO failed: ", error);
    }

    // Exchange client token with server token and fetch the pinned message details.
    exchangeClientTokenForServerToken = async (token) => {
        console.log(this.state.context);
        var response = await axios.get(`/api/chat/getGraphAccessToken?ssoToken=${token}&chatId=${this.state.context.chat.id}`);
        console.log(response);
        var responseMessageData = JSON.parse(response.data);
        this.setState({
            pinnedMessageId: responseMessageData.Id,
            pinnedMessage: responseMessageData.Message,
            messageList: responseMessageData.Messages
        });
    }


    // Method to handle radio button change event.
    handleMessageRadioChange = async (e, props) => {
        console.log(props);
        this.setState({ newMessageId: props.value });
    }

    // Api call to pin message into chat.
    pinNewMessage = async () => {
        var messageId = this.state.newMessageId;
        var response = await axios.get(`/api/chat/pinMessage?ssoToken=${accessToken}&chatId=${this.state.context.chat.id}&messageId=${messageId}`);
        console.log(response);
        window.location.reload();
    }

    // Renders radio button for message list.
    renderMessageList = () => {
        var elements = [];
        this.state.messageList.map((message, index) => {
            elements.push({
                key: index,
                label: message.Message,
                value: message.Id
            });
        });

        return elements;
    }

    render() {
        return (<Flex className="container" column >
            <Flex vAlign="center"   >
                <Text content="Graph Pinned Message" size="largest" weight="semibold" />
            </Flex>
            <Flex><Text styles={{ marginTop: "1rem" }} weight="semibold" size="large" content="Below message is pinned in chat. Click on the delete icon to unpin the message." /></Flex>
            <Flex>
                <Text styles={{ marginTop: "1rem" }} content={`${this.state.pinnedMessage}`} />
            </Flex>
            <Flex><Text styles={{ marginTop: "1rem" }} weight="semibold" size="large" content="You can also pin message from below message list. Select any message and click on Pin new message button." /></Flex>
            <RadioGroup
                className="container-medium"
                styles={{ paddingLeft: "0.5rem", marginTop: "1rem" }}
                onCheckedValueChange={this.handleMessageRadioChange}
                items={this.renderMessageList()}
            />
            <Flex>
                <FlexItem push>
                    <Button primary content="Pin new message" onClick={this.pinNewMessage} />
                </FlexItem>
            </Flex>
        </Flex>)
    }
}

export default Dashboard;