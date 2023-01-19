import React, { Component } from 'react';
import { Button } from '@fluentui/react-components';
import { withMeetingTokenService } from '../context/MeetingServiceProvider';

class DoneTokenButton extends Component {
    constructor(props) {
        super(props);
        this.state = {
            disabled: false,
        };
    }
    
    doneToken = async () => {
        const { meetingTokenService } = this.props;
        this.setState({ disabled: true });
        const serviceCall = async () => {
            const data = await meetingTokenService.ackTokenAsync();
            this.setState({ disabled: false });
            this.props.onAckToken(data);
        }
        await serviceCall();
    }

    render() {
        return (
            <div className="flex-center" >
                {this.props.show ?
                    (
                        <Button content="Done" className="submit-button done-button" disabled={this.state.disabled} style={{ backgroundColor:"green !important", borderColor:"green !important" }} label="Acknowledge that you are done with the token" onClick={this.doneToken}>Done</Button>
                    ) : null
                }
            </div>
        );
    }
}

export default withMeetingTokenService(DoneTokenButton);