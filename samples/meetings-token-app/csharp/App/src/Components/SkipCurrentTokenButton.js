import React, { Component } from 'react';
import { PrimaryButton } from '@fluentui/react/lib/Button';
import { withMeetingTokenService } from '../Context/MeetingServiceProvider';
import { async } from 'regenerator-runtime';

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
                        <PrimaryButton disabled={this.state.disabled} style={{ margin: 10, backgroundColor:"#CC4A31", borderColor:"#CC4A31" }} title="Move current token to next" onClick={this.skipToken}>
                            Skip
                        </PrimaryButton>
                    ) : null
                }
            </div>
        );
    }
}

export default withMeetingTokenService(SkipCurrentTokenButton);