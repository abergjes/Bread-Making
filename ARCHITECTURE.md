# Architecture

## Current state

The app is a standalone Blazor WebAssembly project (`.NET 10`, `Microsoft.NET.Sdk.BlazorWebAssembly`). All logic runs in the browser. There is no server, no database, and no persistence beyond the current browser tab.

```
BreadMaking.App/                      (Blazor WASM, single project)
├── Models/                           ← BreadInputs, BreadRecommendation,
│                                       TimelineStep, GrainProfile, GrainCatalogue
├── Services/BreadAdvisorService.cs   ← recommendation engine (pure logic, no I/O)
├── Components/                       ← Razor components
├── Pages/                            ← Home.razor, NotFound.razor
└── wwwroot/                          ← index.html, bakery.css, Bootstrap 5
```

The existing flow:
`Home.razor → ExperiencedForm / NoviceWizard → BreadAdvisorService.GetRecommendation() → RecommendationPanel → BakingTimeline (static display)`

---

## Target state

The solution becomes a **hosted Blazor WASM** solution — three projects sharing one `Bread-Making.sln`:

```
Bread-Making.sln
├── BreadMaking.App.Client   ← Blazor WASM (browser — existing app, extended)
├── BreadMaking.App.Server   ← ASP.NET Core (server — hosts client, owns API + DB)
└── BreadMaking.App.Shared   ← plain class library (DTOs, shared enums — no EF/Blazor refs)
```

The **Server project** is the host: it serves the WASM bundle as static files and exposes the REST API. The client project is unchanged in how it is built and deployed — it is just served from the server rather than directly.

---

## Project responsibilities

### BreadMaking.App.Client (Blazor WASM)

- All existing components and pages (unchanged)
- New pages: `LiveBake.razor` (`/bake/{id}`), `History.razor` (`/history`), `GrainComparison.razor` (`/history/compare`), `Calculators.razor` (`/calculators`), `KitGuide.razor` (`/kit`, M21), `SafetyPanel.razor` (component, M20)
- New components: `StepCard.razor`, `MeasurementSheet.razor`, `PlanningGantt.razor`, `RiseCurveChart.razor`, `RunChart.razor`
- `BreadAdvisorService` stays here — it is pure C# with no I/O and generates the step list sent to the server
- `HttpClient` factory configured with the server base address
- Client-side elapsed derivation: a `System.Threading.Timer` fires `StateHasChanged()` every second on the active bake page; elapsed is computed as `DateTimeOffset.UtcNow - dto.StartedAt` using the server-provided `StartedAt` value
- `ApexCharts.Blazor` for all chart components
- **No EF Core dependency**

### BreadMaking.App.Server (ASP.NET Core)

- Hosts and serves the WASM client (`UseBlazorFrameworkFiles`, `MapFallbackToFile`)
- REST API (minimal API endpoints or `[ApiController]` — see note below)
- `AppDbContext` (EF Core 9, SQLite for development; provider-configurable for production)
- Services: `TimerService`, `MeasurementService`, `BakeSessionService`
- EF Core migrations under `Server/Migrations/`
- Configuration: connection string and environment in `appsettings.json` / `appsettings.Development.json`

**Minimal API vs. controllers:** ASP.NET Core minimal API is preferred for new development in .NET 10 (less boilerplate, better for small surfaces). The spec code samples show `[ApiController]` classes; both produce identical behaviour. Use whichever is more familiar; the route structure is the same either way.

### BreadMaking.App.Shared (class library)

- DTOs used by both client and server: `BakeDto`, `BakeStepLogDto`, `MeasurementDto`, `BakeOutcomeDto`, `BakeListItemDto`
- Enum mirror types: `StepStatus`, `BakeMethod`
- Request models: `StartBakeRequest`, `AddMeasurementRequest`
- Calculator request/result types (M19) under `Dtos/Calculators/`: `ScaleRequest`, `ScaleResult`, `BatchRequest`, `BatchResult`, `DdtRequest`, `DdtResult`, `HydrationRequest`, `HydrationResult`, `CostRequest`, `CostResult`, `RouxRequest`, `RouxResult`
- No EF Core, no ASP.NET Core, no Blazor references — plain `net10.0` class library

---

## Data model

EF Core entities live on the server only. The client always communicates via DTOs from `Shared`.

### Entity relationship diagram (M0–M9, current)

