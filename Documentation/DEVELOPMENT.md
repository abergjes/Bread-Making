# Development guide

This is the implementation reference for developers building the milestones described in ROADMAP.md. It assumes familiarity with .NET 10, Blazor WebAssembly, and EF Core.

---

## Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| .NET SDK | 10.0 | `dotnet --version` |
| EF Core CLI tools | latest | `dotnet tool install -g dotnet-ef` |
| Rider 2025+ or VS 2022+ | — | Blazor WASM hosted solution support |
| SQLite browser (optional) | — | DB Browser for SQLite, or JetBrains DataGrip |

---

## Solution structure (M10 + M13 + M19 complete, M11–M18 additions noted)

```
Bread-Making.sln
├── BreadMaking.App/                 (Blazor WASM — .NET 10)
│   ├── Models/
│   ├── Services/
│   ├── Components/
│   │   └── bake/                   ← StepCard, MeasurementSheet, PlanningGantt, charts
│   │                                  OutcomeSheet, OutcomeField
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── LiveBake.razor          ← /bake/{id}
│   │   ├── History.razor           ← /history
│   │   ├── GrainComparison.razor   ← /history/compare
│   │   ├── StarterJournal.razor    ← /starter                        (M13 ✅)
│   │   ├── Analytics.razor         ← /history/analytics              (M15, planned)
│   │   ├── Calculators.razor       ← /calculators                    (M19 ✅)
│   │   └── KitGuide.razor          ← /kit                            (M21, planned)
│   └── wwwroot/
│       ├── css/bakery.css
│       ├── js/audio.js             ← Web Audio API alerts (M8)
│       ├── js/offlineQueue.js      ← IndexedDB action queue          (M16, planned)
│       ├── service-worker.js       ← PWA offline cache               (M16, planned)
│       └── manifest.json           ← PWA manifest                    (M16, planned)
│
├── BreadMaking.App.Server/          (ASP.NET Core Web App — .NET 10)
│   ├── Api/
│   │   ├── BakeEndpoints.cs
│   │   ├── StepLogEndpoints.cs
│   │   ├── GrainEndpoints.cs
│   │   ├── StarterEndpoints.cs     ← (M13 ✅)
│   │   ├── RecipeEndpoints.cs      ← (M14, planned)
│   │   ├── AnalyticsEndpoints.cs   ← (M15, planned)
│   │   ├── CalculatorEndpoints.cs  ← (M19 ✅)
│   │   └── KitEndpoints.cs         ← (M21, planned)
│   ├── Services/
│   │   ├── TimerService.cs
│   │   ├── MeasurementService.cs
│   │   ├── BakeSessionService.cs
│   │   ├── StarterService.cs       ← (M13 ✅)
│   │   ├── CalculatorService.cs    ← (M19 ✅)
│   │   └── KitService.cs           ← preheat + steam advisor (M21, planned)
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── SeedData.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
└── BreadMaking.App.Shared/          (class library — net10.0)
    ├── Dtos/
    └── Enums/
```

**Running the full app:**
```
dotnet run --project BreadMaking.App.Server
```

**Running just the client (advisor flow, no persistence):**
```
dotnet run --project BreadMaking.App.Client
```

---

## Packages

### Client project additions

```xml
<PackageReference Include="ApexCharts.Blazor" Version="*" />
```

### Server project additions

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="*"
                  PrivateAssets="all" />
