# Bread-Making App — Roadmap

## Current state (v7, June 2026)

Milestones M0–M9 are complete, including photo upload for bake outcomes (M9 extension). The app is a hosted Blazor WASM solution with a full ASP.NET Core backend, EF Core + SQLite persistence, live bake execution, measurements, visualisations, history, SignalR notifications, a three-tier overrun warning system with audible alerts, and a full bake outcome capture UI with photo upload. Milestones M10–M16 are planned — they address gaps identified by a senior baker assessment and extend the app from a capable bake tracker into a full data-driven baking companion.

## Vision

Transform the advisor into a full baking companion: the static timeline becomes a live, interactive session the baker runs during the bake, recording actual durations and measurements at each step. Over time that record becomes a queryable history that supports troubleshooting, grain comparison, and reproducibility — turning "happy accidents" into something repeatable.

The change is additive. The existing advisor flow is preserved unchanged; a **Start Bake** button bridges planning into execution.

---

## Architecture prerequisite — M0

Before any milestone work begins, the solution is converted from a standalone WASM project to a hosted Blazor WASM solution with an ASP.NET Core server project. This is a structural change, not a feature. It unblocks all persistence, timer-truth, and API work that follows. See ARCHITECTURE.md for the full picture.

---

## Milestones

| # | Name | Core scope | Unlocks | Status |
|---|------|-----------|---------|--------|
| M0 | Architecture lift | Add server + shared projects; hosted WASM; existing app still works | All | ✅ Complete |
| M1 | Data model | EF Core entities, SQLite, migration, seeded step defaults and measurement types | M2, M4 | ✅ Complete |
| M2 | Timer core | TimerService, API endpoints, state machine, server-derived elapsed | M3 | ✅ Complete |
| M3 | Live bake UI | `/bake/{id}` page, step cards, timer readout, ± steppers, Pause / Done | M4, M5 | ✅ Complete |
| M4 | Measurements | MeasurementService, entry sheet, chips on step card, range validation | M5, M6 | ✅ Complete |
| M5 | Visualisations | Live rise curve, planning Gantt, history run chart, grain comparison via ApexCharts.Blazor | M6 | ✅ Complete |
| M6 | History & comparison | `/history` list, grain comparison view, CSV/JSON export, clone-bake | — | ✅ Complete |
| M7 | Notifications (optional) | SignalR hub, 30-min fold reminders, bulk-50% push, cross-device alerts | — | ✅ Complete |
| M8 | Live bake UX enhancements | Duration/total time display; three-tier overrun visualisation; audible alerts with toggle | — | ✅ Complete |
| M9 | Outcome capture UI | BakeOutcomeDto, PUT /api/bakes/{id}/outcome, OutcomeSheet + OutcomeField components, chips + photo on live bake page | — | ✅ Complete |
| M10 | Formula & extended inputs | Persist hydration %, starter activity, flour weight, salt %, inoculation % on every bake | M15 | 📋 Planned |
| M11 | Per-step notes | Notes field on BakeStepLog; inline auto-save in StepCard; 📝 indicator on collapsed card | — | 📋 Planned |
| M12 | Ratings & tags | Overall score (1–5 stars), free-form tags, best-loaf flag on BakeOutcome | M15 | 📋 Planned |
| M13 | Starter journal | Starter + StarterFeedLog entities; /starter page; link bake to feed log entry | M15 | 📋 Planned |
| M14 | Recipe library | User-created recipes with baker's %-formula; save-from-advisor flow; IsUserDefined flag | — | 📋 Planned |
| M15 | Analytics & trends | Scatter correlations, personal bests, season trend, bake-to-bake diff view | — | 📋 Planned |
| M16 | PWA & offline | Service worker, offline step progression via IndexedDB queue, app manifest | — | 📋 Planned |

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

### M8 — Live bake UX enhancements

**Goal:** Surface timing problems immediately and audibly so the baker doesn't have to watch the screen.

Scope:
- **Duration display** — bake header shows "Planned Xh Ym" (sum of all step planned durations) in the meta line, and an "ELAPSED / TOTAL" counter below it that ticks every second while the bake is running and freezes once complete
- **Three-tier overrun visualisation** — applied across Running, Paused, and Completed step cards:
  - On-time: normal accent styling
  - Over `PlannedDurationMin`: orange border, orange progress bar, "+X min over" badge
  - Over `MaxDurationMin`: red border, red elapsed timer, "exceeds max" badge
- **Completed step colour-coding** — compact card actual-duration text: green (on-time), orange (over planned), red (over max); completed card border also shifts
- **Audible alerts** — `wwwroot/js/audio.js` via Web Audio API (no audio files); single C5 beep when a running step first crosses `PlannedDurationMin`; double A5 beep when it first crosses `MaxDurationMin`; each fires at most once per step per page session
- **Sound toggle** — 🔔/🔕 radio pill in the bake page header; silences future beeps without affecting visual indicators

