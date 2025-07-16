// Channel notifications logic
// Uses Teams JS SDK, axios, and moment (loaded globally)

document.addEventListener('DOMContentLoaded', function () {
    // Initialize Teams SDK
    microsoftTeams.app.initialize().then(() => {
        console.log('Teams SDK initialized');
        // Get context
        microsoftTeams.app.getContext().then(context => {
            const teamId = context.channel.ownerGroupId;
            const channelId = context.channel.id;
            // Create/change channel subscription
            axios.post(`/api/changeNotification?teamId=${teamId}&channelId=${channelId}`)
                .then(() => fetchNotifications())
                .catch(err => {
                    // Log Graph API error details for debugging
                    if (err.response) {
                        console.error('Error creating subscription:', err.response.status, err.response.data);
                    } else {
                        console.error('Error creating subscription:', err.message);
                    }
                    // Still fetch existing notifications
                    fetchNotifications();
                });
        }).catch(err => console.error('Failed to get context:', err));
    }).catch(err => console.error('Teams SDK failed to initialize:', err));

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
              
            html += `<p><b>Date:</b> ${moment(item.createdDate).format('LLL')} ` +
                `<b><span class="headColor">${moment(item.createdDate).fromNow()}</span></b></p>` +
                `<hr/>`;
            div.innerHTML = html;
            container.appendChild(div);
        });
    }
});
