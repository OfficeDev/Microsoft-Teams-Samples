import React, { Component } from 'react';
import { Text } from '@fluentui/react-northstar';
import { withMeetingTokenService } from '../Context/MeetingServiceProvider';

class ErrorMessageBar extends Component {
    render() {
        if (!this.props.show) {
            return null;
        }
        return (
            <div >
                <Text error className="error-bar" >{this.props.msg}</Text>
            </div>
        );
    }
}

export default withMeetingTokenService(ErrorMessageBar);