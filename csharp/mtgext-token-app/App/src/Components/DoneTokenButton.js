import React, { Component } from 'react';
import { PrimaryButton } from '@fluentui/react/lib/Button';
import { withMeetingTokenService } from '../Context/MeetingServiceProvider';

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
                        <PrimaryButton disabled={this.state.disabled} style={{ margin: 10,backgroundColor:"green", borderColor:"green" }} title="Acknowledge that you are done with the token" onClick={this.doneToken}>
                            Done
                        </PrimaryButton>
                    ) : null
                }
            </div>
        );
    }
}

export default withMeetingTokenService(DoneTokenButton);