```

### Shared project

No external packages. `<TargetFramework>net10.0</TargetFramework>` only.

---

## Conventions


**DTOs vs. entities.** EF Core entity classes live in `Server/Data/` and are never referenced by the client project. All API responses use DTO types from `BreadMaking.App.Shared/Dtos/`.

**Dates.** All persistence and API values use `DateTimeOffset` UTC. The client converts to local time for display only (`dto.StartedAt.ToLocalTime()` or a JS `Intl.DateTimeFormat` call).

**Elapsed time.** Never stored as a counter. Always computed as `(EndedAt ?? DateTimeOffset.UtcNow) - StartedAt`. Unit tests assert this invariant.

**Measurement validation.** Validation lives in `MeasurementService` on the server. The client shows the expected range as an informational hint (values from the `MeasurementType` included in `BakeDto`) but does not enforce it — the server is authoritative.

**Step defaults by grain and method.** `BakeSessionService` looks up the matching `Recipe` row by grain name and `BakeMethod`. If no exact match exists (e.g., a new grain was added to the advisor without a seeded recipe), it falls back to the modern-wheat recipe for that method and logs a warning.

---

## M0 — Architecture lift

1. Create `BreadMaking.App.Shared` class library targeting `net10.0`. Add to solution.
2. Create `BreadMaking.App.Server` (ASP.NET Core Web App). Add to solution.
3. In `Server.csproj`: add project references to Client and Shared; add WASM hosting package (`Microsoft.AspNetCore.Components.WebAssembly.Server`).
4. In `Client.csproj`: add project reference to Shared; set `HttpClient` base address to server origin in `Program.cs`.
5. In `Server/Program.cs`:
   ```csharp
   app.UseBlazorFrameworkFiles();
   app.UseStaticFiles();
   app.MapFallbackToFile("index.html");
   ```
6. Verify: `dotnet run --project BreadMaking.App.Server` serves the existing advisor app with no regressions.

---

## M1 — Data model & migration

### Entity classes

Write the seven entities listed in ARCHITECTURE.md (full definitions in spec section C1) under `Server/Data/Entities/`.

### Seed data

Seed `MeasurementType` (5 records):

| Id | Name | Unit | Category | MinValid | MaxValid | DefaultForPhase |
|----|------|------|----------|---------|---------|----------------|
| 1 | Dough temp | °C | InProcess | 10 | 40 | Mix |
| 2 | Aliquot rise | % | InProcess | 0 | 200 | Bulk |
| 3 | pH | pH | InProcess | 3.0 | 7.0 | Bulk |
| 4 | TTA | mL | InProcess | 0 | 30 | Bulk |
| 5 | Internal temp | °C | Outcome | 80 | 110 | Bake |

> **Doneness targets** (baker's guide §18): lean sourdough loaves are done at ~96 °C internal (205 °F); enriched doughs (brioche, challah) at ~88 °C (185–195 °F). The seeded 80–110 °C range accommodates both. `DefaultForPhase = Bake` ensures the measurement sheet pre-selects this type on the covered and uncovered bake steps. Steam should be applied for the first 15–20 minutes of the bake (achieved by baking covered), then vented by removing the lid — this aligns with the existing "Bake covered" / "Bake uncovered" step split in the seed data. Lean loaves need at least 1–2 hours cooling on a rack before slicing; the seeded 120-minute "Cool on rack" default is correct.

Seed `RecipeStep` defaults for wheat/autolyse (RecipeId = 1):

| Order | Name | Default (min) | Min | Max | StepMin | Phase |
|-------|------|--------------|-----|-----|---------|-------|
| 1 | Mix flour + water | 5 | 3 | 15 | 5 | Mix |
| 2 | Autolyse rest | 30 | 20 | 60 | 5 | Rest |
| 3 | Add salt + starter | 5 | 3 | 15 | 5 | Mix |
| 4 | Bulk fermentation | 300 | 240 | 360 | 15 | Bulk |
| 5 | Pre-shape | 10 | 5 | 20 | 5 | Shape |
| 6 | Bench rest | 30 | 20 | 45 | 5 | Shape |
| 7 | Final shape | 10 | 5 | 20 | 5 | Shape |
| 8 | Cold proof | 960 | 480 | 1440 | 60 | Proof |
| 9 | Preheat + Dutch oven | 45 | 30 | 60 | 15 | Bake |
| 10 | Bake covered | 20 | 15 | 25 | 5 | Bake |
| 11 | Bake uncovered | 20 | 15 | 30 | 5 | Bake |
| 12 | Cool on rack | 120 | 60 | 180 | 30 | Cool |

For wheat/fermentolyse (RecipeId = 2): step 2 becomes "Fermentolyse rest" (starter added at mix time); step 3 becomes "Add salt only". Bulk default increases slightly (fermentolyse is typically faster once it gets going; adjust per the v3 spec tables).

Per-grain recipes (Einkorn, Emmer, Spelt, Kamut, Teff/Sorghum) are added in M1 as well, using the grain-handling parameters from the spec table (section A4). The bulk duration range, rest duration, and hydration note differ per grain.

**Additional grains (baker's guide §15 — grain encyclopedia):** Nine further grains are documented in the guide. Seed `GrainProfile` + `Recipe` rows for any that the advisor will support:

| Grain | Type | Rest | Hydration vs wheat | Key handling note |
|-------|------|------|--------------------|-------------------|
| Rye | Low-gluten | Skip or soaker | +10–20% | Paste-like; no kneading; mix only; ferment cooler/shorter (very high amylase) |
| Barley | Low-gluten blend | 20–30 min | Slight reduction | Use max 25% in blends; high beta-glucan lifts water absorption |
| Durum / Semolina | Strong | 30–60 min | Slight reduction | Inextensible (plastic) gluten; slow to hydrate; good for shaping |
| Triticale | Moderate | 20–30 min | Standard | Very high amylase — ferment cooler and shorter; 30–50% in blends |
| Oat | GF (binder needed) | Soaker 20–40 min | Standard | Up to 20–25% in GF blends; holds moisture well |
| Buckwheat | GF (binder needed) | Soaker 20–40 min | Standard | Bold flavour — use 10–30%; 100% loaf needs psyllium |
| Amaranth | GF (binder needed) | Soaker 20–40 min | Standard | Best up to 25% in GF blends; complete protein, high calcium |
| Quinoa | GF (binder needed) | Soaker 20–40 min | Standard | Strong flavour — light hand; 15–30% in GF blends |
| Millet | GF (binder needed) | Soaker 20–40 min | Standard | Mild, neutral; good GF base; binder recommended for raised loaves |

Add `FlavorNotes`, `NutritionHighlights`, `UsageNotes`, and `HistoricalOrigin` (all `string?`) to the `GrainProfile` entity in M1 to support the grain encyclopedia feature (M17). Seed values from baker's guide §15.

**Cold retard fermentolyse (baker's guide §16):** Section 7 covers room-temperature fermentolyse (16–26 °C). Baker's guide §16 documents a cold retard variant — taking the fermentolyse all the way down to 5 °C for 12–48+ hours. The yeast becomes dormant below ~8 °C, but lactic acid bacteria and flour enzymes continue, shifting acidity toward sharp acetic notes and building flavour precursors. Use `bake.AmbientTempC < 13` in `BakeSessionService` to detect cold-retard territory and apply the longer `PlannedDurationMin` values.

**Per-degree reference (baker's guide §16.5)** — modelled values, Q10 ≈ 2.2, anchored to measured points at 4 °C, 10 °C and 18 °C:

| Dough temp | Activity (rel. to 26 °C) | Time × | Acetic share | Acid character | Yeast gas |
|---|---|---|---|---|---|
| 5 °C | 19% | 5.2× | 75% | Sharp, vinegary | Near-dormant |
| 6 °C | 21% | 4.8× | 70% | Sharp, vinegary | Near-dormant |
| 7 °C | 22% | 4.5× | 65% | Sharp, vinegary | Near-dormant |
| 8 °C | 24% | 4.1× | 60% | Balanced, tangy | Near-dormant |
| 9 °C | 26% | 3.8× | 55% | Balanced, tangy | Slow |
| 10 °C | 28% | 3.5× | 50% | Balanced, tangy | Slow |
| 11 °C | 31% | 3.3× | 46% | Balanced, tangy | Slow |
| 12 °C | 33% | 3.0× | 42% | Mild-tangy | Slow |
| 13 °C | 36% | 2.8× | 39% | Mild-tangy | Slow |
| 14 °C | 39% | 2.6× | 35% | Mild-tangy | Slow |
| 15 °C | 42% | 2.4× | 31% | Mild-tangy | Slow |
| 16 °C | 45% | 2.2× | 28% | Mild-tangy | Moderate |
| 17 °C | 49% | 2.0× | 24% | Mild, yogurty | Moderate |
| 18 °C | 53% | 1.9× | 20% | Mild, yogurty | Moderate |
| 19 °C | 58% | 1.7× | 19% | Mild, yogurty | Moderate |
| 20 °C | 62% | 1.6× | 18% | Mild, yogurty | Moderate |
| 21 °C | 67% | 1.5× | 17% | Mild, yogurty | Moderate |
| 22 °C | 73% | 1.4× | 16% | Mild, yogurty | Brisk |
| 23 °C | 79% | 1.3× | 15% | Mild, yogurty | Brisk |
| 24 °C | 85% | 1.2× | 14% | Mild, yogurty | Brisk |
| 25 °C | 92% | 1.1× | 13% | Mild, yogurty | Brisk |
| 26 °C | 100% | 1.0× | 12% | Mild, yogurty | Brisk |

**Four practical working bands (baker's guide §16.6):**

| Band | Duration | Acid character |
|---|---|---|
| 5–7 °C (fridge retard) | 720–2880 min (12–48 h+) | Sharp, vinegary (acetic-dominant) |
| 8–12 °C (cold cellar) | 480–1440 min (8–24 h) | Balanced, tangy |
| 13–18 °C (cool room) | 90–180 min | Mild, lactic |
| 19–26 °C (warm room) | 22–130 min | Mild, yogurty |

Use the per-degree Time × multiplier relative to the 26 °C baseline (≈ 50 min at 22 °C → ~175 min at 13 °C) when computing `PlannedDurationMin` in `BakeSessionService`. Dough firmness increases in the cold, making it easier to handle and score.

### Migration

```
dotnet ef migrations add InitialSchema --project BreadMaking.App.Server
dotnet ef database update              --project BreadMaking.App.Server
```

Verify: open the generated SQLite file in a browser; confirm all tables and seed rows exist.

---

## M2 — Timer core

### TimerService

Implement `ITimerService` with four methods. Critical logic:

**Resume from Paused** — shifts `StartedAt` to preserve accumulated elapsed:
```csharp
var frozenElapsed = log.EndedAt!.Value - log.StartedAt!.Value;
log.StartedAt = DateTimeOffset.UtcNow - frozenElapsed;
log.EndedAt = null;
log.Status = StepStatus.Running;
```

**AdjustPlannedAsync** — clamps to `[RecipeStep.MinDurationMin, RecipeStep.MaxDurationMin]` and does not touch `StartedAt` or `EndedAt`.

### Unit tests

Write the following assertions (xUnit or NUnit):

- After `StartAsync`: `Status == Running`, `StartedAt != null`, `EndedAt == null`
- After `PauseAsync`: `Status == Paused`, `EndedAt != null`
- After `AdjustPlannedAsync` on a running step: `StartedAt` and `EndedAt` are unchanged
- After `StartAsync` on a paused step: `Elapsed(now)` is within 1 second of the elapsed time before pause
- After `AdjustPlannedAsync`: `PlannedDurationMin` is clamped to `[min, max]`

### API wiring

Map all five endpoints (`start`, `pause`, `complete`, `duration`, `measurements`) before moving to M3.

---

## M3 — Live bake UI

### Client timer

In `LiveBake.razor` (or the containing component), tick once per second:

```csharp
private System.Threading.Timer? _ticker;

