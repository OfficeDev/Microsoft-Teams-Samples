import axios from "axios";
import AuthService from "./Auth";
import ContextService from "./Context";
const defaultOptions = {
    timeout: 10000,
}

export default class MeetingTokenService {
    constructor(teamsClient, options = defaultOptions) {
        if (!teamsClient) {
            throw "teams client not initialized";
        }
        this.options = options;
        this.client = axios.create({
            timeout: options.timeout,
        });
        this._auth = AuthService(teamsClient);
        this._userContext = ContextService(teamsClient, options.timeout);
    }

    withServices(...a) {
        return Promise.all(a);
    }

    // Gets the meeting token for the current user.
    // returns: token object, ref Models/UserToken.cs
    getMeetingTokenAsync = async () => {
        return this.withServices(this._auth(), this._userContext())
            .then(([token, context]) => {
                console.log(context);
                return axios.post('/api/me/token', {}, {
                    params: {
                        meetingId: context.meetingId,
                        tenantId: context.tid,
                        userId: context.userObjectId
                    },
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                })
            })
            .then(meetingTokenResp => ({
                success: true,
                msg: meetingTokenResp.data,
            }
            ))
            .catch(_err => {
                console.error("Error while fetching meeting token: ", _err);
                return {
                    success: false,
                    msg: `Something went wrong`,
                }
            });
    }

    // Gets the status of the current meeting.
    // returns: MeetingTokenStatus object, ref Models/MeetingTokenStatus.cs
    getMeetingStatusAsync = async () => {
        return this.withServices(this._auth(), this._userContext())
            .then(([token, context]) => {
                console.log(context);
                return axios.get('/api/meeting/summary', {
                    params: {
                        meetingId: context.meeting.id,
                        tenantId: context.user.tenant.id,
                        userId: context.user.id
                    },
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                })
            })
            .then(meetingTokenResp => ({
                success: true,
                msg: meetingTokenResp.data,
            }
            ))
            .catch(_err => {
                console.error("Error while fetching meeting summary: ", _err);
                return {
                    success: false,
                    msg: `Something went wrong`,
                }
            });
    }

    // Gets the role of the user in the current meeting
    // returns: returns UserRoleServiceResponse, ref: Service/UserRoleServiceResponse.cs
    getUserInfoAsync = async () => {
        return this.withServices(this._auth(), this._userContext())
            .then(([token, context]) => {
                return axios.get('/api/me', {
                    params: {
                        meetingId: context.meeting.id,
                        tenantId: context.user.tenant.id,
                        userId: context.user.id
                    },
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                })
            })
            .then(meetingTokenResp => ({
                success: true,
                msg: meetingTokenResp.data,
            }))
            .catch(_err => {
                console.error("Error while fetching meeting token: ", _err);
                return {
                    success: false,
                    msg: "Something went wrong",
                }
            });
    }

    // Acknowledge that the user is done with the token
    // returns: MeetingTokenStatus object, ref Models/MeetingTokenStatus.cs 
    ackTokenAsync = async () => {
        return this.withServices(this._auth(), this._userContext())
            .then(([token, context]) => {
                return axios.post('/api/me/ack-token', {}, {
                    params: {
                        meetingId: context.meeting.id,
                        tenantId: context.user.tenant.id,
                        userId: context.user.id
                    },
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                })
            })
            .then(meetingTokenResp => ({
                success: true,
                msg: meetingTokenResp.data,
            }))
            .catch(_err => {
                console.error("Error acknowledging token: ", _err);
                return {
                    success: false,
                    msg: "Something went wrong",
                }
            });
    }

    // Skip the current token user, This is only available for Organizer
    // returns: MeetingTokenStatus object, ref Models/MeetingTokenStatus.cs
    skipTokenAsync = async () => {
        return this.withServices(this._auth(), this._userContext())
            .then(([token, context]) => {
                return axios.post('/api/user/skip', {}, {
                    params: {
                        meetingId: context.meeting.id,
                        tenantId: context.user.tenant.id,
                        userId: context.user.id
                    },
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                })
            })
            .then(meetingTokenResp => ({
                success: true,
                msg: meetingTokenResp.data,
            }))
            .catch(_err => {
                console.error("Error skipping token: ", _err);
                return {
                    success: false,
                    msg: "Something went wrong",
                }
            });
    }
}