// Minimal service worker. It exists so browsers treat the app as installable ("Add to Home
// screen" / "Install app"). It deliberately does no caching: every request goes to the network,
// so a new deploy is always picked up and signed-in data is never served stale.
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('fetch', () => { /* network only */ });