```
GrainProfile (1) ──< Recipe (1) ──< RecipeStep
                        │
                     Bake (1) ──< BakeStepLog (1) ──< Measurement
                        │
                     BakeOutcome (0..1)
```

### Extended diagram (M10–M14 + M22–M23, planned additions)

```
Starter (1) ──< StarterFeedLog
                      │ (0..1)
GrainProfile (1) ──< Recipe (1) ──< RecipeStep
                        │         └──< RecipeFormula (0..1)
                     Bake (1) ──< BakeStepLog (1) ──< Measurement
                        │
                     BakeOutcome (0..1)
```

Colour coding:
- **Brown** (GrainProfile, Recipe, RecipeStep, RecipeFormula) — recipe definition, the template
- **Tan** (Bake, BakeStepLog) — a specific bake run, what actually happened
- **Amber** (Measurement, MeasurementType, BakeOutcome) — captured data
- **Green** (Starter, StarterFeedLog) — levain health journal

### Entity key fields

| Entity | Key fields |
|--------|-----------|
| `GrainProfile` | `Name`, `Icon`, `FlavorNotes` (nullable), `NutritionHighlights` (nullable), `UsageNotes` (nullable), `HistoricalOrigin` (nullable) — descriptive fields seeded from baker's guide §15 |
| `Recipe` | `Method` (autolyse/fermentolyse/steamed/enriched/other), `GrainProfileId`, `TargetDoughTempC`, `FrictionFactorC`, `IsUserDefined`, `CreatedByLabel` — `Steamed` added by M22; `Enriched` added by M23 |
| `RecipeStep` | `Order`, `Name`, `Phase`, `DefaultDurationMin`, `MinDurationMin`, `MaxDurationMin`, `StepMin`, `TargetTempC` |
| `RecipeFormula` | `RecipeId`, `FlourWeightG`, `WaterPct`, `SaltPct`, `StarterPct`, `Notes` |
| `Bake` | `RecipeId`, `StartedAt`, `EndedAt`, `AmbientTempC`, `AmbientHumidityPct`, `FlourBatch`, `Notes`, `HydrationPct`, `StarterActivity`, `TotalFlourGrams`, `SaltPct`, `InoculationPct`, `StarterFeedLogId` (nullable FK) |
| `BakeStepLog` | `PlannedDurationMin`, `StartedAt`, `EndedAt`, `Status` (enum), `ActualDurationMin` (derived), `Notes` |
| `Measurement` | `BakeStepLogId`, `MeasurementTypeId`, `Value`, `Unit`, `RecordedAt` (server-stamped) |
| `BakeOutcome` | `LoafHeightCm`, `OvenSpringPct`, `InternalTempC`, `WeightLossPct`, `CrumbOpenness`, `CrustScore`, `TasteScore`, `PhotoPath`, `OverallScore`, `Tags`, `IsBestLoaf`, `CrumbNotes` (nullable free-form crumb observation), `ProofingResult` (enum: UnderProofed/Correct/OverProofed — M18) |
| `Bake` (M23 enriched fields) | `ButterPct`, `EggPct`, `SugarPct`, `MilkPct`, `MilkPowderPct` (all nullable), `IsPullmanTin` (bool) |
| `Starter` | `Id`, `Name`, `HydrationPct`, `FlourBlend`, `CreatedAt`, `Notes` |
| `StarterFeedLog` | `Id`, `StarterId`, `FedAt`, `FlourGrams`, `WaterGrams`, `PrevStarterGrams`, `AmbientTempC`, `PeakHours`, `FloatTestPassed`, `FeedRatio` (nullable, e.g. `"1:2:2"`) |

---

## Timer design — server as source of truth

The most important architectural decision in the spec: **the server stores when a step started, not a ticking counter.** Elapsed time is always derived:

```csharp
TimeSpan Elapsed(DateTimeOffset now) =>
    StartedAt is null ? TimeSpan.Zero
    : (EndedAt ?? now) - StartedAt.Value;
```

This makes a 16-hour cold proof, a locked phone, a closed tab, or a server restart completely transparent — there is nothing to reconcile.

### Pause / resume

**Pause:** Set `EndedAt = DateTimeOffset.UtcNow`, `Status = Paused`. Elapsed is now frozen at `EndedAt − StartedAt`.