protected override void OnAfterRender(bool firstRender)
{
    if (firstRender)
        _ticker = new System.Threading.Timer(
            _ => InvokeAsync(StateHasChanged), null,
            TimeSpan.Zero, TimeSpan.FromSeconds(1));
}

public void Dispose() => _ticker?.Dispose();
```

Elapsed displayed in `StepCard.razor`:

```csharp
private TimeSpan Elapsed =>
    Step.StartedAt is null ? TimeSpan.Zero
    : (Step.EndedAt ?? DateTimeOffset.UtcNow) - Step.StartedAt.Value;
```

### Start Bake flow

In `RecommendationPanel.razor`:
1. Inject `HttpClient`
2. On button click: `POST /api/bakes` with `new StartBakeRequest { Inputs = currentInputs }`
3. On 201 response: `NavigationManager.NavigateTo($"/bake/{response.Id}")`

### Overrun CSS

```css
.step-card.overrun .bar { background: var(--zone-hot); }
.step-card.overrun { border: 2px dashed var(--zone-hot); }
```

Toggle by computing `elapsed > TimeSpan.FromMinutes(dto.PlannedDurationMin)` in the component and applying the `overrun` CSS class.

> **Implemented (basic overrun):** 2026-06-03 — extended in M8 to a three-tier system; see M8 below.

---

## M4 — Measurements

### MeasurementService

```csharp
public async Task<Measurement> AddAsync(int stepLogId, AddMeasurementRequest req)
{
    var type = await _db.MeasurementTypes.FindAsync(req.MeasurementTypeId)
               ?? throw new KeyNotFoundException("Unknown MeasurementTypeId");

    if (type.MinValid.HasValue && req.Value < type.MinValid)
        throw new ValidationException($"Value below minimum {type.MinValid} {type.Unit}");
    if (type.MaxValid.HasValue && req.Value > type.MaxValid)
        throw new ValidationException($"Value above maximum {type.MaxValid} {type.Unit}");

    var m = new Measurement
    {
        BakeStepLogId = stepLogId,
        MeasurementTypeId = req.MeasurementTypeId,
        Value = req.Value,
        Unit = type.Unit,
        RecordedAt = DateTimeOffset.UtcNow   // ← never client-supplied
    };
    _db.Measurements.Add(m);
    await _db.SaveChangesAsync();
    return m;
}
```

### MeasurementSheet component

- Receives `stepLogId` and `List<MeasurementTypeDto>` (pre-loaded with the bake)
- Pre-selects the type whose `DefaultForPhase` matches the step's `Phase`
- ± increment values: temperature 0.5, rise % 1, pH 0.1, TTA 1
- On save: `POST /api/steplogs/{stepLogId}/measurements`; on success, emit `EventCallback<MeasurementDto> OnSaved`; parent adds the chip

### API error handling

On 422 Unprocessable Entity (out-of-range value), display the expected-range hint in terracotta tint rather than green. The error message from the server includes the range string; display it as-is.

---

## M5 — Visualisations

### ApexCharts setup

In `Client/Program.cs`:
```csharp
builder.Services.AddApexCharts();
```

In `Client/Pages/_Imports.razor` (or `_Imports.razor`):
```razor
@using ApexCharts
```

### Live rise curve

Rendered inside `StepCard.razor` for the Bulk step when `Status == Running`. Data series:
- Three computed reference curves (22 / 24 / 26 °C) using a logistic approximation: `rise(t) = 100 / (1 + e^(-k*(t - t0)))` where k and t0 are temperature-parameterised
- The baker's actual aliquot rise readings: `List<(TimeSpan elapsed, double rise)>` derived from `MeasurementDto.RecordedAt - stepLog.StartedAt`
- Shaded band between y=50 and y=75: use ApexCharts annotation region or a fill-between series

### Planning Gantt

Use ApexCharts `bar` chart in horizontal mode (`horizontal: true`). One data point per step:
- `x`: step name
- `y`: `[MinDurationMin, MaxDurationMin]` (range bar)
- `goals` annotation at `DefaultDurationMin` (marks the default)

### Run chart

`line` chart. X-axis: bake index (1, 2, 3…). Y-axis: the selected outcome metric. Annotations: horizontal dashed reference line at `targetValue`. The target can default to the mean of all logged values; let the baker override it later (M6 or post-M6 enhancement).

---

## M6 — History & comparison

### Export

CSV: serialize `BakeDto` as flat rows (one per step) with measurement columns. Use `System.Text.StringBuilder` or a simple CSV writer; no library needed for this surface.

JSON: return `JsonSerializer.Serialize(bakeDto)` with `JsonSerializerOptions { WriteIndented = true }`. Set `Content-Disposition: attachment; filename="bake-{id}.json"`.

### Clone-bake

`GET /api/bakes/{id}/inputs` — returns the `StartBakeRequest` used to create the bake. The client pre-fills the advisor form with these values and navigates to `Home`. No new page needed; re-use the existing advisor flow.

---

---

## M8 — Live bake UX enhancements

> **Implemented:** 2026-06-03

### Duration display

Added to `LiveBake.razor` header (`live-bake-title-group`):

- **Planned total** — appended to the existing meta `<p>` as `· Planned Xh Ym` (sum of all `PlannedDurationMin` values from step logs). Computed client-side; no new API call.
- **Elapsed / Total counter** — a new `<p class="live-bake-duration">` below the meta line:
  - Hidden before any step is started
  - Shows **ELAPSED Xh Ym** (ticking every second via the existing `_ticker`) once any step has started
  - Shows **TOTAL Xh Ym** (static) once `_bake.EndedAt` is set

`FormatDuration(TimeSpan)` helper: renders `"1h 05m"` for spans ≥ 1 hour, `"22m"` for shorter.

### Three-tier overrun visualisation

> **Implemented:** 2026-06-03

All logic is client-side in `StepCard.razor`. No new API fields needed — `PlannedDurationMin`, `MaxDurationMin`, `StartedAt`, `EndedAt`, and `Status` are already in `BakeStepLogDto`.

**Computed properties:**

```csharp
// Extended to include Paused (was Running-only in M3)
private bool IsOverrun =>
    (Step.Status == StepStatus.Running || Step.Status == StepStatus.Paused) &&
    Elapsed > TimeSpan.FromMinutes(Step.PlannedDurationMin);

private bool IsMaxOverrun =>
    (Step.Status == StepStatus.Running || Step.Status == StepStatus.Paused) &&
    Step.MaxDurationMin > 0 &&
    Elapsed > TimeSpan.FromMinutes(Step.MaxDurationMin);

private bool CompletedOverPlanned =>
    Step.Status == StepStatus.Completed &&
    ActualDuration > TimeSpan.FromMinutes(Step.PlannedDurationMin);

private bool CompletedOverMax =>
    Step.Status == StepStatus.Completed &&
    Step.MaxDurationMin > 0 &&
    ActualDuration > TimeSpan.FromMinutes(Step.MaxDurationMin);
