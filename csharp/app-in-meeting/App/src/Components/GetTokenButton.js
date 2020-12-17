import React, { Component } from 'react';
import { PrimaryButton } from '@fluentui/react/lib/Button';
import { withMeetingTokenService } from '../Context/MeetingServiceProvider';

class GetTokenButton extends Component {
    constructor(props) {
        super(props);
        this.state = {
            disabled: false,
        };
    }

    getToken = async () => {
        const { meetingTokenService } = this.props;
        this.setState({ disabled: true });
        const serviceCall = async () => {
            const data = await meetingTokenService.getMeetingTokenAsync();
            this.setState({ disabled: false });
            this.props.onGetToken(data);
        };
        await serviceCall();
    }

    render() {
        return (
            <div className="flex-center">
                {this.props.show ?
                    (
                        <PrimaryButton disabled={this.state.disabled} style={{ margin: 10, backgroundColor:"#6264A7", borderColor:"#6264A7" }} title="Get your meeting token" onClick={this.getToken}>
                            Token
                        </PrimaryButton>
                    ) : null
                }
            </div>
        );
    }
}

export default withMeetingTokenService(GetTokenButton);