import React, { Component } from 'react';
import { Button } from '@fluentui/react-northstar';
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
                        <Button className="submit-button" content="Token" disabled={this.state.disabled} title="Get your meeting token" onClick={this.getToken} />
                    ) : null
                }
            </div>
        );
    }
}

export default withMeetingTokenService(GetTokenButton);