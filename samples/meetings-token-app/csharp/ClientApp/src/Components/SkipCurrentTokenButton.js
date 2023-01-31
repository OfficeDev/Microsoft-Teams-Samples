import React, { Component } from 'react';
import { Button } from '@fluentui/react-northstar';
import { withMeetingTokenService } from '../Context/MeetingServiceProvider';

class SkipCurrentTokenButton extends Component {
    constructor(props) {
        super(props);
        this.state = {
            disabled: false,
        };
    }

    skipToken = async () => {
        const { meetingTokenService } = this.props;
        this.setState({ disabled: true });
        const serviceCall = async () => {
            const data = await meetingTokenService.skipTokenAsync();
            this.setState({ disabled: false });
            this.props.onSkipToken(data);
        }
        await serviceCall();
    }

    async componentDidMount() {
        const { meetingTokenService } = this.props;
        const data = await meetingTokenService.getUserInfoAsync();
        this.props.onUserInfoFetched(data);
    }

    render() {
        // This shows the "Skip" button only if the user has the Organizer role in the meeting
        // NOTE: You *must* also check the user role in the server-side API. Do not rely on the hidden UX alone to enforce role restrictions.
        return (
            <div className="flex-center" >
                {this.props.isOrganizer ?
                    (
                        <Button content="Skip" disabled={this.state.disabled} style={{  }} className="submit-buton" label="Move current token to next" onClick={this.skipToken} />
                    ) : null
                }
            </div>
        );
    }
}

export default withMeetingTokenService(SkipCurrentTokenButton);