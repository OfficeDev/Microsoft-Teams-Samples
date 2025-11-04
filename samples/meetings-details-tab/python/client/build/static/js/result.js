/**
 * Result Component - JavaScript equivalent of React Result.js
 * Displays poll results
 */

window.Result = (function() {
    'use strict';
    
    let taskInfo = null;
    
    function render() {
        const container = document.createElement('div');
        container.className = 'container mt-4';
        
        // Initial loading state
        container.innerHTML = `
            <div id="resultContent">
                <div>Loading......</div>
            </div>
        `;
        
        // Load data after rendering
        setTimeout(() => {
            loadResult();
        }, 0);
        
        return container;
    }
    
    async function loadResult() {
        try {
            console.log(" Loading detailed results...");
            
            // 1️FETCH ALL POLLS FROM BACKEND
            const response = await fetch("/api/getAgendaList", {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await response.json();
            
            // 2️⃣ EXTRACT POLL ID FROM URL
            // URL: /Result?id=poll-uuid-123
            const params = window.location.search.split("=");  // ["?id", "poll-uuid-123"]
            const id = params[1];                              // "poll-uuid-123"
            console.log(` Looking for poll ID: ${id}`);
            
            // 3️⃣ FIND SPECIFIC POLL BY ID
            const info = data.find(x => x.Id === id);
            
            if (info) {
                console.log(" Poll data loaded:", info?.title);
                taskInfo = info;
                renderResult();
            } else {
                console.error('Poll not found for ID:', id);
                const resultContent = document.getElementById('resultContent');
                if (resultContent) {
                    resultContent.innerHTML = '<div>Poll not found</div>';
                }
            }
        } catch (error) {
            console.error('Error loading result:', error);
            const resultContent = document.getElementById('resultContent');
            if (resultContent) {
                resultContent.innerHTML = '<div>Error loading results</div>';
            }
        }
    }
    
    function renderResult() {
        const resultContent = document.getElementById('resultContent');
        if (!resultContent) return;
        
        // 🖼️ RENDER DETAILED RESULTS
        if (taskInfo != null) {
            const {option1, option2, personAnswered} = taskInfo;
            
            console.log("Rendering results for:", taskInfo);
            console.log(" PersonAnswered data:", personAnswered);
            
            // Handle different data structures
            let option1Answers = [];
            let option2Answers = [];
            
            if (personAnswered) {
                // Try direct option names first (Pizza, Burger)
                option1Answers = personAnswered[option1] || [];
                option2Answers = personAnswered[option2] || [];
                
                // If not found, try with _option1/_option2 suffix pattern
                if (option1Answers.length === 0) {
                    const option1Key = `${option1}_option1`;
                    option1Answers = personAnswered[option1Key] || [];
                }
                
                if (option2Answers.length === 0) {
                    const option2Key = `${option1}_option2`;  // Notice: uses option1 as prefix
                    option2Answers = personAnswered[option2Key] || [];
                }
                
                // Log what we found
                console.log(" Option1 voters:", option1Answers);
                console.log(" Option2 voters:", option2Answers);
            }

            const totalVotes = option1Answers.length + option2Answers.length;
            const option1Percentage = totalVotes > 0 ? Math.round((option1Answers.length / totalVotes) * 100) : 0;
            const option2Percentage = totalVotes > 0 ? Math.round((option2Answers.length / totalVotes) * 100) : 0;

            resultContent.innerHTML = `
                <div class="card">
                    <div class="card-header bg-primary text-white">
                        � ${taskInfo.title} - Poll Results
                    </div>
                    <div class="card-body">
                        <div class="mb-3">
                            <strong>Total Votes: ${totalVotes}</strong>
                        </div>
                        
                        <!-- OPTION 1 RESULTS -->
                        <div class="mb-3 p-2 border rounded">
                            <h6 class="option1 text-primary"><strong>${option1}</strong></h6>
                            <div class="text-muted">${option1Percentage}% (${option1Answers.length} votes)</div>
                            <div class="resultNames text-success">
                                ${option1Answers.length > 0 
                                    ? ` ${option1Answers.join(", ")}` 
                                    : "No votes yet"}
                            </div>
                        </div>
                        
                        <!-- OPTION 2 RESULTS -->
                        <div class="mb-3 p-2 border rounded">
                            <h6 class="option2 text-primary"><strong>${option2}</strong></h6>
                            <div class="text-muted">${option2Percentage}% (${option2Answers.length} votes)</div>
                            <div class="resultNames text-success">
                                ${option2Answers.length > 0 
                                    ? ` ${option2Answers.join(", ")}` 
                                    : "No votes yet"}
                            </div>
                        </div>
                    </div>
                </div>
            `;
        } else {
            // Show loading state while data is being fetched
            resultContent.innerHTML = '<div>Loading......</div>';
        }
    }
    
    // Public API
    return {
        render: render,
        loadResult: loadResult
    };
})();
