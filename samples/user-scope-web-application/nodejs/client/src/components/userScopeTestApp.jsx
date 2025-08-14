// <copyright file="userScopeTestApp.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from 'react';
import { Avatar, Chat, Divider, Segment, Grid, List, Image, PersonIcon, Text, Flex, MentionIcon, MenuItemIcon } from '@fluentui/react-northstar';
import axios from "axios";
import moment from 'moment'
import { Provider, Button } from '@fluentui/react-northstar';
import { useNavigate, useLocation } from "react-router-dom";
import * as msal from '@azure/msal-browser';
import parse from 'html-react-parser';

const UserScopeTestApp = () => {
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  let token = decodeURIComponent(searchParams.get('token'));
  const navigate = useNavigate();
  const bindLeftRailItems = [];
  const [allMessages, setAllMessages] = useState([]);
  const [currentUser, setCurrentUser] = useState("");
  let UserId = "";
  const [subsStatus, setSubsStatus] = useState("Please wait subscribing...");
  const [showLastReadDivider, setshowLastReadDivider] = useState("");
  const [currentDateTime, setCurrentDateTime] = useState(new Date());
  let msalInstance = undefined;
  const [groupList, setGroupList] = useState([]);
  const [notificationList, setNotificationList] = useState([]);
  let updatedReadList = [];

  const msalConfig = {
    auth: {
      clientId: "<<Microsoft-App-Id>>",
      authority: "https://login.microsoftonline.com/common",
      supportsNestedAppAuth: true // Enable native bridging.
    }
  };

  useEffect(() => {
    async function msalInstanceAPI() {
      msalInstance = await msal.PublicClientNext.createPublicClientApplication(msalConfig);
    }
    msalInstanceAPI()
  }, [])

  // Call the Microsoft Graph API with the access token.
  useEffect(() => {
    async function fetchUserDetails() {
      const response = await fetch(
        `https://graph.microsoft.com/v1.0/me`,
        {
          headers: { Authorization: token },
        }
      );

      if (response.ok) {
        // Write file names to the console. 
        const data = await response.json();
        const results = JSON.stringify(data, null, 2);
        const objFromJson = JSON.parse(results);

        // fetch group chat list (left rail items)
        fetchTeamsLeftRail(objFromJson.id, token);

        // subscribe to change notifications
        subscribeToUserLevelChatsWithNotifiySpecificProperty(objFromJson.id);
        setCurrentUser(objFromJson.displayName);
        UserId = objFromJson.id;

      } else {
        const errorText = await response.text();
        // Send Logout when token expired
        if (errorText) {
          navigate('/');
        }

        console.error("Microsoft Graph call failed - error text: " + errorText);
      }
    }

    fetchUserDetails();
  }, [])

  // Fetch teams group chat list
  const fetchTeamsLeftRail = async (userID, token) => {
    var resp = await axios.get(`/api/changeNotification/getAllChats?userId=${userID}&token=${token}`);
    if (resp.data.length > 0) {
      setGroupList(resp.data);
    }
  }

  // Get Change notification and reflect to UI
  useEffect(() => {
    const interval = setInterval(() => {
      getNotificationsData();
      fetchTeamsLeftRail(UserId, token);
    }, 5000)

    return () => clearInterval(interval); // Clean up the interval 

  }, []);

  // Return notifications URL
  const getNotificationsUrl = () => {
    return axios.post('/api/notifications');
  };

  //  Fetch notification data
  const getNotificationsData = async () => {
    const response = await (getNotificationsUrl());
    addOrRemoveItem(response.data[0]);
  }

  // Function to add or remove an item based on id existence
  const addOrRemoveItem = (newItem) => {
    if (newItem) {
      // Check if newItem's id already exists in notificationList
      const itemIndex = notificationList.findIndex(item => item.id === newItem.id);

      if (itemIndex === -1) {
        // If id does not exist, add newItem to notificationList
        setNotificationList(prevList => [...prevList, newItem]);

      } else {
        // If id exists, remove item from notificationList
        const updatedList = [...notificationList];
        updatedList.splice(itemIndex, 1);
        setNotificationList(updatedList);
      }
    }
  };

  // Function to set group chat As-Read
  const setAsReadById = (itemId) => {
    updatedReadList = notificationList.filter(item => item.id !== itemId);
    setNotificationList(updatedReadList);
  };

  // Bind left rail items
  const renderList = () => {
    groupList.forEach(async (item, index) => {

      // Check if item.id exists in notificationList
      const isInNotificationList = notificationList.some((listItem) => listItem.id === item.id);

      if (item.topic != null) {
        // Construct each element based on the condition
        bindLeftRailItems.push({
          key: item.id,
          media: <Avatar icon={<PersonIcon />} />, // Replace with appropriate Avatar setup
          header: (
            <Text weight={isInNotificationList ? 'bold' : 'regular'} content={item.topic} />
          ),
          headerMedia: moment(item.createdDateTime).fromNow(),
          content: moment(item.lastModifiedDateTime).fromNow()
        });
      }
    });
  };

  // Get the last message of chat using chat Id
  const getLastMessageOfChat = async (chatId) => {
    var response = await axios.get(`/api/changeNotification/getAllMessages?chatId=${chatId}&token=${token}`);
    let msgList = [];
    msgList.push(response.data);
  }

  // Subscribe to  Change Notifications
  const subscribeToUserLevelChatsWithNotifiySpecificProperty = async (userId) => {
    var subsResp = await axios.post(`/api/changeNotification/UserLevelChatsUsingNotifyOnUser?userId=${userId}&token=${token}`);
    try {
      if (subsResp.data) {
        setSubsStatus(subsResp.data);
      }
    }
    catch (error) {
      setSubsStatus("Error : ", error);
    }
  }

  // Get the active account to sign out
  const handleLogout = async () => {
    msalInstance = await msal.PublicClientNext.createPublicClientApplication(msalConfig);
    const logoutRequest = {
      account: msalInstance.getActiveAccount()
    };

    await msalInstance.logoutPopup(logoutRequest);
    navigate('/');
  };

  return (
    <Provider>
      <div>
        {renderList()}
      </div>

      <Flex
        gap="gap.small"
        padding="padding.medium"
        styles={{
          background: '#5b5fc7', // Adjust the background color as needed
          color: 'white',
          alignItems: 'center',
        }}
      >
        <Text size="large" content="User Scope Test Application" weight="bold" />
        <Flex.Item push>
          <Flex gap="gap.small">
            {/* Add additional navigation items as needed */}
            <Button text content="Logout" weight="bold" style={{ color: 'white' }} onClick={handleLogout} />
          </Flex>
        </Flex.Item>
        <Flex gap="gap.small">
          {/* Add profile icon and other icons as needed */}
          <Button icon={<Avatar name={currentUser} />} iconOnly text />
        </Flex>
      </Flex>
      <Grid columns="repeat(3, 1fr)" rows="60px 150px 50px">
        <Segment
          color='brand'
          content={(
            <>
              Logged in as <span style={{ color: '#F7DC6F' }}><b>{currentUser}</b></span>
              <br />
              Subscription Status  <span style={{ color: '#F7DC6F' }}><b>{subsStatus}</b></span>
            </>
          )}
          inverted
          styles={{
            gridColumn: 'span 4'
          }}
        />

        <Segment
          content={
            <List navigable items={bindLeftRailItems}
              selectable
              onSelectedIndexChange={async (e, newProps) => {
                let messageList = [];
                let elements = [];
                let chatId = newProps.items[newProps.selectedIndex].key;
                // set group chat As-Read
                setAsReadById(chatId);
                var response = await axios.get(`/api/changeNotification/getAllMessages?chatId=${chatId}&token=${token}`);
                messageList = [];
                messageList.push(response.data.reverse());
                var isLastReadLine;
                messageList[0].forEach((item, index) => {
                  if (isLastReadLine == undefined) {
                    const isLastReadLine_1 = notificationList.some((listItem) => listItem.viewpointLastMessageReadDT < item.createdDateTime && listItem.id === item.chatId);
                    if (isLastReadLine_1 == true) {
                      isLastReadLine = true;
                      elements.push(
                        {
                          children: isLastReadLine ? <Divider content="Last read" color="brand" important /> : null,
                          key: item.id
                        }
                      )
                    }
                  }
                  if (item.from != null && item.from.user != null) {
                    if (item.from.user.displayName != currentUser) {
                      elements.push({
                        gutter: <Avatar icon={<PersonIcon />} />,
                        contentPosition: 'start',
                        message: <Chat.Message content={parse(item.body.content)} author={item.from.user.displayName} timestamp={moment(item.lastModifiedDateTime).fromNow()}
                          variables={{
                            hasMention: item.mentions.length > 0
                          }}
                        />,
                        key: item.id
                      },
                      )
                      if (isLastReadLine === true) {
                        isLastReadLine = false;
                      }
                    }
                    if (item.from.user.displayName == currentUser) {
                      elements.push({
                        contentPosition: 'end',
                        message: <Chat.Message content={parse(item.body.content)} author={item.from.user.displayName} timestamp={moment(item.lastModifiedDateTime).fromNow()}
                          variables={{
                            hasMention: item.mentions.length > 0
                          }} />,
                        key: item.id,
                      })
                    }
                  }
                });

                setAllMessages([]);
                setAllMessages(elements);
              }}
            />
          }
          inverted
          styles={{
            gridColumn: 'span 1',
            backgroundColor: 'white',
            color: 'black',
            height: 'fit-content',
            overflowY: 'scroll',
            maxHeight: '95vh',
            border: '0px'
          }}
        ></Segment>

        <Segment
          content={<Chat items={allMessages} />}
          styles={{
            gridColumn: 'span 3',
            height: 'fit-content',
            overflowY: 'scroll',
            maxHeight: '95vh',
            border: '0px'
          }}
        />
      </Grid >
    </Provider >

  )
}

export default UserScopeTestApp