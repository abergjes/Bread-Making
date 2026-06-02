# Bread-Making App — Roadmap

## Current state (v3, June 2026)

The app is a pure Blazor WebAssembly SPA running entirely in the browser. There is no server and no persistence. The advisor recommends a method (autolyse / fermentolyse), a rest duration, and a static baking timeline based on grain selection and kitchen temperature. All state is ephemeral — closing or refreshing the tab loses it.

## Vision

Transform the advisor into a full baking companion: the static timeline becomes a live, interactive session the baker runs during the bake, recording actual durations and measurements at each step. Over time that record becomes a queryable history that supports troubleshooting, grain comparison, and reproducibility — turning "happy accidents" into something repeatable.

The change is additive. The existing advisor flow is preserved unchanged; a **Start Bake** button bridges planning into execution.

---

## Architecture prerequisite — M0

Before any milestone work begins, the solution is converted from a standalone WASM project to a hosted Blazor WASM solution with an ASP.NET Core server project. This is a structural change, not a feature. It unblocks all persistence, timer-truth, and API work that follows. See ARCHITECTURE.md for the full picture.

---

## Milestones

| # | Name | Core scope | Unlocks |
|---|------|-----------|---------|
| M0 | Architecture lift | Add server + shared projects; hosted WASM; existing app still works | All |
| M1 | Data model | EF Core entities, SQLite, migration, seeded step defaults and measurement types | M2, M4 |
| M2 | Timer core | TimerService, API endpoints, state machine, server-derived elapsed | M3 |
| M3 | Live bake UI | `/bake/{id}` page, step cards, timer readout, ± steppers, Pause / Done | M4, M5 |
| M4 | Measurements | MeasurementService, entry sheet, chips on step card, range validation | M5, M6 |
| M5 | Visualisations | Live rise curve, planning Gantt, history run chart via ApexCharts.Blazor | M6 |
| M6 | History & comparison | Outcome capture, `/history` list, grain comparison view, CSV/JSON export | — |
| M7 | Notifications (optional) | SignalR reminders, cross-device step alerts | — |

---

### M0 — Architecture lift

**Goal:** Convert the single-project WASM app to a hosted WASM solution. All existing functionality must still work.

Scope:
- Create `BreadMaking.App.Server` (ASP.NET Core — hosts WASM + owns the API and database)
- Create `BreadMaking.App.Shared` (plain class library — DTOs and shared enums, no EF or Blazor references)
- Rename / adjust the existing project to `BreadMaking.App.Client` (Blazor WASM)
- Register `BreadAdvisorService` on client unchanged
- Configure `HttpClient` factory on client pointing at server origin
- Server `Program.cs`: `UseBlazorFrameworkFiles`, `UseStaticFiles`, `MapFallbackToFile("index.html")`

**Success criteria:** `dotnet run --project BreadMaking.App.Server` serves the existing advisor app end-to-end with no regressions.

---

### M1 — Data model & migration

**Goal:** Schema in place; seeded defaults match the step timeline shown in the planning Gantt.

Scope:
- EF Core entities: `GrainProfile`, `Recipe`, `RecipeStep`, `Bake`, `BakeStepLog`, `MeasurementType`, `Measurement`, `BakeOutcome`
- `AppDbContext` with SQLite (development); provider-swappable via connection string
- Migration: `InitialSchema`
- Seed `MeasurementType` (5 built-in types: Dough temp, Aliquot rise, pH, TTA, Internal temp)
- Seed `RecipeStep` defaults for wheat/autolyse and wheat/fermentolyse (12 steps each) — see DEVELOPMENT.md for the full table

**Note:** The existing client-side `GrainProfile` record type (in `Models/`) is a separate, in-memory type that drives the advisor. It coexists with the new server-side EF entity of the same name; they have different shapes and purposes.

**Success criteria:** `dotnet ef database update` produces a working SQLite database with all tables and seed data queryable.

---

### M2 — Timer core

**Goal:** Steps can start, pause, and complete reliably across tab refreshes, app restarts, and overnight proofs.

