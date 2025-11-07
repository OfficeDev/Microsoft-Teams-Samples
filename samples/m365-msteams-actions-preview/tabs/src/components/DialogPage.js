import "./Dialog.css";

import { BearerTokenAuthProvider, TeamsUserCredential, createApiClient } from "@microsoft/teamsfx";
import { Button, Field, Spinner, Text, Textarea } from "@fluentui/react-components";
import { app, dialog } from "@microsoft/teams-js";

import { Login } from "./custom/Login";
import React from "react";
import { ToDoAttachment } from "./custom/Attachments";
import { callFunctionWithErrorHandling } from "./helper/helper";
import config from "./lib/config";
import { loginBtnClick } from "./helper/helper";
import { setValuesToLocalStorage } from "./helper/localstorage";
import { v4 as uuidv4 } from 'uuid';

class DialogPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            showLoginPage: false,
            userInfo: undefined,
            newItemDescription: "",
            loader: false,
            itemId: undefined
        };
    }

    async componentDidMount() {
        await this.initTeamsFx();
        await this.checkIsConsentNeeded();
    }

    async initTeamsFx() {
        const authConfig = {
            clientId: config.clientId,
            initiateLoginEndpoint: config.initiateLoginEndpoint,
        };

        const credential = new TeamsUserCredential(authConfig);
        const userInfo = await credential.getUserInfo();

        this.setState({
            userInfo: userInfo,
        });

        this.scope = config.scopes;
        this.credential = credential;

        const apiBaseUrl = config.apiEndpoint + "/api/";
        // createApiClient(...) creates an Axios instance which uses BearerTokenAuthProvider to inject token to request header
        const apiClient = createApiClient(
            apiBaseUrl,
            new BearerTokenAuthProvider(async () => (await credential.getToken("")).token)
        );
        this.apiClient = apiClient;

        const context = await app.getContext();
        const itemId = context.actionInfo && context.actionInfo.actionObjects[0].itemId;
        this.setState({
            itemId: itemId
        });
    }

    handleInputChange = (description) => {
        this.setState({ newItemDescription: description });
    }

    async checkIsConsentNeeded() {
        try {
            await this.credential.getToken(this.scope);
        } catch (error) {
            this.setState({
                showLoginPage: true,
            });
            return true;
        }
        this.setState({
            showLoginPage: false,
        });
        return false;
    }

    async onAddItem() {
        // For using localstorage
        if (config.localStorage && config.localStorage.toUpperCase() === "TRUE") {
            setValuesToLocalStorage({
                id: uuidv4(),
                objectId: this.state.userInfo.objectId,
                description: this.state.newItemDescription,
                isCompleted: 0,
                itemId: this.state.itemId
            });
        } else {
            // Use client TeamsFx SDK to call "todo" Azure Function in "post" method to insert a new todo item under user oid
            await callFunctionWithErrorHandling(this.apiClient, "todo", "post", {
                description: this.state.newItemDescription,
                isCompleted: false,
                channelOrChatId: "",
                itemId: this.state.itemId
            });
        }
    }

    onTrigger = async () => {
        this.setState({ loader: true });
        await this.onAddItem();
        dialog.url.submit();
    }

    render() {
        return (
            <>{this.state.showLoginPage === false && (
                <div>
                    {this.state.loader && <Spinner size="tiny" />}
                    {!this.state.loader && (
                        <div className="dialog-container">
                            <div className="dialog-header">
                                <Text className="dialog-title" as="h2">
                                    Add Task
                                </Text>
                            </div>
                            <Field size="large" style={{ marginTop: "20px" }} label="Attachment: ">
                                {this.credential && this.state.itemId && this.state.userInfo && (
                                    <ToDoAttachment
                                        teamsUserCredential={this.credential}
                                        scope={this.scope}
                                        action={true}
                                        itemId={this.state.itemId}
                                        userId={this.state.userInfo.objectId}
                                    />
                                )
                                }
                            </Field>
                            <Field size="large" label="Notes" style={{ marginTop: "10px" }}>
                                <Textarea style={{ height: "100px" }} placeholder="Add your notes here" value={this.state.newItemDescription} onChange={(event) => { this.handleInputChange(event.target.value) }} />
                            </Field>
                            <div className="dialog-button">
                                <Button type="button" appearance="primary" onClick={this.onTrigger}>Submit</Button>
                            </div>
                        </div>
                    )}
                </div>
            )}
                {this.state.showLoginPage === true && (
                    <Login userInfo={this.state.userInfo} loginBtnClick={loginBtnClick(this.credential, this.scope)} />
                )}
            </>
        );
    }
}
export default DialogPage;