# Meetings Details Tab dependency maintenance

The server expands Adaptive Card JSON with `adaptivecards-templating` and sends
it using Teams `CardFactory.adaptiveCard`. It does not render cards in a browser,
so the unused `adaptivecards` renderer (and its vulnerable Swiper peer) has been
removed. Keep the `adaptive-expressions` override for `fast-xml-parser` 4.5.7
until the upstream dependency supplies a patched 4.x version.

Validate the server with `npm ci && node build.js`. The client is a separate
application: run `npm ci --legacy-peer-deps && npm run build` from `client`. Its existing
webpack 4 tooling requires `NODE_OPTIONS=--openssl-legacy-provider` on modern
Node.js.