Scope:
- `ITimerService` + `TimerService` (Start, Pause, Complete, AdjustPlanned)
- `BakeSessionService` (creates a `Bake` from advisor inputs, generates step logs)
- API endpoints: `POST /api/steplogs/{id}/start|pause|complete`, `PATCH /api/steplogs/{id}/duration`
- Elapsed is always derived as `(EndedAt ?? now) − StartedAt` — never a stored ticking counter
- Unit tests: pause-then-adjust leaves `StartedAt`/`EndedAt` untouched; resume correctly preserves accumulated elapsed

**Success criteria:** Start a step, close the browser tab, reopen — elapsed time is still correct.

---

### M3 — Live bake UI

**Goal:** The recommendation screen gains a Start Bake button; `/bake/{id}` has fully interactive step cards.

Scope:
- **Start Bake** button on `RecommendationPanel.razor` — POSTs `BreadInputs` to `POST /api/bakes`, navigates to `/bake/{id}` on 201 response
- New `LiveBake.razor` page (`/bake/{id}`) — loads bake state on init
- New `StepCard.razor` — per step: status pill, large elapsed readout, progress bar, ± duration stepper, Pause / Done buttons
- Client 1-second timer: Blazor `System.Threading.Timer` fires `StateHasChanged()`; elapsed computed from server-provided `StartedAt`
- Overrun: soft amber/terracotta warning when elapsed > planned; not a hard stop
- Planning Gantt shown before first step is started; collapses once the bake is running

**Success criteria:** A complete bake session (all steps started and completed) runs without error. Refresh mid-step shows correct elapsed.

---

### M4 — Measurements

**Goal:** Manual readings are captured, validated, and permanently pinned to the step they were taken at.

Scope:
- `IMeasurementService` + `MeasurementService` (Add, list per step, validate against `MeasurementType.MinValid/MaxValid`)
- API endpoint: `POST /api/steplogs/{id}/measurements`
- `MeasurementSheet.razor` — bottom-sheet modal: type selector chips (pre-selected by step phase), large numeric value with ± steppers, expected-range hint, Save / Cancel
- `+ Add` chip on `StepCard.razor` opens the sheet; saved measurements appear as chips ordered by `RecordedAt`
- `RecordedAt` stamped server-side — the client never sets it

**Success criteria:** Add a measurement during bulk fermentation; it appears as a chip with the correct timestamp. A pH of 14 is rejected with the expected range displayed.

---

### M5 — Visualisations

**Goal:** Three charts from the research plan rendered from real data.

Scope:
- Add `ApexCharts.Blazor` to the client project
- **Live rise curve** — inside the bulk step while running: baker's aliquot rise % readings plotted over elapsed time, overlaid on three reference curves (22 / 24 / 26 °C), with the 50–75% shaping target band shaded
- **Planning Gantt** — horizontal bar chart on the live bake page before any step starts: each step with its default duration (solid bar) and adjustable range (thin line)
- **Run chart** — on `/history`: a chosen outcome metric (e.g., loaf height) across successive bakes, with a dashed target reference line

**Success criteria:** Each chart renders correctly with seeded / demo data before real bakes are logged.

---

### M6 — History & comparison

**Goal:** A complete bake record is queryable and comparable across grains.

Scope:
- `BakeOutcome` capture form — launched from the final step (Cool on rack) or a "Log outcome" button on the bake page
- `/history` page — paginated list: grain, method, date, key outcome metrics, "Start similar bake" shortcut
- `/history/compare` — grain comparison view: oven spring, crumb openness, and time-to-50%-rise as side-by-side bar charts, computed from all logged bakes
- Clone-bake: copy a previous bake's inputs, flag the single field you intend to change
- CSV and JSON export of a single bake or full history

**Success criteria:** Three bakes logged with different grains; comparison view shows populated side-by-side bars.

---

### M7 — Notifications (optional, Phase 2)

Scope:
- SignalR hub on the server
- "Start your folds" reminder every 30 min during bulk fermentation
- "Bulk hit 50%" push when the latest aliquot measurement crosses the threshold
- Cross-device: a second browser/device can join an active bake session and receive the same alerts

This milestone is purely additive — no existing components need to change to support it.

---

## Out of scope for this roadmap

- Bluetooth probe or scale integration (spec section C8 future extension)
- Predictive time-to-rise estimation (requires sufficient logged history — defer to post-M6)
- Multi-user / shared bake sessions
- Native mobile app (the app is already a PWA; keep WASM, focus on responsive CSS)
