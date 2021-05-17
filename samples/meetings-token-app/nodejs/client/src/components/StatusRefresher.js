import React, { Component } from 'react';
import StatusRefresh from '../service/StatusRefresh';
import { withMeetingTokenService } from '../context/MeetingServiceProvider';

class StatusRefresher extends Component {
    constructor(props){
        super(props);
        this.statusRefresh = null;
        this.cancelToken = null;
    }

    componentDidMount() {
        if(this.props.meetingTokenService) {
            this.statusRefresh = new StatusRefresh(this.props.meetingTokenService);
            this.statusRefresh.start(this.props.onStatusRefresh);
        }
    }

    componentWillUnmount() {
        if (this.cancelToken || this.props.meetingTokenService) {
            this.statusRefresh.stop();
        }
    }

    render() {
        return <div />;
    }
}

export default withMeetingTokenService(StatusRefresher);