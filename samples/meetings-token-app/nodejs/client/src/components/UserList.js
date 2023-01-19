import React, { Component } from 'react';
import Constants from '../constants';

class TokenActionButtons extends Component {
    
    markIfOrganizer(item) {
        if(item.UserInfo.Role.MeetingRole === Constants.MeetingRoles.Organizer) {
            return `${item.UserInfo.Name} (${item.UserInfo.Role.MeetingRole})`
        }
        
        return `${item.UserInfo.Name}`
    }

    render() {
        const listItems = this.props.items.map((myList)=>{   
            return <li>{`${myList.TokenNumber}. ${this.markIfOrganizer(myList)}`}</li>;   
        });  
        return (
            <div className="flex-center">
                <ul className="list list-center"> {listItems} </ul>
            </div>
        );
    }
}

export default TokenActionButtons;