```

**CSS classes applied via `CardClass`:**

| State | CSS modifier |
|---|---|
| Running / Paused — over planned | `overrun` (orange border, orange progress bar) |
| Running / Paused — over max | `critical-overrun` (red border, red progress bar, red elapsed timer) |
| Completed — over planned | `over-planned` (amber-tinted border, cream background) |
| Completed — over max | `over-max` (red border, light-red background) |

**Overrun badge** — inserted into `step-timer-row` after the "/ X min planned" label:

```razor
@if (IsMaxOverrun)
    <span class="step-overrun-badge overrun-critical">+@OverrunMinutes min — exceeds max</span>
else if (IsOverrun)
    <span class="step-overrun-badge overrun-warn">+@OverrunMinutes min over</span>
```

**Completed duration colouring** — `step-actual-dur` class gains a modifier (`dur-on-time` / `dur-over-planned` / `dur-over-max`) to colour the actual time in green, orange, or red.

### Audible alerts

> **Implemented:** 2026-06-03

Tone generation via the Web Audio API — no audio files. Module at `BreadMaking.App/wwwroot/js/audio.js`, loaded via `<script>` in `index.html`.

```js
window.breadAudio = (() => {
    // ...
    return {
        warnOverPlanned() { beep(523, 0.45); },           // C5 — single soft tone
        warnOverMax()     { beep(880, 0.3); setTimeout(() => beep(880, 0.3), 380); }  // A5 × 2
    };
})();
```

`AudioContext` starts suspended (browser policy). It becomes resumable after the first user gesture (clicking Start on a step), which always precedes any overrun.

**Alert dispatch in `LiveBake.razor`:**

The 1-second ticker was changed from `StateHasChanged` to `TickAsync`, which calls `CheckOverrunAlertsAsync` before re-rendering:

```csharp
private readonly HashSet<int> _alreadyAlertedPlanned = [];
private readonly HashSet<int> _alreadyAlertedMax     = [];

