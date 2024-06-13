// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import "./Profile.css";

import React from "react";
import defaultPhoto from '../../images/default-photo.png';

class Profile extends React.Component {
  render() {
    return (
      <div className="profile">
        <div className="photo">
          <img src={defaultPhoto} alt="avatar" />
        </div>
        <div className="info">
          <div className="name">{this.props.userInfo.displayName}</div>
          <div className="email">{this.props.userInfo.preferredUserName}</div>
        </div>
      </div>
    );
  }
}

export default Profile;