**Success criteria:** A running step that exceeds its planned duration shows orange; continuing past the recipe maximum turns it red and fires a double beep. Pressing 🔕 stops future beeps. Completing an over-time step shows the actual duration in orange or red in the compact row.

**Implemented:** 2026-06-03

---

### M9 — Outcome capture UI

**Goal:** Let the baker record what the finished loaf looked like, felt like, and tasted like — closing the loop from execution back to history.

Scope:
- **`BakeOutcomeDto`** — new shared DTO (LoafHeightCm, OvenSpringPct, InternalTempC, WeightLossPct, CrumbOpenness, CrustScore, TasteScore)
- **`PUT /api/bakes/{id}/outcome`** — upsert endpoint; creates on first call, replaces on subsequent calls
- **`SaveOutcomeAsync`** in `BakeSessionService` — 1:1 upsert against the `BakeOutcome` entity
- **`OutcomeField.razor`** — reusable ± stepper row with null / set / clear states and sensible per-field defaults (loaf 10 cm, oven spring 20%, weight loss 12%, internal temp 95 °C, scores 5/10)
- **`OutcomeSheet.razor`** — bottom-sheet modal (mirrors MeasurementSheet pattern) with MEASUREMENTS and SCORES sections
- **`LiveBake.razor`** — Outcome section after Notes: outcome chips (one per set field) when recorded; empty-state hint + "Log outcome" button when not; button becomes "Edit" after first save and reopens the sheet pre-filled

**Success criteria:** Baker can log outcome after completing a bake; outcome chips appear on the live bake page; "✓" checkmark and metric summary visible in the history list; `PUT /api/bakes/{id}/outcome` returns 204 on create and update.

**Implemented:** 2026-06-03

---

### M10 — Formula & extended bake inputs

**Goal:** Every bake record captures the full formula used, not just the grain and method. Today `HydrationPercent`, `StarterActivity`, and flour weight are advisor inputs that are silently dropped when the bake is created — a baker cannot look back and see exactly what they baked.

Scope:
- Extend `Bake` entity: `HydrationPct`, `StarterActivity` (int, mirrors advisor enum), `TotalFlourGrams` (nullable), `SaltPct` (nullable), `InoculationPct` (nullable)
- Extend `StartBakeRequest` to carry these fields from the advisor
- Update `BakeSessionService.CreateFromInputsAsync` to persist them
- Update `BakeDto` and `BakeListItemDto` to expose them
- Live bake header: show formula summary line (e.g., "78% · 950 g flour · 2% salt")
- New EF migration: `AddFormulaFieldsToBake`

**Success criteria:** After starting a bake, `GET /api/bakes/{id}` returns `hydrationPct`, `totalFlourGrams`, `saltPct`, and `inoculationPct`. History list shows hydration % alongside grain name.

---

### M11 — Per-step notes

**Goal:** Let the baker pin observations to the exact step where they happened — "dough felt slack at pre-shape" is lost if written in the bake notes field.

Scope:
- Add `Notes` (string, nullable) to `BakeStepLog` entity
- New migration: `AddNotesToBakeStepLog`
- Extend `BakeStepLogDto` with `Notes`
- `StepCard.razor` (expanded state): inline `<textarea>` with debounced auto-save (mirrors bake-level notes pattern exactly)
- New API endpoint: `PATCH /api/steplogs/{id}/notes`
- Collapsed completed step card: 📝 chip indicator when `Notes` is set

**Success criteria:** Type a note in an expanded step card; it auto-saves; on page refresh the note is present; collapsed card shows 📝.

---

### M12 — Bake ratings, tags & best-loaf flag

**Goal:** Make the history list filterable and let the baker bookmark their proudest loaves.

Scope:
- Add to `BakeOutcome`: `OverallScore` (int?, 1–5), `Tags` (string?, comma-separated), `IsBestLoaf` (bool)
- New migration: `AddRatingAndTagsToBakeOutcome`
- `OutcomeSheet.razor`: star-rating row (5 tappable ★ / ☆ icons) and tag chip input above the photo section
- `BakeOutcomeDto` and `BakeListItemDto`: expose `OverallScore`, `Tags`, `IsBestLoaf`
- History list: render star rating and tag pills on each bake card; add "⭐ Best loaves only" filter toggle alongside existing grain/method filters

**Success criteria:** Log a 5-star outcome with tag "gift"; it appears in the history list with stars and tag pill; "best loaves only" filter hides unrated bakes.

---

### M13 — Starter journal

**Goal:** Track the health of each sourdough starter over time and link it to the bakes it launches. Starter state is the single most important variable in sourdough timing and is currently invisible in the record.

