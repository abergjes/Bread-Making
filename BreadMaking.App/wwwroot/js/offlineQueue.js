(() => {
    const DB_NAME = 'bread-offline-v1';
    const STORE   = 'queue';

    function openDb() {
        return new Promise((resolve, reject) => {
            const req = indexedDB.open(DB_NAME, 1);
            req.onupgradeneeded = e => e.target.result.createObjectStore(STORE, { autoIncrement: true });
            req.onsuccess  = e => resolve(e.target.result);
            req.onerror    = e => reject(e.target.error);
        });
    }

    async function enqueue(url, method, body) {
        const db    = await openDb();
        const tx    = db.transaction(STORE, 'readwrite');
        tx.objectStore(STORE).add({ url, method, body, ts: Date.now() });
        return new Promise((res, rej) => { tx.oncomplete = res; tx.onerror = rej; });
    }

    async function flush() {
        const db    = await openDb();
        const tx    = db.transaction(STORE, 'readwrite');
        const store = tx.objectStore(STORE);
        const items = await new Promise((res, rej) => {
            const req = store.getAll();
            req.onsuccess = e => res(e.target.result);
            req.onerror   = e => rej(e.target.error);
        });
        const keys  = await new Promise((res, rej) => {
            const req = store.getAllKeys();
            req.onsuccess = e => res(e.target.result);
            req.onerror   = e => rej(e.target.error);
        });

        for (let i = 0; i < items.length; i++) {
            const item = items[i];
            try {
                await fetch(item.url, {
                    method:  item.method,
                    body:    item.body ? JSON.stringify(item.body) : undefined,
                    headers: { 'Content-Type': 'application/json' }
                });
                store.delete(keys[i]);
            } catch {
                break; // still offline — stop replaying
            }
        }
    }

    window.addEventListener('online', () => flush().catch(console.warn));

    window.breadOffline = {
        enqueue,
        flush,
        isOnline: () => navigator.onLine
    };

    window.breadOfflineUtils = {
        isOnline: () => navigator.onLine,
        registerNetworkHandlers: (dotnetRef) => {
            window.addEventListener('online',  () => dotnetRef.invokeMethodAsync('OnBackOnline').catch(console.warn));
            window.addEventListener('offline', () => dotnetRef.invokeMethodAsync('OnGoOffline').catch(console.warn));
        }
    };
})();