**Resume (Start from Paused):** Shift `StartedAt` forward by the paused interval, then clear `EndedAt`:
```csharp
var frozenElapsed = log.EndedAt!.Value - log.StartedAt!.Value;
log.StartedAt = DateTimeOffset.UtcNow - frozenElapsed;
log.EndedAt = null;
log.Status = StepStatus.Running;
```
This preserves accumulated elapsed without a separate counter field.

### Duration adjustment

The ± controls change only `PlannedDurationMin` (the baker's current target). They never touch `StartedAt` or `EndedAt`. Overrun — `elapsed > PlannedDurationMin` — is a soft amber/terracotta warning in the UI, not a hard stop.

---

## Timer state machine

```
NotStarted ──[Start]──▶ Running ◀──[Resume]──┐
                            │                 │
                         [Pause]           Paused
                            └────────────────▶│
                            │
                    elapsed > planned
                            │
                         Overrun (display state only — still Running)
                            │
                         [Done ✓]
                            │
                        Completed
                    (ActualDuration saved = EndedAt − StartedAt)
```

`Overrun` is not a `StepStatus` enum value. It is a derived CSS class applied client-side when `status == Running && elapsed > PlannedDurationMin`.

---

## API surface

### Implemented (M0–M9)

```
POST   /api/bakes                              Create Bake from BreadInputs → BakeDto (201)
GET    /api/bakes/{id}                         Load bake with all step logs and measurements → BakeDto
GET    /api/bakes                              Paginated history list → BakeListItemDto[]
GET    /api/bakes/{id}/inputs                  Original advisor inputs for clone-bake → StartBakeRequest
PATCH  /api/bakes/{id}/notes                   Update bake notes → 204
GET    /api/bakes/{id}/export?format=csv|json  Export a single bake

POST   /api/steplogs/{id}/start                Start or resume → BakeStepLogDto
POST   /api/steplogs/{id}/pause                Pause → BakeStepLogDto
POST   /api/steplogs/{id}/complete             Complete → BakeStepLogDto
PATCH  /api/steplogs/{id}/duration?deltaMin=N  Adjust PlannedDurationMin → BakeStepLogDto
POST   /api/steplogs/{id}/measurements         Add measurement → MeasurementDto (201)

PUT    /api/bakes/{id}/outcome                 Upsert BakeOutcome (fields) → 204
POST   /api/bakes/{id}/outcome/photo           Upload outcome photo (multipart) → { url } (201)

GET    /api/grains/comparison                  Aggregated outcomes by grain → GrainComparisonDto[]
```

### Planned (M11–M19)

```
PATCH  /api/steplogs/{id}/notes                Update step-level notes → 204                  (M11)

GET    /api/starters                           List starters → StarterDto[]                   (M13)
POST   /api/starters                           Create starter → StarterDto (201)              (M13)
GET    /api/starters/{id}/feeds                List feed log entries → StarterFeedDto[]       (M13)
POST   /api/starters/{id}/feeds                Log a feed entry → StarterFeedDto (201)        (M13)

GET    /api/recipes                            List user-defined recipes → RecipeDto[]        (M14)
POST   /api/recipes                            Create user recipe → RecipeDto (201)           (M14)
PUT    /api/recipes/{id}                       Update user recipe → 204                       (M14)
DELETE /api/recipes/{id}                       Delete user recipe → 204                       (M14)

GET    /api/analytics/correlations             Scatter data: outcome vs factor → point[]     (M15)
GET    /api/analytics/personal-bests           Best score per grain per metric → summary[]   (M15)

POST   /api/calculators/scale                  Baker's % scaling → ScaleResult               (M19 ✅)
POST   /api/calculators/batch                  Batch scaling with yield/loss → BatchResult   (M19 ✅)
POST   /api/calculators/ddt                    DDT water temperature → DdtResult             (M19 ✅)
POST   /api/calculators/hydration              Levain split & true hydration → HydrationResult (M19 ✅)
POST   /api/calculators/cost                   Cost per loaf → CostResult                    (M19 ✅)
POST   /api/calculators/roux                   Tangzhong/Yudane roux fold → RouxResult       (M19 ✅)

GET    /api/kit/preheat?ovenType=&surface=     Preheat time recommendation → PreheatResult  (M21)
```

All `/api/calculators/*` endpoints are stateless pure-math operations — no database access, no auth, no EF context. Backed by a single `CalculatorService` on the server holding the C# functions from baker's guide §48 and §54.

All endpoints return `DateTimeOffset` values in ISO 8601 UTC. The client formats them for display using the browser's local timezone.

---

## Advisor → bake step mapping

When the client calls `POST /api/bakes`, it sends `StartBakeRequest` containing the current `BreadInputs`. The server's `BakeSessionService.CreateFromInputsAsync`:

1. Resolves the matching `Recipe` (by grain + method) from the database
2. Loads its `RecipeStep` rows (ordered)
3. Creates a `Bake` row (ambient conditions from `BreadInputs`)
4. Creates one `BakeStepLog` per step with `PlannedDurationMin = RecipeStep.DefaultDurationMin`

The server holds the step-generation and default-duration logic. The client does not send a step list — it sends inputs, and the server generates the steps from the seeded recipe. This keeps step defaults authoritative and versioned in the database.

**Autolyse vs. fermentolyse:** The step name changes ("Autolyse rest" vs. "Fermentolyse rest"), and fermentolyse skips the separate "Add salt + starter" step (salt and starter go in at mix time). This is handled by having two seeded `Recipe` rows per grain: one per method.

**Cold retard fermentolyse (baker's guide §16):** When `bake.AmbientTempC < 13`, `BakeSessionService` should treat the fermentolyse rest as a cold retard — substantially longer (hours, not minutes) and producing sharper acetic acidity. Baker's guide §16.5 provides a full per-degree reference table from 5 °C to 26 °C: at 5 °C the fermentation activity is ~19% of its 26 °C baseline (time multiplier 5.2×, acetic share 75%); at 13 °C it reaches ~36% (2.8×, 39%). At fridge temperature the yeast is dormant but bacteria and enzymes continue, making this a deliberate flavour tool, not simply a slower version of the same process. See DEVELOPMENT.md § M1 for the full per-degree table and the four practical working bands.

**Preferments (baker's guide §19.4):** A sourdough levain is the primary preferment in this app. If commercial-yeast preferments (Poolish, Biga, Pâte fermentée) are added in future, a `PrefermentType` enum on `Recipe` distinguishes them; the rest step and mix step names would change accordingly. This is deferred beyond M16.

---

## Persistence

**Development:** SQLite, file `bread-making.db` in the `Server/` directory (`.gitignore` excludes it).

**Production:** Any EF Core provider via `ConnectionStrings:Default` in `appsettings.json`. The entities use only standard EF features; switching providers requires only a package swap and migration regeneration.

**Migrations** are managed from the `Server` project:

```
dotnet ef migrations add <Name>  --project BreadMaking.App.Server
dotnet ef database update        --project BreadMaking.App.Server
```

---

## Phase 2 addition — SignalR (M7, implemented)

A SignalR hub on the server provides cross-device reminders. Each active bake has a group named `bake-{id}`; all devices joined to that bake receive `FoldsReminder`, `Bulk50Crossed`, and `StepCompleted` events. The hub is at `/hubs/bake`. This is purely additive — no existing components changed.

---

## Phase 3 addition — Offline (M16, planned)

The Blazor WASM template already generates `service-worker.js` and `service-worker.published.js` but they are currently no-ops. M16 wires them up:

- **Cache strategy:** The active bake page (`/bake/{id}`) and its API response are cached on first load. Subsequent loads use the cache when offline.
- **Offline action queue:** `wwwroot/js/offlineQueue.js` intercepts `POST /api/steplogs/{id}/start|pause|complete` calls when `navigator.onLine` is false. Actions are stored in IndexedDB with their URL and timestamp. On the `online` event, the queue is drained in order and each request is replayed.
- **Conflict resolution:** Replayed actions use the same endpoints as online actions. The server's start-time-as-source-of-truth design means elapsed is correctly derived even if the POST was delayed by a few seconds.
- **UI:** `LiveBake.razor` checks `navigator.onLine` on mount and subscribes to `online`/`offline` window events; an amber offline banner appears and disappears automatically.

---

## What does not change

- `BreadAdvisorService` — stays on the client, pure logic, no modifications needed
- All existing components (`ExperiencedForm`, `NoviceWizard`, `GrainBlendSelector`, etc.) — unchanged
- `BakingTimeline.razor` — kept as the static planning preview; the new `LiveBake.razor` page handles execution
- `bakery.css` — the design system is extended (new step-state classes), not replaced
- The existing in-memory `GrainProfile` and `GrainCatalogue` types — still used by the advisor on the client
