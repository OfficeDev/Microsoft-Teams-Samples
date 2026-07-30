import send from 'send';

/**
 * Sets up routes for handling the tabs and their content in the application.
 * 
 * @param {Object} server - The Restify server instance.
 */
export default function tabs(server) {
    // Helper function to streamline serving static HTML files.
    const serveHtml = (filePath, req, res, next) => {
        send(req, filePath).pipe(res).on('error', next);  // Handle errors in the send pipe.
    };

    // Setup home page route.
    server.get('/', (req, res, next) => serveHtml('src/views/hello.html', req, res, next));

    // Setup the static tab route.
    server.get('/hello', (req, res, next) => serveHtml('src/views/hello.html', req, res, next));

    // Setup the configure tab route.
    server.get('/configure', (req, res, next) => serveHtml('src/views/configure.html', req, res, next));

    // Setup content tab with separate 'First' view.
    server.get('/first', (req, res, next) => serveHtml('src/views/first.html', req, res, next));

    // Setup content tab with separate 'second' view.
    server.get('/second', (req, res, next) => serveHtml('src/views/second.html', req, res, next));
}