private async Task CheckOverrunAlertsAsync()
{
    if (_bake is null || !_soundEnabled) return;
    foreach (var step in _bake.StepLogs.Where(s => s.Status == StepStatus.Running))
    {
        var elapsed = DateTimeOffset.UtcNow - step.StartedAt!.Value;
        if (step.MaxDurationMin > 0
            && elapsed > TimeSpan.FromMinutes(step.MaxDurationMin)
            && _alreadyAlertedMax.Add(step.Id))
            await JS.InvokeVoidAsync("breadAudio.warnOverMax");
        else if (elapsed > TimeSpan.FromMinutes(step.PlannedDurationMin)
            && !_alreadyAlertedMax.Contains(step.Id)
            && _alreadyAlertedPlanned.Add(step.Id))
            await JS.InvokeVoidAsync("breadAudio.warnOverPlanned");
    }
}
```

`HashSet.Add` returns `true` only on the first insertion — each alert fires exactly once per step per page session.

### Sound toggle

> **Implemented:** 2026-06-03

A `🔔 / 🔕` radio pill is rendered inside `live-bake-header` in `LiveBake.razor`. Toggling sets `_soundEnabled`; the visual overrun indicators are unaffected by this flag.

---

## Edge cases

From spec section C7:

| Case | Handling |
|------|---------|
| 16-hour cold proof across server restart | `StartedAt` is in the DB; elapsed is always derived — zero reconciliation needed |
| Pause then adjust duration | `AdjustPlannedAsync` must not touch `StartedAt`/`EndedAt`; covered by unit test |
| Skipped step (no autolyse for einkorn) | Add `StepStatus.Skipped`; UI shows "Skip" button alongside "Start"; skipped steps show as `— SKIPPED` in the timeline |
| Out-of-range measurement | Server returns 422; client shows terracotta hint with range; value is not saved |
| Grain with no seeded recipe | `BakeSessionService` falls back to wheat defaults; logs warning to server console |
| Multiple tabs / devices open | All state is server-side; each tab refetches on `visibilitychange` or page focus |
| Overrun | Soft amber/terracotta visual; baker presses Done as normal; actual duration is saved in `EndedAt − StartedAt` |

---

## Testing checklist (per milestone)

**M2:**
- [ ] Start a step → `Status == Running`, `StartedAt` set
- [ ] Pause a step → `Status == Paused`, `EndedAt` set, elapsed frozen
- [ ] Resume a step → accumulated elapsed preserved within 1 s
- [ ] Adjust duration on running step → `StartedAt`/`EndedAt` unchanged
- [ ] Adjust duration clamps to `[min, max]`

**M3:**
- [ ] Refresh tab during a running step → elapsed correct after reload
- [ ] Start Bake button → navigates to `/bake/{id}` with all steps listed
- [ ] Complete all steps → no errors, bake status reflects completion

**M4:**
- [ ] Add measurement → chip appears on step card
- [ ] pH 14 submitted → 422 returned, hint shows valid range
- [ ] `RecordedAt` on saved measurement is server-time, not client-supplied

**M5:**
- [ ] Rise curve shows 50–75% shaded band
- [ ] Gantt renders before first step start; collapses after
- [ ] Run chart renders with demo / seed data

**M6:**
- [ ] CSV export contains all steps and measurements
- [ ] JSON export is valid JSON with full bake graph
- [ ] Grain comparison chart shows bars for all grains with 3+ bakes

**M9:**
- [ ] "Log outcome" button opens OutcomeSheet bottom sheet
- [ ] All 7 outcome fields can be set and cleared independently with ± steppers
- [ ] Default starting values are sensible (loaf 10 cm, oven spring 20%, internal temp 95 °C)
- [ ] PUT /api/bakes/{id}/outcome returns 204; outcome chips appear on page
- [ ] "Edit" reopens sheet with pre-filled values
- [ ] Photo can be selected, previewed, and uploaded alongside outcome fields
- [ ] Photo survives a subsequent outcome field edit (PUT does not overwrite PhotoPath)
- [ ] History list shows ✓ checkmark on bakes with logged outcomes

**M10:**
- [ ] `GET /api/bakes/{id}` returns `hydrationPct`, `totalFlourGrams`, `saltPct`, `inoculationPct`
- [ ] Live bake header shows formula summary line
- [ ] Clone-bake pre-fills hydration and formula fields in the advisor
- [ ] History list shows hydration % on each bake card

**M11:**
- [ ] Expand a step card → notes textarea is visible
- [ ] Type a note → auto-saves within 1.2 s; "Saving…" / "Saved" status feedback
- [ ] Refresh page → note is still present
- [ ] Complete step → 📝 chip visible on the collapsed card when note is set

**M12:**
- [ ] OutcomeSheet shows 5-star rating row; tapping a star saves the score
- [ ] Tag input accepts comma-separated text; tags appear as chips on history card
- [ ] "Best loaves only" filter in History hides bakes without a rating

**M13:**
- [ ] `/starter` page lists all starters; "Add starter" creates one
- [ ] Log a feed entry (amounts + ambient temp); entry appears in feed list
- [ ] Start a bake; link it to the latest feed entry
- [ ] History bake card shows "fed X h ago" badge

**M14:**
- [ ] "Save as recipe" on recommendation panel creates a user recipe
- [ ] "Saved recipes" row appears in advisor; selecting one pre-fills inputs
- [ ] Delete a user recipe → it disappears from the list
- [ ] Seeded recipes are not deletable and do not appear in recipe CRUD list

**M15:**
- [ ] `/history/analytics` loads with scatter chart; dots appear for each bake
- [ ] Changing the X or Y axis metric re-plots the chart
- [ ] Bake diff: select two bakes → table shows all inputs + outcomes side-by-side
- [ ] Personal bests list shows one bake per grain

**M19:**
- [ ] `/calculators` page loads; all six tabs / accordion sections are present
- [ ] Baker's % scaling: enter flour 100%, water 72%, levain 20%, salt 2% + 900 g target → flour 457 g, water 329 g, levain 91 g, salt 9 g (sum = 900 g)
- [ ] Batch scaling: 20 loaves × 500 g baked @ 12% bake loss, 2% scale loss → batch dough ~11 592 g; ingredients feed through from scaling calculator
- [ ] DDT calculator: DDT 25 °C, flour 20 °C, room 22 °C, friction "hand-folds" (2 °C), no preferment → water temp ~31 °C
- [ ] DDT with preferment: DDT 25 °C, flour 20 °C, room 22 °C, preferment 24 °C, friction 2 °C → correct 5-factor result
- [ ] Hydration calculator: 500 g flour, 75% hydration, 100 g levain @ 100% → levain split 50/50; dough flour 450 g, dough water 325 g; overall hydration = 75%
- [ ] Cost per loaf: leave energy/labour/packaging/overhead at zero → result equals ingredient cost / loaf count
- [ ] Roux fold (Tangzhong, 6%): 500 g flour, 70% hydration → roux 30 g flour + 150 g liquid; dough 470 g flour + 200 g liquid; totals 500 g / 350 g preserved
- [ ] Roux fold (Yudane, 1:1, 6%): roux 30 g flour + 30 g liquid; dough 470 g flour + 320 g liquid; totals preserved
- [ ] All endpoints return 400 on invalid input (e.g. negative weight, hydration > 200%)

**M16:**
- [ ] Disable network in DevTools → offline banner appears in `LiveBake`
- [ ] Start/pause/complete a step while offline → no error shown
- [ ] Re-enable network → queued actions replay; step status updates correctly
- [ ] App can be installed from the browser (manifest + service worker registered)

**M20:**
- [ ] Complete a sourdough bake → outcome section shows "Room temp · cloth/paper · 3–5 days" storage chip
- [ ] Storage chip expands to show temperature ladder and rationale
- [ ] `/safety` page (or panel) renders Rope, Mould, and Staling fault cards with symptoms and fixes
- [ ] Rope card: lists stringy centre + melon smell as symptoms; recommends acidify and discard

**M21:**
- [ ] `/kit` page loads; preheat calculator returns 60 min for conventional oven + baking steel
- [ ] Preheat calculator returns 45 min for conventional oven + baking stone
- [ ] Steam method selector marks Dutch oven as top-rated; shows lid-on 20 min / lid-off 15 min protocol
- [ ] Tiered buying guide renders three tiers; Starter tier lists scale, Dutch oven, bench knife, banneton, lame, probe
- [ ] Kit nav button added to History page header

**M22:**
- [ ] Advisor shows steamed-bread option; selecting it presents the Mantou timeline
- [ ] Live bake shows steam-step note with lid protocol
- [ ] History labels bake as "Steamed"
- [ ] Outcome troubleshooting offers wrinkles / density / yellow tinge options

**M23:**
- [ ] Enriched method branch visible in advisor; butter/egg/sugar inputs appear
- [ ] Formula summary on Live Bake header shows enrichment values
- [ ] "Use Tangzhong?" toggle pre-populates step 1 quantities from M19 roux calculator
- [ ] History labels bake as "Enriched"
- [ ] Outcome troubleshooting offers enriched-specific options (dense crumb, collapses, stales fast)

---

## M10 — Formula & extended bake inputs

### Entity changes

Add to `Bake.cs`:

```csharp
public double?  HydrationPct    { get; set; }
public int?     StarterActivity { get; set; }   // mirrors BreadInputs.StarterActivity enum value
public double?  TotalFlourGrams { get; set; }
public double?  SaltPct         { get; set; }
public double?  InoculationPct  { get; set; }
```

### StartBakeRequest changes

```csharp
public double?  HydrationPct    { get; set; }
public int?     StarterActivity { get; set; }
public double?  TotalFlourGrams { get; set; }
public double?  SaltPct         { get; set; }
public double?  InoculationPct  { get; set; }
```

`RecommendationPanel.razor` already has access to the full `BreadInputs` when it posts to `POST /api/bakes` — map `inputs.HydrationPercent`, `inputs.StarterActivity`, etc. into the request there.

### BakeSessionService

In `CreateFromInputsAsync`, after creating the `Bake` entity, populate the new fields from the request:

```csharp
bake.HydrationPct    = req.HydrationPct;
bake.StarterActivity = req.StarterActivity;
bake.TotalFlourGrams = req.TotalFlourGrams;
bake.SaltPct         = req.SaltPct;
bake.InoculationPct  = req.InoculationPct;
```

### DTO + display

Add to `BakeDto` and `BakeListItemDto`. In `LiveBake.razor` header, show:

```razor
@if (_bake.HydrationPct.HasValue || _bake.TotalFlourGrams.HasValue)
{
    <p class="live-bake-formula">
        @if (_bake.HydrationPct.HasValue)  { <span>@_bake.HydrationPct.Value.ToString("F0")%</span> }
        @if (_bake.TotalFlourGrams.HasValue){ <span class="meta-sep">·</span><span>@_bake.TotalFlourGrams.Value.ToString("F0") g flour</span> }
        @if (_bake.SaltPct.HasValue)        { <span class="meta-sep">·</span><span>@_bake.SaltPct.Value.ToString("F1")% salt</span> }
    </p>
}
```

### Migration

```
dotnet ef migrations add AddFormulaFieldsToBake --project BreadMaking.App.Server
dotnet ef database update --project BreadMaking.App.Server
```

---

## M11 — Per-step notes

### Entity + migration

```csharp
// BakeStepLog.cs
public string? Notes { get; set; }
```

```
dotnet ef migrations add AddNotesToBakeStepLog --project BreadMaking.App.Server
```

### API endpoint

```csharp
group.MapPatch("/{id}/notes", async (int id, UpdateNotesRequest req, IBakeSessionService svc) =>
    await svc.SaveStepNotesAsync(id, req.Notes) ? Results.NoContent() : Results.NotFound());
```

Add `SaveStepNotesAsync(int stepLogId, string? notes)` to `IBakeSessionService` / `BakeSessionService` — mirrors the existing `SaveNotesAsync` for bakes.

### StepCard UI

Inside the expanded card section, after the measurements row:

```razor
<div class="step-notes-row">
    <textarea class="step-notes-textarea"
              placeholder="Notes for this step…"
              @bind="@_stepNotes"
              @oninput="HandleStepNotesInput" />
    @if (_stepSaveStatus is not null)
    {
        <span class="notes-save-status @(_stepSaveStatus == "Saving…" ? "saving" : "saved")">
            @_stepSaveStatus
        </span>
    }
</div>
```

Debounce pattern is identical to `LiveBake.razor`'s `HandleNotesInput` — 1200 ms, `PATCH /api/steplogs/{id}/notes`.

On the **collapsed** completed card, add a 📝 chip alongside the actual-duration span when `Step.Notes` is not null/empty.

---

## M12 — Bake ratings, tags & best-loaf flag

### Entity + migration

```csharp
// BakeOutcome.cs
public int?    OverallScore { get; set; }   // 1–5
public string? Tags         { get; set; }   // comma-separated
public bool    IsBestLoaf   { get; set; }
public string? CrumbNotes   { get; set; }   // free-form crumb observation (open/tight/gummy/flying crust etc.)
```

```
dotnet ef migrations add AddRatingAndTagsToBakeOutcome --project BreadMaking.App.Server
```

### OutcomeSheet additions

Add a RATING section above PHOTO:

```razor
<div class="os-group-label">RATING</div>
<div class="os-star-row">
    @for (int i = 1; i <= 5; i++)
    {
        var star = i;
        <button class="os-star @((_overallScore ?? 0) >= star ? "os-star-filled" : "")"
                @onclick="() => _overallScore = (_overallScore == star ? null : star)">★</button>
    }
