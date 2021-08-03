import * as microsoftTeams from "@microsoft/teams-js";

function App() {
    return (
        <button onClick={test}>Execute deeplink!</button>
    );
}

function test() {
    let url = "https://teams.microsoft.com/l/stage/8749ff03-ae9e-4d33-88d8-bc9761ea7335/0?context=%7B%22contentUrl%22%3A%22https%3A%2F%2F2f8fec398a1e.ngrok.io%2Ftab%22%2C%22websiteUrl%22%3A%22https%3A%2F%2F2f8fec398a1e.ngrok.io%2Ftab%22%2C%22name%22%3A%22Contoso%22%7D";
    microsoftTeams.executeDeepLink(url);
};

export default App;
