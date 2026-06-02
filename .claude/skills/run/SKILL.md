---
description: Launch the Bread-Making hosted Blazor WASM app (ASP.NET Core server + SQLite) and run the Playwright e2e suite.
---

# Run skill — Bread-Making App

## Prerequisites

- .NET 10 SDK (`dotnet --version`)
- SQLite DB already initialised (`bread-making.db` in `BreadMaking.App.Server/`). If missing: `dotnet ef database update --project BreadMaking.App.Server`
- For e2e tests: Node.js + local Playwright (`npm install` in repo root, already done)

## ⚠️ Critical: kill the old server before restarting

After any `dotnet build`, the Blazor WASM asset hashes change. If the old server process is still running it serves the stale `index.html` whose hash references no longer match the new files — Blazor fails to boot silently. Always kill first:

```bash
taskkill //F //IM BreadMaking.App.Server.exe 2>/dev/null; echo "stopped"
```

## Run

Start the server in the background (HTTP on port 5112):

```bash
cd "C:\Users\aw_va\RiderProjects\Bread-Making"
dotnet run --project BreadMaking.App.Server --launch-profile http > /tmp/server.log 2>&1 &
```

Wait for ready, then smoke-test:

```bash
until curl -s -o /dev/null -w "%{http_code}" http://localhost:5112/ | grep -q 200; do sleep 2; done
curl -s -o /dev/null -w "root:%{http_code} api:%{http_code}\n" \
    http://localhost:5112/ \
    http://localhost:5112/api/bakes
# → root:200 api:200
```

Server logs are at `/tmp/server.log`.

## Verify

```bash
curl -s http://localhost:5112/api/bakes | head -c 60
# → [{"id":...  (JSON array of past bakes, or [] if none)
```

## Run e2e tests

Requires the server to be up. Uses system Chrome (no download needed):

```bash
cd "C:\Users\aw_va\RiderProjects\Bread-Making"
node e2e-test.mjs
# → 38 passed, 0 failed
```

The suite covers: advisor flow, Start Bake, pause/resume, measurements, history list, grain comparison, CSV/JSON export, clone-bake.

## Stop

```bash
taskkill //F //IM BreadMaking.App.Server.exe 2>/dev/null
```

Or by port:

```bash
for pid in $(netstat -ano | grep ":5112.*LISTENING" | awk '{print $5}'); do
    taskkill //F //PID $pid 2>/dev/null
done
```

## Environment

| Setting | Value |
|---|---|
| HTTP port | `5112` (set in `BreadMaking.App.Server/Properties/launchSettings.json`) |
| DB file | `BreadMaking.App.Server/bread-making.db` (SQLite, gitignored) |
| Chrome path (e2e) | `C:\Program Files\Google\Chrome\Application\chrome.exe` |
