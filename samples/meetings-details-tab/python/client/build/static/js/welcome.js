/**
 * Welcome Component - JavaScript equivalent of React welcome.js
 * Handles agenda management and main page functionality
 */

window.Welcome = (function() {
    'use strict';
    
    let agendaList = [];
    
    function render() {
        const container = document.createElement('div');
        container.className = 'container mt-4';
        
        // Create the main content
             
        container.innerHTML = `
            <div>
       
                <button type="button" id="btnAddAgenda" class="btn btn-outline-info">Add Agenda</button>
                <div id="agendaContainer">
                    <!-- Agenda items will be loaded here -->
                </div>
            </div>
        `;
        
        // Set up event handlers after rendering
        setTimeout(() => {
            setupEventHandlers();
            loadAgenda();
        }, 0);
        
        return container;
    }
    
    function setupEventHandlers() {
        const addButton = document.getElementById('btnAddAgenda');
        if (addButton) {
            addButton.addEventListener('click', openTaskModule);
        }
    }
    
    async function loadAgenda() {
        try {
            const response = await fetch("/api/getAgendaList", {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });
            const textData = await response.text();
            if (textData.length) {
                const data = JSON.parse(textData);
                agendaList = data || [];
                renderAgenda();
            }
        } catch (error) {
            console.error('Error loading agenda:', error);
        }
    }
    
    function setAgendaList(list) {
        console.log('Setting agenda list:', list);
        fetch("/api/setAgendaList", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(list),
        })
        .then(response => {
            console.log('setAgendaList response status:', response.status);
            return response.json();
        })
        .then(data => {
            console.log('setAgendaList response data:', data);
            if (data.success) {
                console.log('Agenda list updated successfully');
            } else {
                console.error('Failed to update agenda list:', data);
            }
        })
        .catch(error => {
            console.error('Error updating agenda list:', error);
        });
    }
    
    function submitHandler(dialogResponse) {
        console.log('Dialog response received:', dialogResponse);
        
        let result = dialogResponse.result;
        console.log('Poll result data:', result);
        
        if (!result || !result.title || !result.option1 || !result.option2) {
            console.error('Invalid poll data received:', result);
            return;
        }
        
        // Generate UUID (simple version)
        const taskInfo = { ...result, Id: generateUUID() };
        console.log('Creating poll with ID:', taskInfo.Id);
        
        const list = [...agendaList, taskInfo];
        agendaList = list;
        
        console.log('Updated agenda list:', list);
        setAgendaList(list);
        renderAgenda();
    }
    
    function openTaskModule() {
        // Use the current page's origin for consistency
        const baseUrl = window.location.origin;
        
        let taskInfo = {
            title: "Add a Poll",
            url: baseUrl + "/detail?teamsfx=dialog",
            size: {
                height: 400,
                width: 600,
            },
            fallbackUrl: baseUrl + "/detail"
        };
        
        // Check if we're running in Teams context
        if (typeof microsoftTeams !== 'undefined') {
            microsoftTeams.app.initialize().then(() => {
                microsoftTeams.dialog.url.open(taskInfo, submitHandler);
            }).catch((error) => {
                console.error('Error opening Teams dialog:', error);
                // If Teams dialog fails, navigate to detail page directly
                if (window.App && window.App.navigate) {
                    window.App.navigate('/detail');
                } else {
                    window.location.href = '/detail';
                }
            });
        } else {
            // If not in Teams, navigate directly to detail page
            if (window.App && window.App.navigate) {
                window.App.navigate('/detail');
            } else {
                window.location.href = '/detail';
            }
        }
    }
    
    function renderAgenda() {
        const container = document.getElementById('agendaContainer');
        if (!container) return;
        
        container.innerHTML = '';
        
        if (agendaList && agendaList.length > 0) {
            agendaList.forEach(agenda => {
                if (window.Agenda && window.Agenda.render) {
                    const agendaElement = window.Agenda.render(agenda, agendaList);
                    container.appendChild(agendaElement);
                } else {
                    // Fallback rendering if Agenda component not loaded
                    const agendaElement = document.createElement('div');
                    agendaElement.className = 'agenda-item mb-3 p-3 border rounded';
                    agendaElement.innerHTML = `
                        <h6>${agenda.title || 'Untitled'}</h6>
                        <p>Option 1: ${agenda.option1 || 'N/A'}</p>
                        <p>Option 2: ${agenda.option2 || 'N/A'}</p>
                    `;
                    container.appendChild(agendaElement);
                }
            });
        } else {
            container.innerHTML = '<p class="text-muted">No agenda items yet. Click "Add Agenda" to get started.</p>';
        }
    }
    
    function generateUUID() {
        // Simple UUID generator (matches uuid v4 functionality)
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    
    // Public API
    return {
        render: render,
        loadAgenda: loadAgenda,
        setAgendaList: setAgendaList
    };
})();
