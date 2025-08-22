/**
 * Agenda Component - JavaScript equivalent of React Agenda.js
 * Renders individual agenda items with poll functionality
 */

window.Agenda = (function() {
    'use strict';
    
    function render(agendaData, taskList) {
        const { title, option1, option2, Id, IsSend } = agendaData;
        
        const container = document.createElement('div');
        
        if (!IsSend) {
            // Not sent yet - show send form
            container.className = 'card agendaCard mb-3';
            container.innerHTML = `
                <div class="card-body">
                    <h5 class="card-title">${title}</h5>
                    <input type="radio" class="option1" name="option1_${Id}" value="${option1}"/>
                    <label class="pollLabel" for="option1_${Id}">${option1}</label><br/>
                    <input type="radio" class="option2" name="option2_${Id}" value="${option2}"/>
                    <label class="pollLabel" for="option2_${Id}">${option2}</label><br/>
                </div>
                <div class="card-footer">
                    <button type="button" class="btn btn-primary" data-id="${Id}">Send</button>
                </div>
            `;
            
            // Set up send button event handler
            const sendButton = container.querySelector('button[data-id="' + Id + '"]');
            if (sendButton) {
                sendButton.addEventListener('click', () => sendAgenda(Id, taskList));
            }
        } else {
            // Already sent - show results
            const taskInfo = taskList.find(x => x.Id == Id);
            const { personAnswered } = taskInfo;
            const option1Answered = personAnswered && personAnswered[option1] ? personAnswered[option1].length : 0;
            const option2Answered = personAnswered && personAnswered[option2] ? personAnswered[option2].length : 0;
            
            const total = option1Answered + option2Answered;
            const percentOption1 = total == 0 ? 0 : parseInt((option1Answered * 100) / total);
            const percentOption2 = total == 0 ? 0 : 100 - percentOption1;
            
            container.className = 'card agendaCard mb-3';
            container.innerHTML = `
                <div class="card-body">
                    <h5 class="card-title">${title}</h5>
                    <span class="option1">${option1}</span><br/>
                    <span class="resultpercentage">${percentOption1}% has answered</span><br />
                    <span class="option2">${option2}</span><br/>
                    <span class="resultpercentage">${percentOption2}% has answered</span>
                </div>
                <div class="card-footer">
                    <button type="button" class="btn btn-primary btnResult" data-id="${Id}">Results</button>
                </div>
            `;
            
            // Set up results button event handler
            const resultsButton = container.querySelector('button[data-id="' + Id + '"]');
            if (resultsButton) {
                resultsButton.addEventListener('click', () => openResultModule(Id));
            }
        }
        
        return container;
    }
    
    function sendAgenda(id, taskList) {
        const taskInfo = taskList.find(x => x.Id == id);
        taskInfo.IsSend = true;
        
        fetch("/api/sendAgenda", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ taskInfo, taskList }),
        }).then(response => {
            if (response.ok) {
                // Refresh the agenda display
                if (window.Welcome && window.Welcome.loadAgenda) {
                    window.Welcome.loadAgenda();
                }
            }
        }).catch(error => {
            console.error('Error sending agenda:', error);
        });
    }
    
    function submitHandler(dialogResponse) {
        return true;
    }
    
    function openResultModule(id) {
        // Try to get agendaList from global scope first
        let agendaList = window.agendaList;
        
        if (!agendaList || agendaList.length === 0) {
            // If not available, fetch from API
            fetch('/api/getAgendaList')
                .then(response => response.json())
                .then(data => {
                    agendaList = data || [];
                    window.agendaList = agendaList; // Store globally for future use
                    showResultModal(id, agendaList);
                })
                .catch(error => {
                    console.error('Error fetching agenda list:', error);
                    alert('Unable to load poll results. Please try again.');
                });
        } else {
            showResultModal(id, agendaList);
        }
    }
    
    function showResultModal(id, agendaList) {
        // Find the poll data for this ID
        const pollData = agendaList.find(item => item.Id === id);
        if (!pollData) {
            console.error('Poll not found for ID:', id);
            alert('Poll not found.');
            return;
        }
        
        // Calculate vote statistics
        const personAnswered = pollData.personAnswered || {};
        const option1 = pollData.option1 || 'Option 1';
        const option2 = pollData.option2 || 'Option 2';
        
        let option1Votes = 0;
        let option2Votes = 0;
        
        // Count votes for each option
        for (const [key, voters] of Object.entries(personAnswered)) {
            if (Array.isArray(voters)) {
                const voteCount = voters.length;
                if (key === option1 || key.endsWith('_option1')) {
                    option1Votes += voteCount;
                } else if (key === option2 || key.endsWith('_option2')) {
                    option2Votes += voteCount;
                }
            }
        }
        
        const totalVotes = option1Votes + option2Votes;
        const percent1 = totalVotes === 0 ? 0 : Math.round((option1Votes * 100) / totalVotes);
        const percent2 = totalVotes === 0 ? 0 : 100 - percent1;
        
        // Create adaptive card for the modal
        const adaptiveCard = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "version": "1.2",
            "type": "AdaptiveCard",
            "body": [
                {
                    "type": "TextBlock",
                    "text": " Poll Results",
                    "size": "Medium",
                    "weight": "Bolder",
                    "color": "Accent",
                    "horizontalAlignment": "Center"
                },
                {
                    "type": "TextBlock",
                    "text": pollData.title || "Poll",
                    "size": "Default",
                    "weight": "Bolder",
                    "wrap": true,
                    "horizontalAlignment": "Center",
                    "spacing": "Medium"
                },
                {
                    "type": "Container",
                    "spacing": "Medium",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": `**${option1}**`,
                            "weight": "Bolder",
                            "color": "Good"
                        },
                        {
                            "type": "TextBlock",
                            "text": `${percent1}% (${option1Votes} votes)`,
                            "color": "Good",
                            "spacing": "Small"
                        },
                        {
                            "type": "TextBlock",
                            "text": `**${option2}**`,
                            "weight": "Bolder",
                            "color": "Attention",
                            "spacing": "Medium"
                        },
                        {
                            "type": "TextBlock",
                            "text": `${percent2}% (${option2Votes} votes)`,
                            "color": "Attention",
                            "spacing": "Small"
                        }
                    ]
                },
                {
                    "type": "TextBlock",
                    "text": `Total Votes: ${totalVotes}`,
                    "size": "Small",
                    "color": "Default",
                    "horizontalAlignment": "Center",
                    "spacing": "Medium"
                }
            ]
        };
        
        // Create task info with embedded card instead of URL
        let taskInfo = {
            title: "Poll Results",
            card: adaptiveCard,
            size: {
                height: 400,
                width: 600,
            }
        };
        
        // Try to open in Teams modal, fallback to web modal
        if (typeof microsoftTeams !== 'undefined') {
            microsoftTeams.app.initialize().then(() => {
                if (microsoftTeams.dialog && microsoftTeams.dialog.adaptiveCard) {
                    microsoftTeams.dialog.adaptiveCard.open(taskInfo, submitHandler);
                } else {
                    // Fallback to older Teams API
                    microsoftTeams.tasks.startTask(taskInfo, submitHandler);
                }
            }).catch((error) => {
                console.error('Error opening result module in Teams:', error);
                // Fallback: Create a web modal with the results
                showWebModal(pollData, option1Votes, option2Votes, percent1, percent2, totalVotes);
            });
        } else {
            // Not in Teams environment, show web modal
            showWebModal(pollData, option1Votes, option2Votes, percent1, percent2, totalVotes);
        }
    }
    

    
    function showWebModal(pollData, option1Votes, option2Votes, percent1, percent2, totalVotes) {
        // Create a web modal for non-Teams environments
        const modalHTML = `
            <div id="resultsModal" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 1000; display: flex; align-items: center; justify-content: center;">
                <div style="background: white; padding: 30px; border-radius: 10px; max-width: 500px; width: 90%; box-shadow: 0 4px 6px rgba(0,0,0,0.1);">
                    <h2 style="text-align: center; margin-bottom: 20px; color: #6264a7;">Poll Results</h2>
                    <h3 style="text-align: center; margin-bottom: 30px;">${pollData.title || 'Poll'}</h3>
                    
                    <div style="margin-bottom: 20px;">
                        <div style="font-weight: bold; color: #323130; margin-bottom: 5px;">${pollData.option1 || 'Option 1'}</div>
                        <div style="color: #605e5c; margin-bottom: 15px;">${percent1}% (${option1Votes} votes)</div>
                        
                        <div style="font-weight: bold; color: #323130; margin-bottom: 5px;">${pollData.option2 || 'Option 2'}</div>
                        <div style="color: #605e5c; margin-bottom: 15px;">${percent2}% (${option2Votes} votes)</div>
                    </div>
                    
                    <div style="text-align: center; font-size: 14px; color: #605e5c; margin-bottom: 20px;">
                        Total Votes: ${totalVotes}
                    </div>
                    
                    <button onclick="document.getElementById('resultsModal').remove()" style="display: block; margin: 0 auto; padding: 10px 20px; background: #6264a7; color: white; border: none; border-radius: 4px; cursor: pointer;">Close</button>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', modalHTML);
    }
    
    function submitHandler(error, result) {
        // Handle modal close or any results
        if (error) {
            console.error('Modal error:', error);
        }
        // Modal closed, no further action needed
    }
    
    // Public API
    return {
        render: render
    };
})();
