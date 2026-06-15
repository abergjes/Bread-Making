# Bread-Making App — Roadmap

## Current state (v8, June 2026)

All milestones M0–M23 are complete. The app is a hosted Blazor WASM solution with a full ASP.NET Core backend, EF Core + SQLite persistence, live bake execution, per-step notes, measurements, visualisations, history with star ratings and tags, SignalR notifications, audible alerts, outcome capture with photo upload, formula fields on every bake, a starter journal, a user recipe library with save-from-advisor flow, a six-tab baker's calculators page, a food safety reference page with StorageAdvisor chip, an equipment & kit guide with preheat calculator, steam-method ranking, tiered buying guide, and scoring reference, full steamed-bread support (Mantou/Baozi), enriched dough / Shokupan support with butter/egg/sugar/milk formula fields, Pullman-tin flag, Tangzhong live-preview panel wired to the roux calculator, 11-step seeded timeline, enriched troubleshooting, and storage advice, and PWA/offline support with service worker caching of active-bake API responses, IndexedDB step-action queue with auto-replay on reconnect, web-app manifest for Add-to-Home-Screen, and dismissible install nudge.

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
| M10 | Formula & extended inputs | Persist hydration %, starter activity, flour weight, salt %, inoculation % on every bake | M15 | ✅ Complete |
| M11 | Per-step notes | Notes field on BakeStepLog; inline auto-save in StepCard; 📝 indicator on collapsed card | — | ✅ Complete |
| M12 | Ratings & tags | Overall score (1–5 stars), free-form tags, best-loaf flag on BakeOutcome | M15 | ✅ Complete |
| M13 | Starter journal | Starter + StarterFeedLog entities; /starter page; link bake to feed log entry | M15 | ✅ Complete |
| M14 | Recipe library | User-created recipes with baker's %-formula; save-from-advisor flow; IsUserDefined flag | — | ✅ Complete |
| M15 | Analytics & trends | Scatter correlations, personal bests, season trend, bake-to-bake diff view | — | ✅ Done |
| M16 | PWA & offline | Service worker, offline step progression via IndexedDB queue, app manifest | — | ✅ Done |
| M17 | Grain encyclopedia | FlavorNotes / NutritionHighlights / UsageNotes / HistoricalOrigin on GrainProfile; 17 grain seeds; profile cards on comparison page | M15 | ✅ Done |
| M18 | Crumb reading & troubleshooting | CrumbNotes on BakeOutcome; OutcomeSheet textarea; proofing-result enum; history card excerpt | M9, M15 | ✅ Done |
| M19 | Baker's calculators | `/calculators` page: baker's-% scaling, batch scaling with yield/loss, DDT water-temperature, levain split & true hydration, cost-per-loaf, tangzhong/yudane roux fold — all from baker's guide §48 + §54 | M14 | ✅ Done |
| M20 | Food safety & shelf life | Temperature ladder reference; rope/mould fault cards; storage recommendations on bake outcome (§49) | — | ✅ Complete |
| M21 | Equipment & kit guide | `/kit` reference page: tiered buying guide, steam-method comparison, preheat calculator, scoring tips (§50 + §51) | — | ✅ Done |
| M22 | Steamed breads | Mantou/baozi support: new `BakeMethod.Steamed`, steaming step defaults, low-protein flour guidance, steam-time advisor (§52) | M1 | ✅ Done |
| M23 | Enriched dough & milk breads | Shokupan/hokkaido support: enriched formula fields (butter %, egg %, sugar %), pullman-tin flag, milk-bread timeline, integrates roux from M19 (§53) | M19 | ✅ Done |

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
- New entities: `Starter` (Id, Name, HydrationPct, FlourBlend, CreatedAt, Notes), `StarterFeedLog` (Id, StarterId, FedAt, FlourGrams, WaterGrams, PrevStarterGrams, AmbientTempC, PeakHours, FloatTestPassed, **FeedRatio**)
- `FeedRatio` — nullable string, e.g. `"1:2:2"` (starter:flour:water by weight); used to compute peak estimates and display a ratio history chip row on the `/starter` page
- `Bake` entity: add `StarterFeedLogId` (nullable FK to `StarterFeedLog`) — "which feed launched this bake"
- New migrations: `AddStarterJournal`, `AddStarterFeedLinkToBake`
- New API endpoints: `GET/POST /api/starters`, `POST /api/starters/{id}/feeds`, `GET /api/starters/{id}/feeds`
- New page `/starter`: list starters; log a feed entry (amounts, ambient temp, feed ratio); see peak-hours trend chart per starter; health indicator chips (🟢 Active / 🟡 Hungry / 🔴 Discard) derived from `Notes` and feed age
- Advisor start sheet: optional "Link starter feed" selector showing the 3 most recent feed log entries
- History list bake card: "fed X h ago" badge when a feed is linked
- `BakeDto`: expose `StarterFeedLog` summary (fed at, hours before bake)

