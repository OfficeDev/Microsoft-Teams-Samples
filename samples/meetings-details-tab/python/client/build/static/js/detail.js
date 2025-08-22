/**
 * Detail Component - JavaScript equivalent of React Detail.js
 * Handles poll creation form and poll details viewing
 */

window.Detail = (function() {
    'use strict';
    
    let currentPoll = null;
    
    function render() {
        // Check for poll ID in URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        const pollId = urlParams.get('id');
        
        const container = document.createElement('div');
        container.className = 'surface theme-light';
        
        if (pollId) {
            // View existing poll details
            container.innerHTML = `
                <div id="PollDetails" style="margin: 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
                    <h3 style="margin-bottom: 20px; font-weight: 600;">Poll Details</h3>
                    
                    <div id="LoadingPoll" style="text-align: center; color: #605e5c; padding: 20px;">
                        Loading poll details...
                    </div>
                    
                    <div id="PollDetailsCard" style="display: none; border: 1px solid #ddd; border-radius: 4px; padding: 20px; margin-bottom: 20px; background-color: #f9f9f9;">
                        <!-- Poll details will be inserted here -->
                    </div>
                    
                    <div id="PollError" style="display: none; color: #c4314b; text-align: center; margin-bottom: 20px;">
                        <!-- Error message will be shown here -->
                    </div>
                    
                    <div style="text-align: center; margin-bottom: 10px;">
                        <button onclick="createNewPoll()" 
                                style="background-color: #6264a7; color: white; border: 1px solid #6264a7; padding: 10px 20px; border-radius: 4px; font-size: 14px; cursor: pointer;">
                            Create New Poll
                        </button>
                    </div>
                </div>
            `;
            
            // Load poll details
            setTimeout(() => {
                loadPollDetails(pollId);
            }, 100);
            
        } else {
            // Create new poll form
            container.innerHTML = `
                <div id="PollForm" style="margin: 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
                    <h3 style="margin-bottom: 20px; font-weight: 600;">Add a Poll</h3>
                    
                    <div style="margin-bottom: 15px;">
                        <label style="display: block; margin-bottom: 5px; font-weight: 500;">Title:</label>
                        <input type="text" id="title" name="title" 
                               style="width: 100%; padding: 8px 12px; border: 1px solid #ccc; border-radius: 4px; font-size: 14px;"
                               placeholder="Enter poll question"/>
                    </div>
                    
                    <div style="margin-bottom: 15px;">
                        <label style="display: block; margin-bottom: 5px; font-weight: 500;">Choice 1:</label>
                        <input type="text" id="option1" name="option1" 
                               style="width: 100%; padding: 8px 12px; border: 1px solid #ccc; border-radius: 4px; font-size: 14px;"
                               placeholder="Enter first option"/>
                    </div>
                    
                    <div style="margin-bottom: 20px;">
                        <label style="display: block; margin-bottom: 5px; font-weight: 500;">Choice 2:</label>
                        <input type="text" id="option2" name="option2" 
                               style="width: 100%; padding: 8px 12px; border: 1px solid #ccc; border-radius: 4px; font-size: 14px;"
                               placeholder="Enter second option"/>
                    </div>
                    
                    <div style="text-align: center; margin-bottom: 20px;">
                        <button onclick="validateForm()" 
                                style="background-color: #0078d4; color: white; border: none; padding: 10px 20px; border-radius: 4px; font-size: 14px; cursor: pointer;">
                            Create
                        </button>
                    </div>
                </div>
                
                <!-- Poll Preview Section (initially hidden) -->
                <div id="PollPreview" style="display: none; margin: 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
                    <h4 style="margin-bottom: 20px; font-weight: 600;">Poll Created Successfully</h4>
                    
                    <div id="PollCard" style="border: 1px solid #ddd; border-radius: 4px; padding: 20px; margin-bottom: 20px; background-color: #f9f9f9;">
                        <!-- Poll card content will be inserted here -->
                    </div>
                    
                    <div style="text-align: center; margin-bottom: 10px;">
                        <button onclick="sendPollToChat()" 
                                style="background-color: #0078d4; color: white; border: none; padding: 10px 20px; border-radius: 4px; font-size: 14px; cursor: pointer; margin-right: 10px;">
                            Send
                        </button>
                        <button onclick="createNewPoll()" 
                                style="background-color: #6c757d; color: white; border: none; padding: 10px 20px; border-radius: 4px; font-size: 14px; cursor: pointer;">
                            Create New Poll
                        </button>
                    </div>
                    
                    <div id="sendStatus" style="margin-top: 15px; text-align: center;"></div>
                </div>
            `;
        }
        
        // Set up global functions after rendering
        setTimeout(() => {
            window.validateForm = validateForm;
            window.sendPollToChat = sendPollToChat;
            window.createNewPoll = createNewPoll;
            window.loadPollDetails = loadPollDetails;
        }, 0);
        
        return container;
    }
    
    function loadPollDetails(pollId) {
        console.log('Loading poll details for ID:', pollId);
        
        const loadingDiv = document.getElementById("LoadingPoll");
        const detailsCard = document.getElementById("PollDetailsCard");
        const errorDiv = document.getElementById("PollError");
        
        if (!loadingDiv || !detailsCard || !errorDiv) {
            console.error('Required DOM elements not found');
            return;
        }
        
        // Fetch poll data from the agenda list
        fetch('/api/getAgendaList')
        .then(response => {
            console.log('API response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(agendaList => {
            console.log('Agenda list data:', agendaList);
            console.log('Looking for poll ID:', pollId);
            
            if (Array.isArray(agendaList)) {
                // Find the poll with the matching ID
                const poll = agendaList.find(item => {
                    const itemId = item.id || item.Id;
                    console.log('Checking poll:', itemId, 'against target:', pollId);
                    return itemId === pollId;
                });
                
                if (poll) {
                    console.log('Found poll:', poll);
                    displayPollDetails(poll);
                } else {
                    console.error('Poll not found with ID:', pollId);
                    console.log('Available poll IDs:', agendaList.map(item => item.id || item.Id));
                    showPollError('Poll not found');
                }
            } else {
                console.error('Unexpected agenda list format:', agendaList);
                showPollError('Invalid poll data format');
            }
        })
        .catch(error => {
            console.error('Error loading poll details:', error);
            showPollError('Error loading poll details: ' + error.message);
        });
    }
    
    function displayPollDetails(poll) {
        const loadingDiv = document.getElementById("LoadingPoll");
        const detailsCard = document.getElementById("PollDetailsCard");
        const errorDiv = document.getElementById("PollError");
        
        loadingDiv.style.display = 'none';
        errorDiv.style.display = 'none';
        detailsCard.style.display = 'block';
        
        // Handle both id and Id property formats
        const pollId = poll.id || poll.Id;
        const pollTitle = poll.title || poll.Title || 'Untitled Poll';
        const option1 = poll.option1 || poll.Option1 || 'Option 1';
        const option2 = poll.option2 || poll.Option2 || 'Option 2';
        
        // Calculate response counts and percentages
        let option1Count = 0;
        let option2Count = 0;
        
        if (poll.personAnswered) {
            if (typeof poll.personAnswered === 'object') {
                // Handle different response formats
                option1Count = (poll.personAnswered[option1] && Array.isArray(poll.personAnswered[option1])) 
                    ? poll.personAnswered[option1].length : 0;
                option2Count = (poll.personAnswered[option2] && Array.isArray(poll.personAnswered[option2])) 
                    ? poll.personAnswered[option2].length : 0;
            }
        }
        
        // Also check direct count properties if available
        if (poll.option1ResponseCount !== undefined) option1Count = poll.option1ResponseCount;
        if (poll.option2ResponseCount !== undefined) option2Count = poll.option2ResponseCount;
        
        const totalResponses = option1Count + option2Count;
        const option1Percentage = totalResponses > 0 ? Math.round((option1Count / totalResponses) * 100) : 0;
        const option2Percentage = totalResponses > 0 ? Math.round((option2Count / totalResponses) * 100) : 0;
        
        // Format creation date
        const createdDate = poll.created_at || poll.Created_at || new Date().toISOString();
        const formattedDate = new Date(createdDate).toLocaleString();
        
        const detailsHtml = `
            <div style="border-bottom: 1px solid #ddd; padding-bottom: 10px; margin-bottom: 15px;">
                <h5 style="margin: 0; font-weight: 600; font-size: 18px;">${pollTitle}</h5>
                <small style="color: #605e5c; font-size: 12px;">Poll ID: ${pollId}</small>
            </div>
            
            <div style="margin-bottom: 20px;">
                <h6 style="margin-bottom: 10px; font-weight: 600;">Results:</h6>
                
                <div style="margin-bottom: 15px;">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 5px;">
                        <span style="font-weight: 500;">A. ${option1}</span>
                        <span style="font-weight: 600; color: #323130;">${option1Count} votes (${option1Percentage}%)</span>
                    </div>
                    <div style="background-color: #e9ecef; border-radius: 4px; height: 8px; overflow: hidden;">
                        <div style="background-color: #0078d4; height: 100%; width: ${option1Percentage}%; transition: width 0.3s ease;"></div>
                    </div>
                </div>
                
                <div style="margin-bottom: 15px;">
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 5px;">
                        <span style="font-weight: 500;">B. ${option2}</span>
                        <span style="font-weight: 600; color: #323130;">${option2Count} votes (${option2Percentage}%)</span>
                    </div>
                    <div style="background-color: #e9ecef; border-radius: 4px; height: 8px; overflow: hidden;">
                        <div style="background-color: #28a745; height: 100%; width: ${option2Percentage}%; transition: width 0.3s ease;"></div>
                    </div>
                </div>
            </div>
            
            <div style="border-top: 1px solid #ddd; padding-top: 15px;">
                <div style="display: flex; justify-content: space-between; align-items: center;">
                    <small style="color: #605e5c; font-size: 12px;">Total Responses: ${totalResponses}</small>
                    <small style="color: #605e5c; font-size: 12px;">Created: ${formattedDate}</small>
                </div>
            </div>
        `;
        
        detailsCard.innerHTML = detailsHtml;
    }
    
    function showPollError(message) {
        const loadingDiv = document.getElementById("LoadingPoll");
        const detailsCard = document.getElementById("PollDetailsCard");
        const errorDiv = document.getElementById("PollError");
        
        loadingDiv.style.display = 'none';
        detailsCard.style.display = 'none';
        errorDiv.style.display = 'block';
        errorDiv.innerHTML = message;
    }
    
    function validateForm() {
        var pollInfo = {
            title: document.getElementById("title").value,
            option1: document.getElementById("option1").value,
            option2: document.getElementById("option2").value
        };
        
        // Validate inputs
        if (!pollInfo.title || !pollInfo.option1 || !pollInfo.option2) {
            alert('Please fill in all fields');
            return false;
        }
        
        // Check if we're in a Teams dialog context
        if (typeof microsoftTeams !== 'undefined' && window.location.search.includes('teamsfx')) {
            // We're in a Teams dialog, submit the data back to the parent
            microsoftTeams.app.initialize().then(() => {
                microsoftTeams.dialog.url.submit(pollInfo);
            }).catch((error) => {
                console.error('Error submitting to Teams dialog:', error);
                // Fallback to creating poll normally
                createPollViaAPI(pollInfo);
            });
        } else {
            // We're not in a Teams dialog, create poll normally
            createPollViaAPI(pollInfo);
        }
        
        return true;
    }
    
    function createPollViaAPI(pollInfo) {
        // Create poll via API
        fetch('/api/createPoll', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(pollInfo)
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Store poll data for sending later
                currentPoll = data.poll_data;
                // Show poll preview
                showPollPreview(data.poll_data);
            } else {
                alert('Error creating poll: ' + (data.error || 'Unknown error'));
            }
        })
        .catch(error => {
            console.error('Error creating poll:', error);
            alert('Error creating poll. Please try again.');
        });
    }
    
    function showPollPreview(pollInfo) {
        // Hide the form
        document.getElementById("PollForm").style.display = "none";
        
        // Show the preview section
        document.getElementById("PollPreview").style.display = "block";
        
        // Create poll card HTML
        const pollCardHtml = `
            <div style="border-bottom: 1px solid #ddd; padding-bottom: 10px; margin-bottom: 15px;">
                <h5 style="margin: 0; font-weight: 600; font-size: 16px;">${pollInfo.title}</h5>
            </div>
            <div>
                <div style="margin-bottom: 8px;">
                    <div style="border: 1px solid #ddd; border-radius: 4px; padding: 8px 12px; background-color: white; text-align: left; font-size: 14px;">
                        <input type="radio" name="pollChoice" id="choice1" value="${pollInfo.option1}" style="margin-right: 8px;">
                        A. ${pollInfo.option1}
                    </div>
                </div>
                <div style="margin-bottom: 8px;">
                    <div style="border: 1px solid #ddd; border-radius: 4px; padding: 8px 12px; background-color: white; text-align: left; font-size: 14px;">
                        <input type="radio" name="pollChoice" id="choice2" value="${pollInfo.option2}" style="margin-right: 8px;">
                        B. ${pollInfo.option2}
                    </div>
                </div>
                <div style="border-top: 1px solid #ddd; padding-top: 10px; margin-top: 15px;">
                    <small style="color: #666; font-size: 12px;">Created: ${new Date().toLocaleString()}</small>
                </div>
            </div>
        `;
        
        document.getElementById("PollCard").innerHTML = pollCardHtml;
    }
    
    function sendPollToChat() {
        console.log('sendPollToChat() called');
        console.log('Current poll data:', currentPoll);
        
        if (!currentPoll) {
            console.error('No poll data available');
            alert('No poll data available');
            return;
        }
        
        const statusDiv = document.getElementById("sendStatus");
        statusDiv.innerHTML = '<div style="color: blue; text-align: center;">Checking bot status...</div>';
        
        console.log('Checking bot status...');
        
        // First check if bot is initialized
        fetch('/api/botStatus')
        .then(response => {
            console.log('Bot status response:', response.status);
            return response.json();
        })
        .then(botStatus => {
            console.log('Bot status data:', botStatus);
            
            if (!botStatus.bot_initialized) {
                console.warn('Bot not initialized');
                statusDiv.innerHTML = `
                    <div style="color: orange; text-align: center;">
                        <strong>Bot Not Ready</strong><br>
                        ${botStatus.message}<br>
                        <small>Go to the Teams chat and send a message like "hello" to the bot first.</small>
                    </div>
                `;
                return;
            }
            
            console.log('Bot is ready, proceeding with sending...');
            
            // Bot is ready, proceed with sending
            statusDiv.innerHTML = '<div style="color: blue; text-align: center;"></div>';
            
            // Create agenda data structure that matches the expected format
            const agendaData = {
                taskList: [], // Empty task list since we're just sending a poll
                taskInfo: currentPoll
            };
            
            console.log('Sending agenda data:', agendaData);
            
            // Send to the existing API endpoint
            return fetch('/api/sendAgenda', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(agendaData)
            });
        })
        .then(response => {
            if (!response) {
                console.log('No response (bot not ready case)');
                return; // Bot not ready case
            }
            
            console.log('SendAgenda response status:', response.status);
            console.log('SendAgenda response:', response);
            
            if (!response.ok) {
                console.error('SendAgenda request failed:', response.status, response.statusText);
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            return response.json();
        })
        .then(data => {
            if (!data) {
                console.log('No data (bot not ready case)');
                return; // Bot not ready case
            }
            
            console.log('SendAgenda response data:', data);
            
            if (data.success) {
                //console.log('Poll sent successfully!');
                statusDiv.innerHTML = '<div style="color: green; text-align: center;"></div>';
            } else if (data.solution) {
                //console.warn('Poll send failed with solution:', data);
                // Enhanced error message with solution
                statusDiv.innerHTML = `
                    <div style="color: orange; text-align: center;">
                        <strong>${data.error}</strong><br>
                        <strong>Solution:</strong> ${data.solution}<br>
                        <small>${data.details}</small>
                    </div>
                `;
            } else {
                console.error('Poll send failed:', data);
                statusDiv.innerHTML = '<div style="color: red; text-align: center;">Failed to send poll: ' + (data.error || 'Unknown error') + '</div>';
            }
        })
        .catch(error => {
            console.error('Error in sendPollToChat:', error);
            console.error('Error stack:', error.stack);
            statusDiv.innerHTML = '<div style="color: red; text-align: center;">Error sending poll to chat: ' + error.message + '</div>';
        });
    }
    
    // function createNewPoll() {
    //     // Clear URL parameters to go back to create mode
    //     const url = new URL(window.location);
    //     url.searchParams.delete('id');
    //     window.history.replaceState(null, '', url);
        
    //     // Re-render the component
    //     const container = document.querySelector('.surface.theme-light');
    //     if (container && container.parentNode) {
    //         const newComponent = render();
    //         container.parentNode.replaceChild(newComponent, container);
    //     }
    // }
    
    // Public API
    return {
        render: render,
        validateForm: validateForm
    };
})();
