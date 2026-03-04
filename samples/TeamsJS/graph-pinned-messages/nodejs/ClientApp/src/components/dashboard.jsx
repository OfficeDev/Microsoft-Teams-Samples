// <copyright file="dashboard.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { Component } from "react";
import { Text, Flex, FlexItem, RadioGroup, Button, TrashCanIcon } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
import "../style/style.css";

var accessToken;

// Dashboard where user can manage the pinned message
class Dashboard extends Component {
    constructor(props) {
        super(props);

        this.state = {
            isError: false,
            ssoError: false,
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
                this.setState({ context: context });
            }).then(() => {
                // Fetch id token
                microsoftTeams.authentication.getAuthToken().then((result) => {
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
        this.setState({ ssoError: true });
    }

    // Callback function for successfull authorization
    consentSuccess = async (result) => {
        this.setState({ ssoError: false });
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.authentication.getAuthToken().then((result) => {
                this.ssoLoginSuccess(result);
            })
        });
    }

    // Callback function for failure authorization
    consentFailure(error) {
        console.log("Consent failed: ", error);
    }  

    grantConsent = async() => {
        microsoftTeams.authentication.authenticate({
            url: window.location.origin + "/auth-start",
            width: 600,
            height: 535
        }).then((result) => {
            alert("in success");
            this.consentSuccess(result)
        }).catch((error) => {
            alert("in failure");
            this.consentFailure(error)
        });
    }

    // Exchange client token with server token and fetch the pinned message details.
    exchangeClientTokenForServerToken = async (token) => {
        if (!this.state.context || !this.state.context.chat || !this.state.context.chat.id) {
            console.error("Chat context is not available");
            this.setState({ isError: true });
            return;
        }
        
        var id = this.state.context.chat.id;
        axios.get(`/api/chat/getGraphAccessToken?ssoToken=${token}&chatId=${id}`).then((response) => {
            var responseMessageData = response.data;
            console.log("-----Response--->",responseMessageData);
            this.setState({
                pinnedMessageId: responseMessageData.id,
                pinnedMessage: responseMessageData.message,
                messageList: responseMessageData.messages
            });

        }).catch((error) => {
            this.setState({
                isError: true
            });
            console.log("error is" , error);
        });
    }

    // Method to handle radio button change event.
    handleMessageRadioChange = async (e, props) => {
        console.log(props);
        this.setState({ newMessageId: props.value });
    }

    // Method to unpin the pinned message.
    deletePinnedMessage = async () => {
        var pinnedMessageId = this.state.pinnedMessageId;
        var response = await axios.get(`/api/chat/unpinMessage?ssoToken=${accessToken}&chatId=${this.state.context.chat.id}&pinnedMessageId=${pinnedMessageId}`)
        this.setState({ isError: true });
        console.log("Response---->",response);
    }

    // Api call to pin message into chat.
    pinNewMessage = async () => {
        this.setState({ isError: false });
        var messageId = this.state.newMessageId;
        var response = await axios.get(`/api/chat/pinMessage?ssoToken=${accessToken}&chatId=${this.state.context.chat.id}&messageId=${messageId}`);
        console.log("Response---->",response);
        window.location.reload();
    }

    // Renders radio button for message list.
    renderMessageList = () => {
        var elements = [];
        this.state.messageList.map((message, index) => {
            elements.push({
                key: index,
                label: message.value,
                value: message.id
            });
        });

        return elements;
    }

    render() {
        return (<Flex className="container" column >
            <Flex vAlign="center">
                <Text content="Graph Pinned Message" size="largest" weight="semibold" />
            </Flex>
            {this.state.ssoError ? <><Flex>Invalid grant Error occured. Please click on consent to grant consent.</Flex><Flex><Button hidden={!this.state.ssoError} content="consent" onClick={this.grantConsent} /></Flex></>
                : <><Flex><Text styles={{ marginTop: "1rem" }} weight="semibold" size="large" content="Below message is pinned in chat. Click on the delete icon to unpin the message." /></Flex>
            {!this.state.isError ? <Flex>
                <Flex.Item><Text styles={{ marginTop: "1rem" }} content={`${this.state.pinnedMessage}`} /></Flex.Item>
                <Flex.Item size="size.quarter">
                    <TrashCanIcon styles={{ marginTop: "1rem" }} className="manage-icons" onClick={this.deletePinnedMessage} />
                </Flex.Item>
            </Flex> : <>
                <Flex><Text content="Please pin a message in chat" /></Flex>
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
                        </Flex></>}</>}
            </Flex>)
    }
}

export default Dashboard;