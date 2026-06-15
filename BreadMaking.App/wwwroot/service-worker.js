const CACHE = 'bake-cache-v1';

self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', e => e.waitUntil(self.clients.claim()));

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;
    if (!event.request.url.includes('/api/bakes/')) return;

    event.respondWith(
        caches.open(CACHE).then(cache =>
            fetch(event.request)
                .then(resp => {
                    if (resp.ok) cache.put(event.request, resp.clone());
                    return resp;
                })
                .catch(() =>
                    cache.match(event.request).then(cached =>
                        cached ?? new Response(JSON.stringify({ error: 'offline' }), {
                            status: 503,
                            headers: { 'Content-Type': 'application/json' }
                        })
                    )
                )
        )
    );
});
