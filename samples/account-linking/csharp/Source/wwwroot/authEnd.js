
// The following is a helper to wrap the initialize call in a promise and to detect if we're not 
// running inside of Teams
const teamsPromise = Promise.race([
    new Promise(resolve => microsoftTeams.app.initialize().then(() => resolve())),
    new Promise((resolve, reject) => setTimeout(() => reject('Failed to initialize connection with Microsoft Teams'), 250))]);

// Entrypoint of the script
async function main() {
    try {
        await teamsPromise;
        console.log("auth end started");
        const currentUri = new URL(window.location.href);
        var state = currentUri.searchParams.get('state');
        var code = currentUri.searchParams.get('code');
        // We need to send back the 'state' and the 'code' so that the caller can claim the token(s)
        microsoftTeams.authentication.notifySuccess(JSON.stringify({state, code}));
    } catch (ex) {
        console.error('Error happened while trying to end auth', ex);
        console.log("Attempting to close window");
        window.close();
    }
}
  
// Execute the entrypoint and log any errors
main().catch(err => console.error(err));