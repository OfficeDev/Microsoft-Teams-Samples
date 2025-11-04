// Channel notifications logic
// Uses Teams JS SDK, axios, and moment (loaded globally)

document.addEventListener('DOMContentLoaded', function () {
    let currentTeamId = null;
    let currentChannelId = null;
    
    // Initialize Teams SDK
    microsoftTeams.app.initialize().then(() => {
        console.log('Teams SDK initialized');
        // Get context
        microsoftTeams.app.getContext().then(context => {
            currentTeamId = context.channel.ownerGroupId;
            currentChannelId = context.channel.id;
            
            // Create/change channel subscription
            axios.post(`/api/changeNotification?teamId=${currentTeamId}&channelId=${currentChannelId}`)
                .then(() => {
                    fetchNotifications();
                    // Automatically fetch member list on startup
                    fetchMemberListInternal();
                })
                .catch(err => {
                    // Log Graph API error details for debugging
                    if (err.response) {
                        console.error('Error creating subscription:', err.response.status, err.response.data);
                    } else {
                        console.error('Error creating subscription:', err.message);
                    }
                    // Still fetch existing notifications and member list
                    fetchNotifications();
                    fetchMemberListInternal();
                });
        }).catch(err => console.error('Failed to get context:', err));
    }).catch(err => console.error('Teams SDK failed to initialize:', err));

    // Internal function to fetch member list (used on startup)
    function fetchMemberListInternal() {
        if (!currentTeamId || !currentChannelId) {
            console.error('Team/Channel context not available for member list fetch');
            return;
        }
        
        const memberContainer = document.getElementById('member-list');
        memberContainer.innerHTML = '<p>Loading member list...</p>';
        
        axios.get(`/api/members/${currentTeamId}/${currentChannelId}`)
            .then(response => {
                const data = response.data;
                let html = `<h4>Channel Members (${data.members.length})</h4>`;
                html += `<p><small>Last updated: ${moment(data.timestamp).format('LLL')}</small></p>`;
                
                if (data.members.length === 0) {
                    html += '<p>No members found.</p>';
                } else {
                    html += '<ul>';
                    data.members.forEach(member => {
                        html += `<li>${member.displayName || member.email || member.id}</li>`;
                    });
                    html += '</ul>';
                }
                
                memberContainer.innerHTML = html;
            })
            .catch(err => {
                console.error('Error fetching member list:', err);
                memberContainer.innerHTML = '<p class="error">Error loading member list. Please try again.</p>';
            });
    }

    // Add member list functionality
    window.fetchMemberList = function() {
        fetchMemberListInternal();
    };

    // Fetch notifications and render
    function fetchNotifications() {
        axios.get('/api/notifications')
            .then(response => renderNotifications(response.data))
            .catch(err => {
                console.error('Error fetching notifications:', err);
                const container = document.getElementById('notifications');
                container.innerHTML = '<p class="error">Error loading notifications. Please try again.</p>';
            });
    }

    function renderNotifications(data) {
        const container = document.getElementById('notifications');
        container.innerHTML = '';
        if (!data || data.length === 0) {
            container.innerHTML = '<p>No notifications yet.</p>';
            return;
        }
        // Reverse for newest first
        data.slice().reverse().forEach(item => {
            debugger;
            const div = document.createElement('div');
            let html = '';
            if (item.changeType === 'created' && item.displayName !== null) {
                html += `<p><b>Description:</b> Channel is shared with a Team</p>` +
                    `<p><b>Event Type:</b> <span class="statusColor"><b>${item.changeType}</b></span></p>` +
                    `<p><b>Team Name:</b> ${item.displayName}</p>`;
            }else if (item.changeType === 'deleted' && item.displayName !== null) {
                html += `<p><b>Description:</b> Channel is unshared from a Team</p>` +
                    `<p><b>Event Type:</b> <span class="deleteStatus"><b>${item.changeType}</b></span></p>` +
                    `<p><b>Team Name:</b> ${item.displayName}</p>`;
            }else if (item.changeType === 'updated') {
                html += `<p><b>Description:</b> Users membership updated</p>` +
                    `<p><b>Event Type:</b> <span class="statusColor"><b>${item.changeType}</b></span></p>`;
            }else if (item.changeType === 'created') {
                html += `<p><b>Description:</b> New user has been Added</p>` +
                    `<p><b>Event Type:</b> <span class="statusColor"><b>${item.changeType}</b></span></p>`;
            }else if (item.changeType === 'deleted') {
                html += `<p><b>Description:</b> User has been removed</p>` +
                    `<p><b>Event Type:</b> <span class="deleteStatus"><b>${item.changeType}</b></span></p>` +
                    `<p><b>User Access Status:</b> <span class="userAccess"><b>${item.hasUserAccess}</b></span></p>`;
            }
            
            // Add member list update information
            if (item.memberListUpdated !== undefined) {
                html += `<p><b>Member List Updated:</b> <span class="${item.memberListUpdated ? 'statusColor' : 'deleteStatus'}"><b>${item.memberListUpdated}</b></span></p>`;
                
                if (item.memberListUpdated && item.currentMemberCount !== undefined) {
                    html += `<p><b>Current Member Count:</b> ${item.currentMemberCount}</p>`;
                }
                
                if (!item.memberListUpdated && item.memberListSkipReason) {
                    html += `<p><b>Skip Reason:</b> ${item.memberListSkipReason}</p>`;
                }
                
                if (item.memberListUpdateError) {
                    html += `<p><b>Update Error:</b> <span class="deleteStatus">${item.memberListUpdateError}</span></p>`;
                }
            }
              
            html += `<p><b>Date:</b> ${moment(item.createdDate).format('LLL')} ` +
                `<b><span class="headColor">${moment(item.createdDate).fromNow()}</span></b></p>` +
                `<hr/>`;
            div.innerHTML = html;
            container.appendChild(div);
        });
    }
});
