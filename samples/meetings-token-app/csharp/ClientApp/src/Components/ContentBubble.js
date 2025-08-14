import React from "react";

function ContentBubble(props) {
    const query = new URLSearchParams(decodeURIComponent(window.location.search));

    return (
        <div>
            <div style={{color: 'white', flexFlow: 'column', textAlign: 'center'}}>Current Token</div>
            <h2 id="current-token" style={{color: 'white', flexFlow: 'column', alignContent: 'center', justifyContent: 'center', textAlign: 'center'}}>
                {query.get('user')? query.get('user') : 'N/A'}
            </h2>

            <h3 id="current-user" style={{color: 'white', flexFlow: 'column', alignContent: 'center', justifyContent: 'center', textAlign: 'center'}}>
                {query.get('token')? query.get('token') : 'N/A'}
            </h3>
        </div>
    );
}

export default ContentBubble;