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

## Solution structure after M0

```
Bread-Making.sln
├── BreadMaking.App.Client/          (Blazor WASM — .NET 10)
│   ├── Models/
│   ├── Services/
│   ├── Components/
│   │   └── bake/                   ← new: StepCard, MeasurementSheet, PlanningGantt, charts
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── LiveBake.razor          ← new: /bake/{id}
│   │   ├── History.razor           ← new: /history
│   │   └── GrainComparison.razor   ← new: /history/compare
│   └── wwwroot/
│
├── BreadMaking.App.Server/          (ASP.NET Core Web App — .NET 10)
│   ├── Api/                        ← endpoint mapping files (or Controllers/)
│   ├── Services/
│   │   ├── TimerService.cs
│   │   ├── MeasurementService.cs
│   │   └── BakeSessionService.cs
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── Seed/                   ← seed data as static methods or IEntityTypeConfiguration
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