Scope:
- New entities: `Starter` (Id, Name, HydrationPct, FlourBlend, CreatedAt, Notes), `StarterFeedLog` (Id, StarterId, FedAt, FlourGrams, WaterGrams, PrevStarterGrams, AmbientTempC, PeakHours, FloatTestPassed)
- `Bake` entity: add `StarterFeedLogId` (nullable FK to `StarterFeedLog`) — "which feed launched this bake"
- New migrations: `AddStarterJournal`, `AddStarterFeedLinkToBake`
- New API endpoints: `GET/POST /api/starters`, `POST /api/starters/{id}/feeds`, `GET /api/starters/{id}/feeds`
- New page `/starter`: list starters, log a feed entry (amounts, ambient temp), see peak-hours trend chart per starter
- Advisor start sheet: optional "Link starter feed" selector showing the 3 most recent feed log entries
- History list bake card: "fed X h ago" badge when a feed is linked
- `BakeDto`: expose `StarterFeedLog` summary (fed at, hours before bake)

**Success criteria:** Add a starter, log a feed, start a bake and link it to that feed. History shows "fed 4 h ago" on the bake card.

---

### M14 — User recipe library

**Goal:** Let the baker save their own formulas so they can restart a favourite recipe without re-entering everything from scratch each time.

Scope:
- Extend `Recipe` entity: `IsUserDefined` (bool, default false for all seeded records), `CreatedByLabel` (string?)
- New `RecipeFormula` entity: `Id`, `RecipeId`, `FlourWeightG`, `WaterPct`, `SaltPct`, `StarterPct`, `Notes`
- New migrations: `AddUserDefinedToRecipe`, `AddRecipeFormula`
- New API endpoints: `GET/POST/PUT/DELETE /api/recipes` (user-defined only; seeded recipes are read-only)
- Advisor UI: "Saved recipes" chip row above the grain selector; tapping one pre-fills all advisor inputs
- Recommendation panel: "Save as recipe" button — posts current inputs as a new user-defined recipe
- `IsUserDefined = false` recipes are never exposed via the recipe CRUD endpoints

**Success criteria:** Create a custom recipe from a recommendation; it appears in the "Saved recipes" row; selecting it pre-fills the advisor; deleting it removes it from the list.

---

### M15 — Analytics & trends

**Goal:** Surface the patterns hidden in the accumulated bake history — this is the payoff for all the data entry in M4, M9, M10–M13.

Scope:
- New API endpoint: `GET /api/analytics/correlations?metric=crumb|ovenspring|taste&factor=hydration|bulktime|kitchentemp`
  Returns `{ x, y, bakeId, date, grainName }[]` for scatter plotting
- New API endpoint: `GET /api/analytics/personal-bests` — best score per grain per outcome metric
- New page `/history/analytics`:
  - **Scatter chart** — any outcome metric (Y axis) vs any input factor (X axis); each dot is a bake; hover shows date + grain
  - **Best loaf gallery** — top-rated bake per grain with photo thumbnail (requires M12)
  - **Season trend** — monthly average oven spring / crumb openness as an area chart
- Extend `/history/compare` page: **bake diff** — select any two bakes from a dropdown; a table shows every input and outcome side-by-side with changed values highlighted

**Success criteria:** With 5+ bakes logged, the scatter chart shows a visible trend; personal bests list one bake per grain; bake diff highlights differences between two selected bakes.

---

### M16 — PWA & offline

**Goal:** The bake timer must survive a network drop. Kitchens frequently lose wifi, and a step completing while the server is unreachable should not lose state.

Scope:
- Wire up Blazor WASM service worker scaffolding (`service-worker.js` / `service-worker.published.js`) — already generated by the template, currently no-op
- Cache the active bake's API response (`/api/bakes/{id}`) for offline reads
- `LiveBake.razor`: offline banner ("Working offline — timer still running; changes sync when reconnected") when `navigator.onLine` is false
- New `wwwroot/js/offlineQueue.js` — queues `start`, `pause`, `complete` POST requests to IndexedDB when offline; flushes and replays in order on `navigator.online` event
- `manifest.json`: app name "Bread-Making", 192×192 and 512×512 icons, `display: standalone`, `start_url: /`
- One-time "Add to Home Screen" nudge (dismissible, stored in `localStorage`) via `navigator.getInstalledRelatedApps()`

**Success criteria:** Disable network in DevTools while a step is running; start the next step; re-enable network; confirm the step start POST was replayed and server state is correct.

---

## Out of scope for this roadmap

- Bluetooth probe or scale integration (Bluetooth Web API — possible future extension)
- Predictive time-to-rise estimation (requires sufficient logged history with M13 starter data)
- Multi-user / shared bake sessions
- Native mobile app (the app is a PWA after M16; keep WASM, focus on responsive CSS)
- Dark mode (CSS custom properties are already used throughout; a dark theme can be added as a low-priority polish task after M16)
