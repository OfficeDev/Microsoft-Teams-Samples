import React, { Component } from 'react';
import { MessageBar, MessageBarType } from '@fluentui/react/lib/MessageBar';
import { withMeetingTokenService } from '../Context/MeetingServiceProvider';

class ErrorMessageBar extends Component {
    render() {
        if (!this.props.show) {
            return null;
        }
        return (
            <div >
                <MessageBar messageBarType={MessageBarType.error} className="error-bar" >{this.props.msg}</MessageBar>
                
            </div>
        );
    }
}

export default withMeetingTokenService(ErrorMessageBar);