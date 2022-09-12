import React, { Component } from 'react';
import { List } from '@fluentui/react-northstar';
import Constants from '../Constants';

class TokenActionButtons extends Component {
    
    markIfOrganizer(item) {
        if(item.UserInfo.Role.MeetingRole === Constants.MeetingRoles.Organizer) {
            return `${item.UserInfo.Name} (${item.UserInfo.Role.MeetingRole})`
        }

        return `${item.UserInfo.Name}`
    }

    render() {
        return (
            <div className="flex-center">
                <List className="list-center" title="Get your token"
                    items={this.props.items.map(item => (`${item.TokenNumber}. ${this.markIfOrganizer(item)}`))}
                />
            </div>
        );
    }
}

export default TokenActionButtons;