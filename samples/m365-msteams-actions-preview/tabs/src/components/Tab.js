// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import "./App.css";
import "./Tab.css";

import {
  BearerTokenAuthProvider,
  TeamsUserCredential,
  createApiClient,
} from "@microsoft/teamsfx";
import { Checkbox, Input, Spinner } from "@fluentui/react-components";
import { callFunctionWithErrorHandling, loginBtnClick } from "./helper/helper";
import { getItemsFromLocalStorage, onDeleteItem, setValuesToLocalStorage, updateItem } from "./helper/localstorage";

import { DataGrid } from "./custom/DataGrid";
import FilteteredResult from './custom/FilteredResult';
import { Login } from "./custom/Login";
import React from "react";
import { TodoDialog } from "./custom/Dialog";
import { app } from "@microsoft/teams-js";
import config from "./lib/config";
import noItemimage from "../images/no-item.png";
import { v4 as uuidv4 } from 'uuid';

class Tab extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      userInfo: {},
      dbItems: [],
      actionObjectsType: undefined,
      newItemDescription: "",
      showLoginPage: undefined,
      isAddingItem: false,
      initialized: false,
      attachmentId: "",
      editId: undefined,
      loader: false,
      showFilter: true,
      itemId: undefined
    };
    this.handleDialogInputChange = this.handleDialogInputChange.bind(this);
    this.handleFormSubmit = this.handleFormSubmit.bind(this);
    this.onAddItem = this.onAddItem.bind(this);
    this.onDeleteItem = this.onDeleteItem.bind(this);
    this.onCompletionStatusChange = this.onCompletionStatusChange.bind(this);
    this.onUpdateItem = this.onUpdateItem.bind(this);
    this.handleInputChange = this.handleInputChange.bind(this);
    this.onEdit = this.onEdit.bind(this);
    this.loginBtn = this.loginBtn.bind(this);
    this.clearFilter = this.clearFilter.bind(this);
  }

  async componentDidMount() {
    await this.initTeamsFx();
    await this.initData();
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
      actionObjectsType: context.actionInfo && context.actionInfo.actionObjects[0].type,
      itemId: itemId
    });
  }

  async initData() {
    if (!(await this.checkIsConsentNeeded())) {
      this.setState({ loader: true });
      await this.getItems(this.state.itemId);
      this.setState({ loader: false });
    }
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

  async getItems(itemId) {
    let result = undefined;

    // For using localstorage
    if (config.localStorage && config.localStorage.toUpperCase() === "TRUE") {
      result = getItemsFromLocalStorage(itemId);
    } else {
      // Use client TeamsFx SDK to call "todo" Azure Function in "get" method to get all todo list which belong to user oid
      result = await callFunctionWithErrorHandling(this.apiClient, "todo", "get", undefined,
        { itemId: itemId, objectId: this.state.userInfo.objectId });
    }

    if (result === "Error") {
      throw new Error("todo Function failed, please check Azure Functions log for details!");
    } else {
      this.setState({
        dbItems: result,
        initialized: true,
      });
    }
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
    await this.refresh();
    this.setState({
      newItemDescription: "",
      attachmentId: ""
    });
  }

  async onUpdateItem(id, description) {
    // For using localstorage
    if (config.localStorage && config.localStorage.toUpperCase() === "TRUE") {
      updateItem(id, description, undefined);
    } else {
      // Use client TeamsFx SDK to call "todo" Azure Function in "put" method to update a todo item
      await callFunctionWithErrorHandling(this.apiClient, "todo", "put", { id, description });
    }
  }

  async onEdit(id) {
    this.setState({ editId: id })
  }

  async onDeleteItem(id) {
    this.setState({ loader: true });

    // For using localstorage
    if (config.localStorage && config.localStorage.toUpperCase() === "TRUE") {
      onDeleteItem(id);
    } else {
      // Use client TeamsFx SDK to call "todo" Azure Function in "delete" method to delete a todo item
      await callFunctionWithErrorHandling(this.apiClient, "todo", "delete", { id });
    }
    await this.refresh();
    this.setState({ loader: false });
  }

  async onCompletionStatusChange(id, index, isCompleted) {
    this.setState({ loader: true });
    this.handleInputChange(index, "isCompleted", isCompleted);

    // For using localstorage
    if (config.localStorage && config.localStorage.toUpperCase() === "TRUE") {
      updateItem(id, undefined, isCompleted);

    } else {
      // Use client TeamsFx SDK to call "todo" Azure Function in "put" method to update a todo item to completed
      await callFunctionWithErrorHandling(this.apiClient, "todo", "put", {
        id,
        isCompleted
      });
    }

    await this.refresh();
    this.setState({
      loader: false
    });
  }

  handleInputChange(index, property, value) {
    const newItems = JSON.parse(JSON.stringify(this.state.dbItems));
    newItems[index][property] = value;
    this.setState({
      dbItems: newItems
    });
  }

  async refresh() {
    await this.getItems(this.state.itemId);
  }

  async handleFormSubmit() {
    await this.onAddItem();
  }

  handleDialogInputChange(description) {
    this.setState({ newItemDescription: description });
  }

  loginBtn() {
    loginBtnClick(this.credential, this.scope);
  }

  async clearFilter() {
    this.setState({
      itemId: undefined,
      loader: true
    });
    await this.getItems(undefined);
    this.setState({
      showFilter: false,
      loader: false
    });
  }

  render() {
    return (
      <div>
        {this.state.showLoginPage === false && (
          <div className="flex-container">
            {this.state.showFilter &&
              <FilteteredResult
                teamsUserCredential={this.credential}
                scope={this.scope}
                actionObjectsType={this.state.actionObjectsType}
                clearFilter={this.clearFilter}
                count={this.state.dbItems.length}
                itemId={this.state.itemId}
                objectId={this.state.userInfo.objectId}
              />
            }
            <div className="todo-col">
              <div className="todo">
                <div className="header">
                  <div className="title">
                    <h2>To Do List</h2>
                  </div>
                  <div className="add-button">
                    {this.credential && this.state.itemId && this.state.userInfo && (
                      <TodoDialog
                        handleFormSubmit={this.handleFormSubmit}
                        handleDialogInputChange={this.handleDialogInputChange}
                        description={this.state.newItemDescription}
                        teamsUserCredential={this.credential}
                        scope={this.scope}
                        userInfo={this.state.userInfo}
                        itemId={this.state.itemId}
                      />)}
                  </div>
                </div>

                {this.state.dbItems.length > 0 && (
                  <DataGrid
                    dbItems={this.state.dbItems}
                    teamsUserCredential={this.credential}
                    scope={this.scope}
                    onDeleteItem={this.onDeleteItem}
                    onAddItem={this.onAddItem}
                    onUpdateItem={this.onUpdateItem}
                    onCompletionStatusChange={this.onCompletionStatusChange}
                    handleInputChange={this.handleInputChange}
                    onEdit={this.onEdit}
                    editId={this.state.editId}
                    userInfo={this.state.userInfo}
                  />
                )}

                {this.state.isAddingItem && (
                  <div className="item add">
                    <div className="complete">
                      <Checkbox disabled className="is-completed-input" />
                    </div>
                    <div className="description">
                      <Input autoFocus type="text" value={this.state.newItemDescription} onChange={(e) =>
                        this.setState({ newItemDescription: e.target.value })
                      }
                        onKeyDown={(e) => {
                          if (e.key === "Enter") {
                            this.onAddItem();
                          }
                        }}
                        onBlur={() => {
                          if (this.state.newItemDescription) {
                            this.onAddItem();
                          }
                          this.setState({
                            isAddingItem: false,
                          });
                        }}
                        className="text"
                      />
                    </div>
                  </div>
                )}

                {this.state.initialized &&
                  !this.state.dbItems.length &&
                  !this.state.isAddingItem && (
                    <div className="no-item">
                      <div>
                        <img src={noItemimage} alt="no item" />
                      </div>
                      <div>
                        <h2>No tasks</h2>
                        <p>Add more tasks to make you day productive.</p>
                      </div>
                    </div>
                  )}
              </div>
            </div>
            {this.state.loader && <Spinner tabIndex={5} className="spinner" />}
          </div>

        )}

        {this.state.showLoginPage === true && (
          <Login userInfo={this.state.userInfo} loginBtnClick={this.loginBtn} />
        )}
      </div>
    );
  }
}
export default Tab;