**Success criteria:** Add a starter, log a feed with ratio `"1:2:2"`, start a bake and link it to that feed. History shows "fed 4 h ago" on the bake card. `/starter` page shows the feed ratio chip and the peak-hours chart.

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

### M17 — Grain encyclopedia

**Goal:** Surface the character of each grain — taste, nutrition, history — so the baker can choose not just by gluten budget but by flavour intent.

Scope:
- Add `FlavorNotes`, `NutritionHighlights`, `UsageNotes`, `HistoricalOrigin` (all `string?`) to `GrainProfile` entity
- New migration: `AddEncyclopediaFieldsToGrainProfile`
- Seed all fields from baker's guide §15 for the existing 6 ancient grains + wheat, and add seeded `GrainProfile` rows for 9 further grains: Rye, Barley, Durum/Semolina, Triticale, Oat, Buckwheat, Amaranth, Quinoa, Millet
- Grain comparison page (`/history/compare`): tapping a grain bar opens a compact profile card showing flavour note, nutrition highlights, usage, and historical origin
- Grain selection chips in the advisor: long-press or hover reveals a tooltip with the flavour note

**Success criteria:** Tapping "Emmer" on the comparison page shows its flavour note, nutrition highlights, and historical origin. All 15 grains have non-null encyclopedia fields in the database.

---

### M18 — Crumb reading & troubleshooting

**Goal:** Close the feedback loop: help the baker diagnose what went wrong (or right) from the finished loaf, and record that diagnosis against the bake.

Scope:
- Add `CrumbNotes` (`string?`) to `BakeOutcome` entity
- New migration: `AddCrumbNotesToBakeOutcome`
- `OutcomeSheet.razor`: add CRUMB NOTES textarea below the SCORES section; auto-saves on blur; 📋 chip appears on history card when set
- New API endpoint or extend existing: `PUT /api/bakes/{id}/outcome` already handles the upsert — include `CrumbNotes` in the DTO and `SaveOutcomeAsync`
- Extend `BakeListItemDto` to expose `CrumbNotes` excerpt (first 80 characters) for history card preview
- `/history/analytics` (M15): add "poke-test" proofing column to the bake diff table — under-proofed / properly proofed / over-proofed — derived from baker notes or a new `ProofingResult` enum on `BakeOutcome`

**Success criteria:** Log crumb notes on a bake; notes appear on the history card (truncated) and in full on the bake detail view.

---

### M19 — Baker's calculators

**Goal:** Give the baker a set of purpose-built numeric tools derived directly from baker's guide §48 and §54. These are stateless, server-side pure-math endpoints that need no database — they convert inputs to outputs and return results. All six calculators live on a single `/calculators` page with a tabbed or accordion layout so the baker can jump between them.

Scope:

