import React, { Component } from 'react';
import { List } from '@fluentui/react-northstar';
import Constants from '../constants'; 
import ReactList from 'react-list'; 

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
                  {/* <ul className="list list-center"> {listItems} </ul>   */}
               <List className="list-center" title="Get your token"
                    items={this.props.items.map(item => (`${item.TokenNumber}. ${this.markIfOrganizer(item)}`))}
                /> 
            </div>
        );
    }
}

export default TokenActionButtons;