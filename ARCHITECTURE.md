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
- New pages: `LiveBake.razor` (`/bake/{id}`), `History.razor` (`/history`), `GrainComparison.razor` (`/history/compare`)
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
- No EF Core, no ASP.NET Core, no Blazor references — plain `net10.0` class library

---

## Data model

Seven EF Core entities live on the server only. The client always communicates via DTOs from `Shared`.

```
GrainProfile (1) ──< Recipe (1) ──< RecipeStep
                        │
                     Bake (1) ──< BakeStepLog (1) ──< Measurement
                        │
                     BakeOutcome (0..1)
```

Colour coding from the spec ER diagram:
- **Brown** (GrainProfile, Recipe, RecipeStep) — recipe definition, the template
- **Tan** (Bake, BakeStepLog) — a specific bake run, what actually happened
- **Amber** (Measurement, MeasurementType, BakeOutcome) — captured data

Full entity definitions are in the spec (Part C, section C1). Key properties:

| Entity | Key fields |
|--------|-----------|
| `Recipe` | `Method` (autolyse/fermentolyse/other), `GrainProfileId`, `TargetDoughTempC`, `FrictionFactorC` |
| `RecipeStep` | `Order`, `Name`, `Phase`, `DefaultDurationMin`, `MinDurationMin`, `MaxDurationMin`, `StepMin`, `TargetTempC` |
| `Bake` | `RecipeId`, `StartedAt`, `EndedAt`, `AmbientTempC`, `AmbientHumidityPct`, `FlourBatch`, `Notes` |
| `BakeStepLog` | `PlannedDurationMin`, `StartedAt`, `EndedAt`, `Status` (enum), `ActualDurationMin` (derived) |
| `Measurement` | `BakeStepLogId`, `MeasurementTypeId`, `Value`, `Unit`, `RecordedAt` (server-stamped) |
| `BakeOutcome` | `LoafHeightCm`, `OvenSpringPct`, `InternalTempC`, `WeightLossPct`, `CrumbOpenness`, `CrustScore`, `TasteScore` |

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

```
POST   /api/bakes                              Create Bake from BreadInputs → BakeDto (201)
GET    /api/bakes/{id}                         Load bake with all step logs and measurements → BakeDto
GET    /api/bakes                              Paginated history list → BakeListItemDto[]

POST   /api/steplogs/{id}/start                Start or resume → BakeStepLogDto
POST   /api/steplogs/{id}/pause                Pause → BakeStepLogDto
POST   /api/steplogs/{id}/complete             Complete → BakeStepLogDto
PATCH  /api/steplogs/{id}/duration?deltaMin=N  Adjust PlannedDurationMin → BakeStepLogDto

POST   /api/steplogs/{id}/measurements         Add measurement → MeasurementDto (201)
GET    /api/steplogs/{id}/measurements         List measurements for a step → MeasurementDto[]

POST   /api/bakes/{id}/outcome                 Record BakeOutcome → BakeOutcomeDto (201)
GET    /api/bakes/{id}/outcome                 Read BakeOutcome → BakeOutcomeDto

GET    /api/grains/comparison                  Aggregated outcomes by grain → GrainComparisonDto[]
GET    /api/bakes/{id}/export?format=csv|json  Export a single bake
```

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

## Phase 2 addition — SignalR (M7, optional)

The architecture reserves a SignalR hub on the server for cross-device reminders. Each active bake has a group named `bake-{id}`; all devices joined to that bake receive step-completed and threshold-crossed events. This is purely additive — no existing components need to change.

---

## What does not change

- `BreadAdvisorService` — stays on the client, pure logic, no modifications needed
- All existing components (`ExperiencedForm`, `NoviceWizard`, `GrainBlendSelector`, etc.) — unchanged
- `BakingTimeline.razor` — kept as the static planning preview; the new `LiveBake.razor` page handles execution
- `bakery.css` — the design system is extended (new step-state classes), not replaced
- The existing in-memory `GrainProfile` and `GrainCatalogue` types — still used by the advisor on the client
