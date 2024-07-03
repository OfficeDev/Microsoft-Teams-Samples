// <copyright file="userScopeTestApp.jsx" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

import React, { useEffect, useState } from 'react';
import { Avatar, Chat, Divider, Segment, Grid, List, Image, PersonIcon, Text, Flex } from '@fluentui/react-northstar';
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
  const [subsStatus, setSubsStatus] = useState("Please wait subscribing...");
  const [showLastReadDivider, setshowLastReadDivider] = useState("");
  const [currentDateTime, setCurrentDateTime] = useState(new Date());
  let msalInstance = undefined;
  const [groupList, setGroupList] = useState([]);
  const [notificationList, setNotificationList] = useState([]);

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

        //  Get Chats from graph api by passing CurrentUser
        var resp = await axios.get(`/api/changeNotification/getAllChats?userId=${objFromJson.id}&token=${token}`);
        if (resp.data.length > 0) {
          setGroupList(resp.data);
        }

        // subscribe to change notifications
        subscribeToUserLevelChatsWithNotifiySpecificProperty(objFromJson.id);
        setCurrentUser(objFromJson.displayName);

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

  // Get Change notification when state changes
  useEffect(() => {
    const getNotificationsUrl = () => {
      return axios.post('/api/notifications');
    };

    const getNotificationsData = async () => {
      const response = await (getNotificationsUrl());
      setNotificationList([]);
      setNotificationList(response.data);
    }

    getNotificationsData();
  }, [notificationList]);

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
                var response = await axios.get(`/api/changeNotification/getAllMessages?chatId=${chatId}&token=${token}`);
                messageList = [];
                messageList.push(response.data);
                messageList[0].forEach((item, index) => {
                  const isInNotificationList = notificationList.some((listItem) => listItem.viewpointLastMessageReadDT < currentDateTime);
                  if (isInNotificationList) {
                    elements.push({
                      children: <Divider content="Last read" color="brand" important />,
                      key: item.id, // Unique key for the last read divider
                    });
                  }

                  if (item.messageType === "message" && item.from.user.displayName == currentUser) {
                    elements.push({
                      gutter: <Avatar icon={<PersonIcon />} />,
                      contentPosition: 'start',
                      message: <Chat.Message content={parse(item.body.content)} author={item.from.user.displayName} timestamp={moment(item.lastModifiedDateTime).fromNow()} />,
                      key: item.id
                    },
                      {
                        children: <Divider content={moment(item.lastModifiedDateTime).format('dddd')} />,
                        key: item.id, // Unique key for the last read divider
                      })
                  }
                  if (item.messageType === "message" && item.from.user.displayName != currentUser) {
                    elements.push({
                      contentPosition: 'end',
                      message: <Chat.Message content={parse(item.body.content)} author={item.from.user.displayName} timestamp={moment(item.lastModifiedDateTime).fromNow()} />,
                      key: item.id,
                    })
                  }
                });

                setAllMessages([]);
                setAllMessages(elements.reverse());
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