</div>
<div class="os-tag-row">
    <input class="os-tag-input" placeholder="Tags (comma separated)"
           @bind="_tags" @bind:event="oninput" />
</div>
```

`_overallScore`, `_tags`, `_isBestLoaf` follow the same load-on-open / save-on-save pattern as existing fields in `OutcomeSheet.razor`.

### BakeListItemDto

Add `OverallScore`, `Tags`, `IsBestLoaf`. Update `DtoMapper` and the history query projection.

---

## M13 — Starter journal

### New entities

```csharp
// Starter.cs
public class Starter {
    public int     Id          { get; set; }
    public string  Name        { get; set; } = "";
    public double  HydrationPct{ get; set; }
    public string? FlourBlend  { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Notes       { get; set; }
    public List<StarterFeedLog> Feeds { get; set; } = [];
}

// StarterFeedLog.cs
public class StarterFeedLog {
    public int     Id               { get; set; }
    public int     StarterId        { get; set; }
    public Starter Starter          { get; set; } = null!;
    public DateTimeOffset FedAt     { get; set; }
    public double  FlourGrams       { get; set; }
    public double  WaterGrams       { get; set; }
    public double  PrevStarterGrams { get; set; }
    public double? AmbientTempC     { get; set; }
    public double? PeakHours        { get; set; }
    public bool?   FloatTestPassed  { get; set; }
    public string? FeedRatio        { get; set; }   // e.g. "1:2:2" (starter:flour:water by weight)
}
```

Add `StarterFeedLogId int?` FK on `Bake` with nullable navigation.

### Migrations

```
dotnet ef migrations add AddStarterJournal      --project BreadMaking.App.Server
dotnet ef migrations add AddStarterFeedLinkToBake --project BreadMaking.App.Server
dotnet ef database update --project BreadMaking.App.Server
```

### StarterService

`IStarterService` with: `GetAllAsync()`, `CreateAsync(name, hydration, flourBlend)`, `LogFeedAsync(starterId, feedDto)`, `GetFeedsAsync(starterId)`.

### /starter page

Simple list of starters with an "Add" sheet. Per-starter expanded view shows feed history as a timeline with peak hours plotted (ApexCharts line chart — reuses the existing chart setup).

**Feeding ratio guidance** (baker's guide §19.2): `FeedRatio` is stored as a string (e.g. `"1:2:2"` = starter:flour:water by weight). Display a computed peak estimate beside the ratio chip:

| Ratio | Approx peak at 22 °C | Flavour |
|-------|----------------------|---------|
| 1:1:1 | 4–6 h | Tangier, more acidic |
| 1:2:2 | 6–8 h | Balanced — recommended default |
| 1:5:5 | 10–14 h | Mildest — overnight builds |

**Health indicators**: The feed list entry should surface the following status chips derived from the `Notes` field (the baker enters these freeform, but the UI should prompt with recognisable states): `🟡 Hungry (hooch)` — harmless, feed sooner; `🟢 Active` — yeasty/mildly sour smell, doubles reliably; `🔴 Discard` — pink/orange streaks or mould. These are display-only labels parsed from notes; no separate `HealthStatus` enum is required.

**Preferments reference**: The guide (§19.4) documents five preferment types. `StarterFeedLog.FeedRatio` covers sourdough levain builds. If the advisor later supports commercial-yeast preferments (Poolish, Biga, Pâte fermentée), a `PrefermentType` enum on `Recipe` would distinguish them; this is deferred to M14+.

---

## M14 — User recipe library

### Entity changes

```csharp
// Recipe.cs (additions)
public bool    IsUserDefined   { get; set; }
public string? CreatedByLabel  { get; set; }
public RecipeFormula? Formula  { get; set; }

// RecipeFormula.cs (new)
public class RecipeFormula {
    public int     Id           { get; set; }
    public int     RecipeId     { get; set; }
    public Recipe  Recipe       { get; set; } = null!;
    public double  FlourWeightG { get; set; }
    public double  WaterPct     { get; set; }
    public double  SaltPct      { get; set; }
    public double  StarterPct   { get; set; }
    public string? Notes        { get; set; }
}
```

All 12 seeded recipes have `IsUserDefined = false`. The API endpoints filter to `IsUserDefined = true` only, preventing deletion of seed data.

### Migrations

```
dotnet ef migrations add AddUserDefinedToRecipe --project BreadMaking.App.Server
dotnet ef migrations add AddRecipeFormula       --project BreadMaking.App.Server
dotnet ef database update --project BreadMaking.App.Server
```

### Advisor integration

In `RecommendationPanel.razor`, after the recommendation is shown, add a "Save as recipe" link button. On click, POST to `/api/recipes` with the current inputs mapped to a recipe DTO.

On `Home.razor`, load `GET /api/recipes` on first render and display as chip rows above the grain selector. Tapping a chip calls a new `LoadRecipe(RecipeDto r)` method that sets `BreadInputs` fields and triggers a re-render.

---

## M15 — Analytics & trends

### Server-side queries

`AnalyticsEndpoints.cs` — two endpoints backed by raw EF LINQ projections:

**Correlations:**
```csharp
// factor: hydration → bake.HydrationPct; bulktime → bulk step actual duration; kitchentemp → bake.AmbientTempC
// metric: crumb → outcome.CrumbOpenness; ovenspring → outcome.OvenSpringPct; taste → outcome.TasteScore
var points = await db.Bakes
    .Where(b => b.Outcome != null)
    .Select(b => new { X = xSelector(b), Y = ySelector(b), b.Id, b.StartedAt, GrainName = b.Recipe!.GrainProfile!.Name })
    .Where(p => p.X != null && p.Y != null)
    .ToListAsync();
```

Use `Expression<Func<Bake, double?>>` factory methods keyed by the `factor` and `metric` query params.

### /history/analytics page

Uses ApexCharts scatter series (same `AddApexCharts()` setup already in place). The bake diff table is a simple `<table>` comparing two `BakeDto` objects fetched by ID.

---

## M16 — PWA & offline

### Service worker

Blazor WASM projects generate `service-worker.js` (development) and `service-worker.published.js` (production). In development mode, edit `service-worker.js` to cache the active bake API response:

```js
self.addEventListener('fetch', event => {
    if (event.request.url.includes('/api/bakes/') && event.request.method === 'GET') {
        event.respondWith(
            caches.open('bake-cache-v1').then(cache =>
                fetch(event.request).then(resp => { cache.put(event.request, resp.clone()); return resp; })
                .catch(() => cache.match(event.request))
            )
        );
    }
});
```

### Offline queue (wwwroot/js/offlineQueue.js)

```js
const DB_NAME = 'bread-offline', STORE = 'queue';
async function enqueue(url, method) { /* open IDB, add { url, method, ts } */ }
async function flush()             { /* replay each entry in order, delete on 2xx */ }
window.addEventListener('online', flush);
window.breadOffline = { enqueue };
```

`LiveBake.razor` calls `breadOffline.enqueue(url, method)` via `IJSRuntime` instead of `HttpClient.PostAsync` when `navigator.onLine` is false.

### manifest.json (wwwroot/manifest.json)

```json
{
  "name": "Bread-Making",
  "short_name": "Bread",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#fdf6ec",
  "theme_color": "#7a4f2e",
  "icons": [
    { "src": "icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

Add `<link rel="manifest" href="manifest.json" />` to `index.html`.

---

## M19 — Baker's calculators

All calculator logic lives in `CalculatorService.cs` on the server. The methods are pure functions — no `DbContext`, no `HttpContext`, no `async`. Each endpoint in `CalculatorEndpoints.cs` is one line: call the service, return the result. Source: baker's guide §48 + §54.

### CalculatorService.cs

```csharp
public static class CalculatorService
{
    // §48.1 — Baker's percentage scaling
    public static decimal TotalFormulaPct(IEnumerable<IngredientPct> formula)
        => formula.Sum(i => i.Percent);                    // flour's percent is 100

    public static Dictionary<string, decimal> Scale(
        IEnumerable<IngredientPct> formula, decimal targetDoughGrams)
    {
        var tfp   = TotalFormulaPct(formula) / 100m;
        var flour = targetDoughGrams / tfp;
        return formula.ToDictionary(
            i => i.Name,
            i => Math.Round(flour * i.Percent / 100m, 1));
    }

    // §48.2 — Batch scaling with yield/loss
    public static decimal DoughPerLoaf(decimal bakedG, decimal bakeLossPct)
        => bakedG / (1 - bakeLossPct / 100m);

    public static decimal BatchDough(int loaves, decimal doughPerLoafG,
                                     decimal scaleLossPct)
        => loaves * doughPerLoafG / (1 - scaleLossPct / 100m);

    // §48.3 — DDT water temperature
    // frictionC: ~2 hand-folds, ~3 hand-knead, ~10 stand mixer, ~14 spiral, ~24 intensive
    public static decimal WaterTemp(decimal ddt, decimal flourC, decimal roomC,
                                    decimal frictionC, decimal? prefermentC = null)
    {
        var factors = new List<decimal> { flourC, roomC, frictionC };
        if (prefermentC is decimal p) factors.Add(p);
        int n = factors.Count + 1;          // +1 for the water term itself
        return ddt * n - factors.Sum();
    }

    // §48.4 — Levain split & true hydration
    public static (decimal flour, decimal water) SplitLevain(
        decimal levainGrams, decimal levainHydrationPct)
    {
        var flour = levainGrams / (1 + levainHydrationPct / 100m);
        return (Math.Round(flour, 1), Math.Round(levainGrams - flour, 1));
    }

    public static decimal OverallHydration(decimal doughFlour, decimal doughWater,
                                           decimal levainGrams, decimal levainHydrationPct)
    {
        var (lf, lw) = SplitLevain(levainGrams, levainHydrationPct);
        return Math.Round((doughWater + lw) / (doughFlour + lf) * 100m, 1);
    }

    // §48.5 — Cost per loaf
    public static decimal CostPerLoaf(
        Dictionary<string, decimal> gramsByIngredient,
        Dictionary<string, decimal> pricePerGram,
        decimal energyCost, decimal labourCost,
        decimal packagingCost, decimal overheadCost,
        int saleableLoaves)
    {
        var ingredients = gramsByIngredient
            .Sum(kv => kv.Value * pricePerGram.GetValueOrDefault(kv.Key, 0));
        var batch = ingredients + energyCost + labourCost
                  + packagingCost + overheadCost;
        return Math.Round(batch / saleableLoaves, 4);
    }

    // §54.3 — Water-roux fold (Tangzhong or Yudane)
    // rouxRatio: 5.0 for Tangzhong (1:5), 1.0 for Yudane (1:1)
    public static (decimal rouxFlour, decimal rouxLiquid,
                   decimal doughFlour, decimal doughLiquid)
    FoldRoux(decimal totalFlour, decimal hydrationPct,
             decimal rouxFlourSharePct, decimal rouxRatio)
    {
        var totalLiquid = totalFlour * hydrationPct / 100m;
        var rf = Math.Round(totalFlour * rouxFlourSharePct / 100m, 1);
        var rl = Math.Round(rf * rouxRatio, 1);
        return (rf, rl, Math.Round(totalFlour - rf, 1),
                        Math.Round(totalLiquid - rl, 1));
    }
}
```

### CalculatorEndpoints.cs

```csharp
app.MapGroup("/api/calculators")
   .MapPost("/scale",     (ScaleRequest req)      => CalculatorService.Scale(req.Formula, req.TargetDoughGrams))
   .MapPost("/batch",     (BatchRequest req)       => /* DoughPerLoaf → BatchDough → Scale */)
   .MapPost("/ddt",       (DdtRequest req)         => CalculatorService.WaterTemp(req.Ddt, req.FlourC, req.RoomC, req.FrictionC, req.PrefermentC))
   .MapPost("/hydration", (HydrationRequest req)   => CalculatorService.OverallHydration(req.DoughFlour, req.DoughWater, req.LevainGrams, req.LevainHydrationPct))
   .MapPost("/cost",      (CostRequest req)        => CalculatorService.CostPerLoaf(req.GramsByIngredient, req.PricePerGram, req.Energy, req.Labour, req.Packaging, req.Overhead, req.SaleableLoaves))
   .MapPost("/roux",      (RouxRequest req)        => CalculatorService.FoldRoux(req.TotalFlour, req.HydrationPct, req.RouxFlourSharePct, req.RouxRatio));
```

### Friction factor presets (baker's guide §50.4)

| Mix method | FrictionC to use |
|---|---|
| Hand — folds | 2 °C |
| Hand — knead | 3 °C |
| Stand mixer (hook) | 10 °C |
| Spiral mixer | 14 °C |
| Intensive (planetary, high) | 24 °C |

Display as a radio pill group on the DDT tab. Include a "Custom" option that unlocks a numeric input.

### Shared DTOs (BreadMaking.App.Shared/Dtos/Calculators/)

```csharp
// ScaleRequest / ScaleResult
public record IngredientPct(string Name, decimal Percent);
public record ScaleRequest(IEnumerable<IngredientPct> Formula, decimal TargetDoughGrams);
public record ScaleResult(Dictionary<string, decimal> Grams, decimal TotalFormulaPct);

// DdtRequest / DdtResult
public record DdtRequest(decimal Ddt, decimal FlourC, decimal RoomC,
                          decimal FrictionC, decimal? PrefermentC);
public record DdtResult(decimal WaterTempC);

// HydrationRequest / HydrationResult
public record HydrationRequest(decimal DoughFlour, decimal DoughWater,
                                decimal LevainGrams, decimal LevainHydrationPct);
public record HydrationResult(decimal LevainFlour, decimal LevainWater,
                               decimal FinalDoughFlour, decimal FinalDoughWater,
                               decimal OverallHydrationPct);

// RouxRequest / RouxResult  (rouxRatio: 5.0 = Tangzhong, 1.0 = Yudane)
public record RouxRequest(decimal TotalFlour, decimal HydrationPct,
                           decimal RouxFlourSharePct, decimal RouxRatio);
public record RouxResult(decimal RouxFlour, decimal RouxLiquid,
                          decimal DoughFlour, decimal DoughLiquid,
                          decimal CheckTotalFlour, decimal CheckTotalLiquid);
```

### Calculators.razor

The page has a two-column layout at ≥768 px: a tab list on the left and the active calculator form on the right. On mobile, tabs collapse to a horizontal scroll strip at the top.

Each calculator form follows the same pattern:
- Input fields with labels, units and hint text
- A **Calculate** button (primary, amber fill)
- A result card that appears below the button once calculated (results never shown until the baker submits)
- A **Reset** button that clears inputs and hides the result

Pre-fill the baker's-% scaling tab with a default wheat sourdough formula (flour 100%, water 72%, levain 20%, salt 2%) so the page is immediately useful without data entry.

The DDT tab pre-fills with mix method "hand-folds" selected and `FrictionC = 2`. Changing the radio updates the friction field in real time; the custom option shows a numeric input.

The roux tab shows a Tangzhong / Yudane radio. Tangzhong sets `RouxRatio = 5`; Yudane sets `RouxRatio = 1`. The result card shows a two-row table (Roux row / Dough row) and a Totals check row in green.

---

## M20 — Food safety, shelf life & storage (§49)

### StorageAdvisor component

Pure client-side component; no API or DB required. Add to `BreadMaking.App/Components/StorageAdvisor.razor`.

Logic table (stored as a `static readonly` lookup in the component):

| BakeMethod | StarterActivity | StorageMedium | ShelfLifeDays | Notes |
|---|---|---|---|---|
| Autolyse / Fermentolyse | AtPeak / PastPeak | Cloth or paper bag | 3–5 | Sourdough acidity extends shelf life |
| Autolyse / Fermentolyse | JustFed / NotAvailable | Cloth or paper bag | 2–3 | Lower acidity; consume sooner |
| Steamed (M22) | any | Airtight container or fridge | 1–2 room / 4–5 fridge | No crust barrier; moisture loss fast |
| Enriched (M23) | any | Wrapped, room temp | 4–6 | Fat content slows staling |

Render in `LiveBake.razor` after the outcome section when `_bake.EndedAt != null`:

```razor
<StorageAdvisor Method="@_bake.Method" StarterActivity="@_bake.StarterActivity" />
```

### /safety route

Add `BreadMaking.App/Pages/Safety.razor` with three fault cards (no DB, no API):

```
Rope       → symptoms, cause, control, when-to-discard
Mould      → types, never-cut-off rule, defences
Staling    → retrogradation, fridge-is-worst rule, freezer guidance, one-time refresh
```

Each card follows the `.calc-form` panel style from `bakery.css` (reuse existing CSS variables). Add a **Safety** link to the History header alongside Calculators and Starters.

### Temperature ladder

Inline SVG or CSS-grid bar in `Safety.razor`:

| Zone | Range | Colour |
|---|---|---|
| Safe cold | < 5 °C | `--zone-cold` (blue-grey) |
| Danger zone | 5–60 °C | `--zone-hot` (terracotta) |
| Crust formation | 140–180 °C | `--zone-warm` (amber) |
| Full bake | > 180 °C | `--accent` |

---

## M21 — Equipment & kit guide (§50 + §51)

### KitService.cs (server — static, no DB)

```csharp
public static class KitService
{
    // §50.1–50.2: returns recommended preheat time in minutes
    public static int PreheatMinutes(string ovenType, string surface) => (ovenType, surface) switch
    {
        ("convection", "steel")  => 45,
        ("convection", "stone")  => 40,
        ("conventional", "steel") => 60,
        ("conventional", "stone") => 55,
        ("gas", "steel")         => 55,
        ("gas", "stone")         => 50,
        _                        => 45,   // bare tray fallback
    };
}
```

### KitEndpoints.cs

```csharp
group.MapGet("/preheat", (string ovenType, string surface) =>
    Results.Ok(new { Minutes = KitService.PreheatMinutes(ovenType, surface) }));
```

### KitGuide.razor (/kit)

Four sections as accordion cards (collapse/expand with `@_open` bool — no library):

1. **Preheat calculator** — two radio groups (oven type, surface) → "Preheat for N minutes"
2. **Steam methods** — ranked list (Dutch oven / combo cooker / lava rocks / spray / pan); each row: method name, effectiveness rating (1–5 stars), one-line protocol
3. **Tiered buying guide** — three tiers; each tier is a `<ul>` of items with a ✓ (own) or □ (buy next) checkbox driven by local `localStorage` flags
4. **Scoring guide** — static diagram and blade-angle rule

Nav: add `🔧 Kit` button to History header after `🧮 Calculators`.

---

## M22 — Steamed breads — Mantou & Baozi (§52)

### Entity + enum changes

Add `BakeMethod.Steamed` to the existing `BakeMethod` enum in `BreadMaking.App.Shared/Enums/`.

### Migration + seed data

```
dotnet ef migrations add AddSteamedBreadSeeds --project BreadMaking.App.Server
```

Seed new `GrainProfile` (low-protein wheat), `Recipe` (method = Steamed), and `RecipeStep` rows per the ROADMAP.md step table. Use `IsUserDefined = false`.

### Advisor branch

In `BreadAdvisorService.GetRecommendation()`, add a `BakeMethod.Steamed` branch:
- Suppresses the autolyse/fermentolyse radio (steamed bakes do not use either)
- Returns a shorter timeline (no cold proof, no scoring)
- Adds a `SteamNote` field to the result: `"Vigorous simmer; cloth-lined lid; do not open for first 10 min"`

### LiveBake steam step note

In `StepCard.razor`, when `Step.Phase == "Bake"` and `_bake.Method == BakeMethod.Steamed`, render a `.step-steam-note` panel below the timer:

```
⚠ Steam protocol: line lid with a cloth to catch condensation drops. Do not lift the lid
for the first 10 min — temperature shock causes wrinkling. Rest 2–3 min with lid ajar
before removing entirely.
```

---

## M23 — Enriched dough & milk breads — Shokupan (§53)

### Entity changes

```csharp
// Bake.cs (additions for M23)
public double? ButterPct      { get; set; }
public double? EggPct         { get; set; }
public double? SugarPct       { get; set; }
public double? MilkPct        { get; set; }
public double? MilkPowderPct  { get; set; }
public bool    IsPullmanTin   { get; set; }
```

```
dotnet ef migrations add AddEnrichedFormulaFieldsToBake --project BreadMaking.App.Server
```

Seed new `Recipe` rows for Shokupan (BakeMethod = Enriched; grain = strong white wheat) with the 11-step timeline from ROADMAP.md § M23.

### Advisor UI additions

In `ExperiencedForm.razor`, after the existing method radio group, add an `@if (Method == Enriched)` section:
- Input fields: butter %, egg %, sugar %, milk %, milk powder % (all default 0)
- "Use Tangzhong?" toggle (bool) — when true, on start-bake, calls `/api/calculators/roux` client-side and persists the roux quantities as step notes on step 1 and step 2

### Formula summary

Extend the existing `live-bake-formula` paragraph in `LiveBake.razor`:

```razor
@if (_bake.ButterPct.HasValue)
    { <span class="meta-sep">·</span><span>@_bake.ButterPct.Value.ToString("F0")% butter</span> }
@if (_bake.SugarPct.HasValue)
    { <span class="meta-sep">·</span><span>@_bake.SugarPct.Value.ToString("F0")% sugar</span> }
```

### Roux integration

When `IsPullmanTin` and `UseTangzhong` are both true:
1. On `POST /api/bakes`, compute the roux split client-side using `RouxRequest { TotalFlour = req.TotalFlourGrams, HydrationPct = req.HydrationPct, RouxFlourSharePct = 6, RouxRatio = 5 }` — call `/api/calculators/roux` before submitting
2. Attach the result as `InitialNotes` on step 1 ("Tangzhong: Xg flour + Yg liquid") and step 2 ("Cool to room temp before mixing into dough — aim for DDT of 25 °C")