**Calculator 1 — Baker's % scaling (§48.1)**
- Input: target dough weight (g) + a formula table (ingredient name, baker's %)
- Output: grams of each ingredient; total formula % as a cross-check
- The client ships a pre-filled wheat/sourdough formula that the baker can edit

**Calculator 2 — Batch scaling with yield & loss (§48.2)**
- Input: desired loaf count, target baked weight (g), bake-loss % (default 12%), scaling-loss % (default 2%)
- Output: required batch-dough weight → then feeds Calculator 1 to split into ingredients
- Bake-loss and scaling-loss are editable fields with informational hints

**Calculator 3 — DDT water temperature (§48.3)**
- Input: desired dough temperature (DDT), flour temp, room temp, friction factor (preset by mix method: hand-folds 2 °C, hand-knead 3 °C, stand mixer 10 °C, spiral mixer 14 °C, intensive mixer 24 °C), optional preferment temp
- Output: required water temperature in °C
- Friction factor shown as a radio selector (method → °C), with a "custom" option for bakers who have measured their own
- Friction-factor presets sourced from baker's guide §50.4

**Calculator 4 — Levain split & true hydration (§48.4)**
- Input: total flour (g), target overall hydration %, levain weight (g), levain hydration % (default 100%)
- Output: levain flour, levain water (the split), final-dough flour, final-dough water; overall hydration as a check
- Supports stiff levains (e.g. 50%) and liquid levains (100%)

**Calculator 5 — Cost per loaf (§48.5)**
- Input: batch dough (from Calculator 1/2), ingredient price per 100 g for each, energy cost (£), labour (£), packaging (£), overhead (£), number of saleable loaves
- Output: total cost and cost per loaf, with a breakdown card (ingredients / overhead / labour)
- All cost fields default to zero so the baker only fills what they track

**Calculator 6 — Water-roux fold (§54.3, tangzhong / yudane)**
- Input: total flour (g), target hydration %, roux type (Tangzhong 1:5 or Yudane 1:1), roux flour share % (default 6%)
- Output: roux flour (g), roux liquid (g), remaining dough flour (g), remaining dough liquid (g); totals check confirms formula hydration is preserved
- A note explains each method: Tangzhong is cooked to ~65 °C; Yudane is scalded with boiling water and rested

API endpoints (all `POST`, all return 200 with the result DTO; no DB access):
```
POST /api/calculators/scale          ScaleRequest  → ScaleResult
POST /api/calculators/batch          BatchRequest  → BatchResult + ScaleResult
POST /api/calculators/ddt            DdtRequest    → DdtResult
POST /api/calculators/hydration      HydrationRequest → HydrationResult
POST /api/calculators/cost           CostRequest   → CostResult
POST /api/calculators/roux           RouxRequest   → RouxResult
```

All request/result types live in `BreadMaking.App.Shared/Dtos/Calculators/`.

**Success criteria:** Navigate to `/calculators`; select the DDT tab; enter kitchen temp 22 °C, flour temp 20 °C, method "hand-folds"; result shows ~31 °C water. Select the roux tab; enter 500 g flour, 70% hydration, Tangzhong 6%; result shows roux flour 30 g, roux liquid 150 g, dough flour 470 g, dough liquid 200 g with totals preserved.

---

---

### M20 — Food safety, shelf life & storage (§49)

**Goal:** Surface the baker's guide §49 safety rules at the right moment — when a bake ends and the baker is deciding how to store or share the loaf.

Scope:
- **StorageAdvisor component** — appears in the Outcome section of `LiveBake.razor` once a bake is marked complete; shows: recommended storage medium, expected shelf life, and one-line rationale based on `BakeMethod` and `StarterActivity`
  - Sourdough (any method): room-temperature cloth/paper, 3–5 days; freezer up to 3 months
  - Enriched / milk bread (M23): wrapped, 4–6 days room temp (higher fat slows staling)
  - Steamed bread (M22): room temp 1–2 days, fridge 4–5 days (no crust to protect)
- **Fault reference cards** — three compact cards in a `/safety` route (or collapsible panel on `/history`):
  - Rope: symptoms (stringy centre, melon/pineapple smell), cause, prevention, disposal
  - Mould: never cut off, discard whole loaf; surface moisture control
  - Staling: counter-intuitive fridge rule, freezer guidance, one-time refreshing
- **Temperature ladder graphic** — a static SVG/HTML element (no chart library needed) showing the danger zone (5–60 °C), crumb-set band, and crust-formation zone; referenced from the StorageAdvisor tooltip
- No new EF entities required — all logic is pure client-side or minimal server logic

**Success criteria:** Complete a bake; the outcome section shows a storage chip ("Room temp · 3–5 days") with a tap-to-expand fault card. The `/safety` page (or panel) renders all three fault cards with correct symptoms and fixes.

---

### M21 — Equipment & kit guide (§50 + §51)

**Goal:** Give the baker a tiered, opinionated reference for buying and using equipment — anchored to the same guide sections that inform the DDT calculator and step timings.

Scope:
- **`/kit` page** — single-page reference, no new API or DB needed
  - **Oven & preheat calculator** — input: oven type (conventional/convection/gas), surface (steel/stone/tray); output: recommended preheat time (§50.1–50.2). Stateless; client-side math.
  - **Steam method selector** — radio group (Dutch oven / combo cooker / lava rocks / spray / steam pan); shows effectiveness rating and protocol note for each (§50.3)
  - **Tiered buying guide** — three collapsible tiers (Starter / Serious / Semi-pro) with items, why-buy rationale, and skip-if note (§51.5)
  - **Measuring priority ranking** — a ranked list (scale → probe → IR gun → pH meter) with acceptable ranges for each (§51.1)
  - **Scoring guide** — blade angle (30–45° ear vs 90° split) diagram and lame note (§51.3)
- Nav link: add **Kit** button to History page header alongside Starters and Calculators
- All content is static (seeded from the guide); no DB access

**Success criteria:** Navigate to `/kit`; the preheat calculator returns 60 min for a conventional oven with baking steel. The tiered buying guide shows three distinct tiers. The steam method selector highlights Dutch oven as top-rated.

---

### M22 — Steamed breads — Mantou & Baozi (§52)

**Goal:** Extend the advisor and bake tracker to cover the steamed-bread tradition — an entirely different heat path (no crust, no Maillard) with its own flour choice, timing, and troubleshooting.

Scope:
- **New `BakeMethod.Steamed`** enum value and advisor branch
- **New `GrainProfile` + `Recipe` rows** for low-protein white wheat (9–11%) and Mantou/Baozi variants
- **New `RecipeStep` defaults** for the steamed method:

  | Order | Name | Default (min) | Phase |
  |-------|------|--------------|-------|
  | 1 | Mix flour + water | 10 | Mix |
  | 2 | Bulk (until doubled) | 60 | Bulk |
  | 3 | Knock back + portion | 10 | Shape |
  | 4 | Final shape | 15 | Shape |
  | 5 | Final proof | 20 | Proof |
  | 6 | Steam | 15 | Bake |
  | 7 | Rest in steamer (lid off) | 3 | Bake |
  | 8 | Cool on rack | 15 | Cool |

- **Steam-time advisor** — a note on the Live Bake page when `BakeMethod == Steamed`: "Vigorous simmer throughout; lid lined to prevent condensation drips; do not open lid during first 10 min"
- **Low-protein formula advisor branch** in `BreadAdvisorService` — if grain is low-protein white wheat + method is Steamed, suppress autolyse/fermentolyse radio and show the shorter Mantou timeline
- **Migration**: `AddSteamedBreadSeeds`
- **Troubleshooting** for Steamed bakes in the outcome flow: wrinkles (over-proof), density (under-proof or wrong flour), yellow tinge (too much yeast), rough skin (steam too vigorous)

**Dependencies:** M1 (recipe/step data model already in place)

**Success criteria:** Choose "Steamed" in the advisor; receive a Mantou timeline; start the bake; the Steam step shows the lid-protocol note. The `/history` list labels the bake as Steamed. Outcome troubleshooting offers steam-specific fault options.

---

### M23 — Enriched dough & milk breads — Shokupan (§53)

**Goal:** Support enriched doughs (milk, butter, egg, sugar) culminating in the Japanese shokupan workflow — uses the roux calculation already built in M19 and the enriched-formula concept from §53.

Scope:
- **Enriched formula fields** on `Bake` entity: `ButterPct` (nullable), `EggPct` (nullable), `SugarPct` (nullable), `MilkPct` (nullable), `MilkPowderPct` (nullable), `IsPullmanTin` (bool, default false)
- **Migration**: `AddEnrichedFormulaFieldsToBake`
- **New `BakeMethod.Enriched`** enum value; new seeded `Recipe` for Shokupan (tangzhong variant):

  | Order | Name | Default (min) | Phase |
  |-------|------|--------------|-------|
  | 1 | Prepare Tangzhong | 10 | Mix |
  | 2 | Cool Tangzhong | 30 | Rest |
  | 3 | Mix dough (autolyse) | 15 | Mix |
  | 4 | Add butter (window-pane) | 15 | Mix |
  | 5 | Bulk (until doubled) | 60 | Bulk |
  | 6 | Divide + pre-shape | 10 | Shape |
  | 7 | Bench rest | 15 | Shape |
  | 8 | Final shape + tin | 15 | Shape |
  | 9 | Final proof (to 80–90% tin height) | 60 | Proof |
  | 10 | Bake (lidded pullman: 190 °C) | 30 | Bake |
  | 11 | Cool on rack | 60 | Cool |

- **Advisor UI additions**: when method is Enriched, show enrichment inputs (butter %, egg %, sugar %); a "Use Tangzhong?" toggle pre-populates the roux split from M19 calculator
- **Formula summary line** on Live Bake header: extend existing `live-bake-formula` to show butter/egg/sugar when set
- **Roux integration**: when `IsPullmanTin` is true, the step notes for the Tangzhong step pull the roux-fold result from M19 and show the pre-computed flour/liquid split
- **Outcome troubleshooting for Enriched bakes**: dense crumb (mix too short / butter added too early), collapses after bake (over-proof), stales fast (insufficient roux or low hydration)

**Dependencies:** M19 (roux arithmetic already implemented)

**Success criteria:** Start an Enriched/Shokupan bake with butter 10%, sugar 10%, pullman tin; formula summary shows these values; step 1 shows the Tangzhong quantities from the roux calculator. History card labels the bake "Enriched". Outcome sheet offers enriched troubleshooting options.

---

## Milestone priority order

Ordered by value/effort ratio — implement top-to-bottom where possible:

| Priority | Milestone | Rationale |
|----------|-----------|-----------|
| 1 | **M11** — Per-step notes | Smallest scope; highest daily-use value for active bakers |
| 2 | **M12** — Ratings & tags | Enriches history immediately; unlocks M15 analytics |
| 3 | **M17** — Grain encyclopedia | Content-only seeding; FlavorNotes fields already on the entity plan |
| 4 | **M18** — Crumb reading | One DB field + textarea; closes the feedback loop after every bake |
| 5 | **M20** — Food safety & storage | No new DB entities; immediate safety value after M19 calculators |
| 6 | **M14** — Recipe library | Moderate scope; save-from-advisor flow the most-requested feature |
| 7 | **M15** — Analytics & trends | Needs M12 star data for full value; large but high payoff |
| 8 | **M21** — Equipment guide | Static content page; low effort; natural companion to M19 calculators |
| 9 | **M22** — Steamed breads | New bread paradigm; significant but self-contained scope |
| 10 | **M23** — Enriched dough | Builds on M19 roux; requires enriched-formula entity changes |
| 11 | **M16** — PWA & offline | Infrastructure; high effort; defer until core feature set is stable |

---

## Out of scope for this roadmap

- Bluetooth probe or scale integration (Bluetooth Web API — possible future extension)
- Predictive time-to-rise estimation (requires sufficient logged history with M13 starter data)
- Multi-user / shared bake sessions
- Native mobile app (the app is a PWA after M16; keep WASM, focus on responsive CSS)
- Dark mode (CSS custom properties are already used throughout; a dark theme can be added as a low-priority polish task after M16)
- Cold retard fermentolyse as a first-class `BakeMethod` — the current `BakeMethod.Other` and `AmbientTempC` capture sufficient data; a dedicated UI is deferred beyond M18
