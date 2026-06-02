# Measuring the Baking Process: Research & Implementation Plan

*A comprehensive guide to adding step timing and measurement capture to an existing .NET/Razor baking application — for both the baker and the developers.*

**Version 1.0 · 2026-06-01 · Companion to *Autolyse vs. Fermentolyse — A Professional Baker's Guide* (v3)**

---

## 0. Executive summary

This document answers one question in two halves: *why* measure a bake, and *how* to build that measurement into the existing **Baking Timeline** screen.

The research half (Part A) establishes what is worth measuring and why. A loaf is the output of a controlled fermentation, and the same recipe produces wildly different bread depending on dough temperature, fermentation progress, and environment. A small set of well-chosen measurements — taken at the right step and stored against that step — turns a "happy accident" into something repeatable, diagnosable, and comparable across grains and methods.

The build half (Parts B–C) turns that into a plan for the application. The existing timeline (each step with a default duration and temperature) becomes interactive: every step can be **shown, started, paused, completed, and nudged up or down from its default duration**, and at any step the baker can record a measurement (dough temperature, rise %, pH, and so on) that is timestamped to that step automatically. The work is additive — new database tables seed their defaults from your current recipe, and nothing existing is rewritten.

All ten of the motivations you confirmed are mapped to concrete features in the traceability matrix in the appendix.

---

# Part A — Research: why and what to measure

## A1. From motivation to data

You confirmed all ten reasons for measuring. Each one implies that specific data must exist somewhere. The table below is the bridge between intent and schema — it is the reason each field in the data model (Part C) exists.

| # | Motivation | What it requires you to capture |
|---|------------|---------------------------------|
| 1 | Reproducibility | Every input and the *actual* (not planned) duration and temperature of each step |
| 2 | Troubleshooting | A per-step trail so a bad loaf can be traced to the step that caused it |
| 3 | Optimization | One changed variable per bake, with the outcome recorded against it |
| 4 | Understanding the process | In-process readings (rise %, pH) that reveal what fermentation is doing |
| 5 | Ancient-grain comparison (ancient↔ancient and ancient↔standard) | The same measurement set across bakes that differ only by grain |
| 6 | Controlling for environment | Ambient temperature and humidity, flour batch, starter activity per bake |
| 7 | Scaling & sharing | Baker's percentages and a complete, exportable record others can follow |
| 8 | Comparison across methods | Method tag (autolyse vs. fermentolyse) plus identical outcome metrics |
| 9 | Building a record over time | Persistent, queryable history of every bake |
| 10 | End-result quality criteria | Defined objective outcomes (height, spring, internal temp) *and* subjective scores (taste, crumb) |

The recurring pattern is important: **the value of a measurement comes almost entirely from being attached to a step and a bake, and from being compared.** A lone "pH 4.2" means little. "pH 4.2 at the end of bulk, on bake #14, Kamut, 24 °C kitchen" is data you can learn from.

## A2. What actually changes a loaf — the measurable variables

It helps to group the measurable quantities into four families, because each family lives at a different point in the timeline and answers a different question.

**Inputs (set before the bake).** Flour weight, water weight and therefore hydration, salt %, starter/leaven %, and the *desired dough temperature* you are aiming for. These are the levers you choose.

**Environment (the quiet variables).** Kitchen/ambient temperature and humidity, the flour batch or mill date, and the activity of the starter on the day. Your own guide makes the point that kitchen temperature is the single most important variable in both autolyse and fermentolyse — yet it is the one most often left unrecorded. Capturing it is what lets you explain why "the same recipe" behaved differently in July and January.

**In-process (measured during the bake).** The dough temperature right after mixing, the **percentage rise** during bulk fermentation, and optionally **pH** or **TTA** as acidity builds. These tell you, in real time, where fermentation actually is — independent of the clock.

**Outcomes (measured at and after the bake).** The **internal temperature** at the end of the bake, the finished loaf height and oven spring, bake (weight) loss, and the subjective scores you care about: crumb openness, crust, flavour. These define "the end result."

## A3. The key measurement methods

These five methods are well established in both home and professional baking and are exactly the ones the application should make easy. Each maps to a field in the data model and, where useful, to a chart in Part B.

### Desired Dough Temperature (DDT) and the friction factor

The temperature of the dough when mixing finishes governs how fast it ferments — roughly, **fermentation speed doubles for about every 9 °C (17 °F) increase in dough temperature.** Professional bakers therefore aim for a target dough temperature (commonly **24–26 °C / 75–78 °F** for wheat sourdough) and hit it by adjusting the one input they can easily control: water temperature.

The standard formula multiplies the target by the number of temperature factors and subtracts the ones you can't change:

```
Without a preferment (3 factors):
    Water temp = (3 × DDT) − (Flour temp + Room temp + Friction factor)

With a sourdough preferment (4 factors):
    Water temp = (4 × DDT) − (Flour temp + Room temp + Preferment temp + Friction factor)
```

The **friction factor** is the heat added by mixing. It is not universal — it depends on your method and must be calibrated by experiment. Common starting estimates are small for hand mixing and substantially larger for a stand mixer or food processor; sources differ, which is the point — *you measure your own.* The app can both store your calibrated friction factor and compute the water temperature for you.

> Capturing the actual post-mix dough temperature each bake, against the water temperature you used, is how the friction factor stops being a guess.

### The aliquot jar and percentage rise

Clock time is a poor guide to bulk fermentation because it varies with temperature, starter strength, and flour. The **aliquot method** replaces time with something you can see: a small sample (typically 20–50 g) of the mixed dough is placed in a straight-sided jar, the start height is marked, and the sample ferments alongside the main dough. The rise is read directly:

```
Rise % = ((Current height − Initial height) / Initial height) × 100
```

Target rise depends on dough temperature; a widely used guide is roughly **30% rise at ~27 °C, 50% at ~25 °C, and 75% at ~23 °C**, with many wheat sourdoughs shaped in the 50–75% band. The figure below shows why warmer dough reaches the target band sooner — and why recording dough temperature and rise together is far more informative than either alone.

![Dough rise curves at three dough temperatures, with a 50–75% target band](images/01_dough_rise_curve.png)

*Figure 1 — Aliquot-jar % rise versus time at three dough temperatures. The shaded band is the typical shaping window; warmer dough crosses it sooner. This is the live readout the app shows during the bulk-fermentation step.*

### pH and total titratable acidity (TTA)

For bakers who want to go deeper, acidity is the most direct read on fermentation chemistry. As fermentation proceeds, **pH falls** (a dough may move from around 6 toward 3.8–4.3) and **acid accumulates**. pH is cheap to measure with a probe and matters because it gates enzyme activity — amylase, for example, works best around **pH 5.4–5.8** and slows below that. TTA (the amount of base needed to neutralise the acids) tracks total acid and therefore sour-flavour strength more reliably than pH, because the weak acids in dough barely move the pH reading; the trade-off is that TTA needs titration equipment and is impractical mid-bake for most bakers. A useful flavour rule of thumb is a **lactic-to-acetic acid ratio of about 3:1 to 4:1** — more acetic and the bread tastes too sharp.

![pH falling and TTA rising over fermentation time, with the amylase optimum band marked](images/02_ph_tta_curve.png)

*Figure 2 — Acidification over fermentation. pH (left axis) falls while TTA (right axis) rises. The app treats pH as a quick optional reading and TTA as an advanced one — both stored as ordinary measurements.*

### Internal temperature (doneness)

The single most reliable doneness signal is the loaf's internal temperature, read with an instant-read probe in the centre. For lean doughs (flour, water, salt, leaven — i.e. most sourdough and ancient-grain hearth loaves) the target is about **96–98 °C (205–210 °F)**; enriched doughs are pulled a little lower to stay tender, and dense whole-grain or rye loaves a little higher. A caveat worth surfacing in the app: a correct internal temperature confirms the bake is *cooked through*, but it cannot rescue an under-fermented dough, which will still read "done" yet bake gummy.

### Baker's percentage and bake loss

Two derived numbers make bakes comparable across batch sizes and grains. **Baker's percentage** expresses every ingredient relative to flour (flour = 100%), so hydration, salt, and starter are directly comparable whatever the batch size — essential for scaling and sharing. **Bake loss** is the weight lost in the oven (a finished lean loaf typically weighs around 1.4–1.5× the flour weight); tracking it flags under- or over-baking independent of colour.

## A4. Ancient grains — what to measure differently

Your guide already establishes *why* the six grains behave differently: within the wheat family it is the gliadin-to-glutenin ratio (ancient wheats are gliadin-rich, so extensible but weak and fast-fermenting), and Teff and Sorghum have no gluten at all and are worked as batters with a binder. For a measurement system, the consequence is that the *same* metrics are captured, but their *expected ranges and targets* differ by grain. That difference is exactly what a comparison study sets out to quantify.

The handling parameters from your guide become per-grain defaults the app can seed and then refine from real data:

| Grain | Gluten | Hydration vs. modern wheat | Autolyse rest | Proof speed |
|-------|--------|----------------------------|---------------|-------------|
| Modern bread wheat | Strong | baseline | 20–45 min | baseline |
| Einkorn | Very weak | −10 to −20% | skip / ≤15 min | very fast |
| Emmer | Weak | −5 to −15% | ≤20 min | fast |
| Spelt | Moderate (fragile) | similar to −5% | 20–30 min | fast |
| Kamut | Strong | +5 to +15% | 30–45 min | slightly slow |
| Teff / Sorghum | None | batter (soaker, +binder) | 20–60 min soaker | n/a |

The payoff of logging is being able to put measured outcomes side by side. The chart below is an example of what the app would show after a small comparison study — the kind of head-to-head that supports both your ancient↔ancient and ancient↔standard goals.

![Measured oven spring, crumb openness and time-to-rise across five grains](images/04_grain_comparison.png)

*Figure 3 — Example logged outcomes by grain. Once enough bakes exist, these bars are computed from real measurements rather than estimates, and the per-grain defaults above can be replaced by your own numbers.*

## A5. Treating a bake as an experiment

Three principles make the difference between "collecting numbers" and learning something, and they should shape both how you bake and how the app nudges you.

**Change one variable at a time.** If you alter the grain, the hydration, and the bulk time all at once, the outcome tells you nothing about cause. The app can support this by letting you clone a previous bake and flag the single field you intend to change.

**Record the environment, not just the recipe.** Ambient temperature and humidity, flour batch, and starter activity are the hidden variables that make "the same recipe" behave differently. They are quick to capture and disproportionately valuable.

**Define outcomes before you bake.** Decide which objective metrics (height, oven spring, internal temp, bake loss) and which subjective scores (crumb openness, crust, flavour) you will record, and capture them every time. Consistency of *what* you measure is what makes the history comparable.

The reward for doing this over time is convergence: as the record grows, guesswork shrinks and results cluster on your target.

![Run chart of loaf height across successive bakes converging on target](images/05_bake_run_chart.png)

*Figure 4 — A run chart of a single outcome (loaf height) across logged bakes. Early bakes scatter; as the record informs adjustments, results converge on the target and the spread narrows. This is the long-term dividend of motivation #1 and #9.*

---

# Part B — Visualizing measurements in the application

Each chart in Part A is also a screen (or screen element) in the app. Mapping them explicitly keeps the build focused on visuals that earn their place rather than decoration.

| Visualization | Where it lives | Driven by |
|---------------|----------------|-----------|
| Live rise curve (Fig. 1) | Inside the **Bulk fermentation** step, while running | Aliquot rise % measurements + dough temp |
| pH / TTA curve (Fig. 2) | Optional "acidity" panel on fermentation steps | pH / TTA measurements |
| Step timeline / Gantt (Fig. 5, below) | Bake overview, planning view | RecipeStep default & adjustable durations |
| Grain comparison (Fig. 3) | History → "Compare" view | Aggregated BakeOutcome across bakes |
| Run chart (Fig. 4) | History → single-metric trend | One outcome metric over time |

The timeline itself is worth visualizing before any timer runs, because it shows the baker the whole shape of the bake and exactly how much each step can be stretched or shortened.

![Gantt-style timeline showing each step's default duration and its adjustable range](images/03_step_gantt_timeline.png)

*Figure 5 — The full bake as a timeline. The solid bar is each step's default duration; the thin line is the adjustable range (the ± the baker can apply). This is the planning counterpart to the live, running timeline on the main screen.*

---

# Part C — Development plan (detailed steps)

This part is implementation-focused. It assumes the existing app is ASP.NET Core with Razor. Because "Razor" spans both **Razor Pages/MVC** (server-rendered views, interactivity via JavaScript) and **Blazor** (Razor components with C# interactivity), the timer and step-card sections give both paths and call out which to prefer.

## C0. Architecture overview

The change is a vertical slice through the existing layers, not a rewrite. The client gains timer state and a measurement sheet; the server gains a few endpoints and services; the data layer gains a handful of additive tables.

![Layered architecture diagram from UI through ASP.NET Core to the database](images/08_architecture.png)

*Figure 6 — Architecture. The browser holds only display state; the server is the source of truth for time. The optional SignalR hub (Phase 2) pushes cross-device reminders.*

The most important design decision is in the top-right box: **the server stores when a step *started*, not a ticking counter.** Elapsed time is always derived as `now − StartedAt`. This is what makes a 16-hour cold proof, a locked phone, or a refreshed tab resume correctly — the timer is a calculation, never a number being incremented in the browser.

## C1. The data model

![Entity-relationship diagram of the seven new/extended entities](images/07_data_model.png)

*Figure 7 — Data model. Brown = recipe definitions (templates), tan = a specific bake run, amber = captured measurements. `1 / ∗` denotes one-to-many.*

The model separates **definition** (a recipe and its steps — the template) from **execution** (a bake and its step logs — what actually happened), with measurements hanging off the step logs. The corresponding EF Core entities:

```csharp
public enum BakeMethod { Autolyse, Fermentolyse, Other }
public enum StepStatus { NotStarted, Running, Paused, Completed }

public class GrainProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";       // e.g. "Spelt"
    public string? Ploidy { get; set; }
    public string GlutenStrength { get; set; } = "";
    public double HydrationAdjustPct { get; set; } // e.g. -15 for emmer
    public int MaxAutolyseMinutes { get; set; }
    public bool NeedsBinder { get; set; }
}

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public BakeMethod Method { get; set; }
    public int? GrainProfileId { get; set; }
    public GrainProfile? GrainProfile { get; set; }
    public double TargetHydrationPct { get; set; }
    public double TargetDoughTempC { get; set; }   // DDT
    public double FrictionFactorC { get; set; }     // calibrated
    public List<RecipeStep> Steps { get; set; } = new();
}

public class RecipeStep
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = "";          // "Bulk fermentation"
    public string? Phase { get; set; }              // "Rest","Bulk","Bake"...
    public int DefaultDurationMin { get; set; }
    public int MinDurationMin { get; set; }
    public int MaxDurationMin { get; set; }
    public int StepMin { get; set; } = 5;            // ± increment, e.g. 15
    public double? TargetTempC { get; set; }
    public string? Description { get; set; }
}

public class Bake
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public double? AmbientTempC { get; set; }
    public double? AmbientHumidityPct { get; set; }
    public string? FlourBatch { get; set; }
    public string? Notes { get; set; }
    public List<BakeStepLog> StepLogs { get; set; } = new();
    public BakeOutcome? Outcome { get; set; }
}

public class BakeStepLog
{
    public int Id { get; set; }
    public int BakeId { get; set; }
    public int RecipeStepId { get; set; }
    public int PlannedDurationMin { get; set; }      // starts at default, editable by ±
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public StepStatus Status { get; set; } = StepStatus.NotStarted;
    public List<Measurement> Measurements { get; set; } = new();

    // Elapsed is DERIVED, never stored ticking:
    public TimeSpan Elapsed(DateTimeOffset now) =>
        StartedAt is null ? TimeSpan.Zero
        : (EndedAt ?? now) - StartedAt.Value;
}

public class MeasurementType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";           // "Dough temp"
    public string Unit { get; set; } = "";           // "°C","%","pH"
    public string Category { get; set; } = "";        // InProcess / Outcome
    public double? MinValid { get; set; }
    public double? MaxValid { get; set; }
    public string? DefaultForPhase { get; set; }
}

public class Measurement
{
    public int Id { get; set; }
    public int BakeStepLogId { get; set; }
    public int MeasurementTypeId { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
}

public class BakeOutcome
{
    public int Id { get; set; }
    public int BakeId { get; set; }
    public double? LoafHeightCm { get; set; }
    public double? OvenSpringPct { get; set; }
    public double? InternalTempC { get; set; }
    public double? WeightLossPct { get; set; }
    public int? CrumbOpenness { get; set; }          // 0–10
    public int? CrustScore { get; set; }
    public int? TasteScore { get; set; }
    public string? PhotoPath { get; set; }
}
```

## C2. Migration and seeding from the existing recipe

The new tables are added with a single EF Core migration, and your current timeline (the steps in the screenshot) becomes the seed for `RecipeStep` defaults. Nothing existing is overwritten.

```bash
dotnet ef migrations add AddTimingAndMeasurements
dotnet ef database update
```

Seed the measurement types and the step defaults so every recipe starts from the values already shown on the timeline:

```csharp
// MeasurementType seed (the manual-entry vocabulary)
modelBuilder.Entity<MeasurementType>().HasData(
  new { Id=1, Name="Dough temp",   Unit="°C", Category="InProcess", MinValid=10.0, MaxValid=40.0, DefaultForPhase="Mix" },
  new { Id=2, Name="Aliquot rise", Unit="%",  Category="InProcess", MinValid=0.0,  MaxValid=200.0, DefaultForPhase="Bulk" },
  new { Id=3, Name="pH",           Unit="pH", Category="InProcess", MinValid=3.0,  MaxValid=7.0,  DefaultForPhase="Bulk" },
  new { Id=4, Name="TTA",          Unit="mL", Category="InProcess", MinValid=0.0,  MaxValid=30.0, DefaultForPhase="Bulk" },
  new { Id=5, Name="Internal temp",Unit="°C", Category="Outcome",   MinValid=80.0, MaxValid=110.0,DefaultForPhase="Bake" }
);

// RecipeStep defaults seeded from the existing timeline, e.g. Bulk fermentation:
modelBuilder.Entity<RecipeStep>().HasData(
  new { Id=4, RecipeId=1, Order=4, Name="Bulk fermentation", Phase="Bulk",
        DefaultDurationMin=300, MinDurationMin=240, MaxDurationMin=360,
        StepMin=15, TargetTempC=23.0,
        Description="4–6 sets of stretch & folds every 30 min. Dough should grow 50–75%." }
);
```

## C3. Services and endpoints

Three small services keep the controllers thin and the timer logic in one place.

```csharp
public interface ITimerService
{
    Task<BakeStepLog> StartAsync(int bakeStepLogId);
    Task<BakeStepLog> PauseAsync(int bakeStepLogId);
    Task<BakeStepLog> CompleteAsync(int bakeStepLogId);
    Task<BakeStepLog> AdjustPlannedAsync(int bakeStepLogId, int deltaMinutes);
}

public class TimerService : ITimerService
{
    private readonly AppDbContext _db;
    public TimerService(AppDbContext db) => _db = db;

    public async Task<BakeStepLog> StartAsync(int id)
    {
        var log = await _db.BakeStepLogs.FindAsync(id)
                  ?? throw new KeyNotFoundException();
        // Resuming from Paused keeps accumulated time by shifting StartedAt:
        if (log.Status == StepStatus.Paused && log.StartedAt is not null)
            log.StartedAt = DateTimeOffset.UtcNow - log.Elapsed(DateTimeOffset.UtcNow);
        else
            log.StartedAt = DateTimeOffset.UtcNow;
        log.Status = StepStatus.Running;
        await _db.SaveChangesAsync();
        return log;
    }

    public async Task<BakeStepLog> PauseAsync(int id)
    {
        var log = await _db.BakeStepLogs.FindAsync(id)!;
        // freeze: store elapsed by setting EndedAt to now (interpreted as pause mark)
        log.EndedAt = DateTimeOffset.UtcNow;
        log.Status = StepStatus.Paused;
        await _db.SaveChangesAsync();
        return log;
    }

    public async Task<BakeStepLog> CompleteAsync(int id)
    {
        var log = await _db.BakeStepLogs.FindAsync(id)!;
        log.EndedAt = DateTimeOffset.UtcNow;
        log.Status = StepStatus.Completed;
        await _db.SaveChangesAsync();
        return log;
    }

    public async Task<BakeStepLog> AdjustPlannedAsync(int id, int delta)
    {
        var log = await _db.BakeStepLogs
            .Include(l => l.RecipeStep).FirstAsync(l => l.Id == id);
        var min = log.RecipeStep.MinDurationMin;
        var max = log.RecipeStep.MaxDurationMin;
        log.PlannedDurationMin =
            Math.Clamp(log.PlannedDurationMin + delta, min, max);   // ± controls
        await _db.SaveChangesAsync();
        return log;
    }
}
```

Controller endpoints (Razor Pages handlers or Web API actions — names match Figure 6):

```csharp
[ApiController, Route("api/steplogs")]
public class StepLogController : ControllerBase
{
    private readonly ITimerService _timer;
    private readonly IMeasurementService _measure;
    public StepLogController(ITimerService t, IMeasurementService m)
        { _timer = t; _measure = m; }

    [HttpPost("{id:int}/start")]    public Task<BakeStepLog> Start(int id)    => _timer.StartAsync(id);
    [HttpPost("{id:int}/pause")]    public Task<BakeStepLog> Pause(int id)    => _timer.PauseAsync(id);
    [HttpPost("{id:int}/complete")] public Task<BakeStepLog> Complete(int id) => _timer.CompleteAsync(id);

    [HttpPatch("{id:int}/duration")]
    public Task<BakeStepLog> Adjust(int id, [FromQuery] int deltaMin)
        => _timer.AdjustPlannedAsync(id, deltaMin);

    [HttpPost("{id:int}/measurements")]
    public Task<Measurement> AddMeasurement(int id, [FromBody] MeasurementDto dto)
        => _measure.AddAsync(id, dto);
}
```

`MeasurementService.AddAsync` validates the value against the `MeasurementType` range, stamps `RecordedAt = UtcNow`, and attaches it to the step log — which is what makes every reading automatically timestamped to its step.

## C4. The timer — server as source of truth

The timer follows a small, explicit state machine. The crucial rule (called out in the footnote of Figure 8) is that the **± duration controls change the *target* (`PlannedDurationMin`), never the elapsed time.** Overrun — running past the planned duration — is a soft warning, not a hard stop, because fermentation does not obey the clock.

![State machine: NotStarted → Running ↔ Paused → Completed, with an Overrun warning branch](images/09_timer_state_machine.png)

*Figure 8 — Per-step timer states and transitions.*

Because elapsed time is derived from `StartedAt`, the client only needs to *display* it. In **Blazor** this is a one-second `System.Timers.Timer` that re-renders the readout; in **Razor Pages** it is a few lines of JavaScript anchored to the server's `StartedAt`:

```javascript
// Razor Pages path: anchor the on-screen clock to the server start time.
function runStepTimer(el, startedAtIso, plannedMin) {
  const start = new Date(startedAtIso).getTime();
  const plannedMs = plannedMin * 60000;
  function tick() {
    const elapsed = Date.now() - start;          // derived, survives refresh
    el.querySelector('.elapsed').textContent = fmt(elapsed);
    const pct = Math.min(100, (elapsed / plannedMs) * 100);
    el.querySelector('.bar').style.width = pct + '%';
    el.classList.toggle('overrun', elapsed > plannedMs);   // soft warning
    requestAnimationFrame(() => setTimeout(tick, 1000));
  }
  tick();
}
```

For **Blazor**, prefer it for this feature: the same C# `BakeStepLog` model drives the UI, and (with Blazor Server) a single source of truth lives server-side already. The equivalent component holds `StartedAt`, ticks a timer to call `StateHasChanged()`, and posts to the same service methods on button click.

## C5. Adapting the GUI

The existing timeline card grows three capabilities — **a timer (show/start/pause/done), ± duration steppers, and a measurement strip** — without losing the look of the current screen. Here is the target state of a single running step:

![Step card mockup: RUNNING pill, timer readout, progress bar, ± duration stepper, Pause/Done buttons, measurement chips](images/06_gui_step_card.png)

*Figure 9 — The adapted step card. Compared with today's static row, it adds the live readout and progress bar, the `−  4h 30m  +` duration stepper (clamped to the step's min/max), Pause/Done actions, and a strip of measurement chips with an "+ Add" affordance.*

A condensed Razor markup for the card (server-rendered values, JS-driven clock):

```html
<div class="step-card @log.Status.ToString().ToLower()"
     data-steplog-id="@log.Id">
  <div class="step-head">
    @if (log.Status == StepStatus.Running) { <span class="pill running">● RUNNING</span> }
    <h3>@log.RecipeStep.Name</h3>
  </div>

  <div class="meta">
    <span class="clock-ico"></span> @FormatRange(log.RecipeStep) (default @Min(log.RecipeStep.DefaultDurationMin))
    <span class="temp-ico"></span> @log.RecipeStep.TargetTempC °C
  </div>
  <p class="desc">@log.RecipeStep.Description</p>

  <div class="timer">
    <span class="elapsed">00:00:00</span>
    <span class="planned">/ @Format(log.PlannedDurationMin) planned</span>
    <div class="track"><div class="bar"></div></div>
  </div>

  <div class="controls">
    <fieldset class="stepper">
      <legend>Adjust duration</legend>
      <button data-delta="-@log.RecipeStep.StepMin">−</button>
      <output>@Format(log.PlannedDurationMin)</output>
      <button data-delta="@log.RecipeStep.StepMin">+</button>
    </fieldset>

    <button class="btn primary" data-action="start">Start</button>
    <button class="btn" data-action="pause">Pause</button>
    <button class="btn done" data-action="complete">Done ✓</button>
  </div>

  <div class="measurements">…chips + “Add” button…</div>
</div>
```

```javascript
// Wire the buttons to the endpoints from C3.
card.querySelectorAll('.stepper button').forEach(b =>
  b.onclick = () => patch(`/api/steplogs/${id}/duration?deltaMin=${b.dataset.delta}`)
                      .then(render));
card.querySelector('[data-action=start]').onclick    = () => post(`/api/steplogs/${id}/start`).then(render);
card.querySelector('[data-action=pause]').onclick    = () => post(`/api/steplogs/${id}/pause`).then(render);
card.querySelector('[data-action=complete]').onclick = () => post(`/api/steplogs/${id}/complete`).then(render);
```

Manual measurement entry (your chosen capture method) opens a small sheet from the "+ Add" chip. It defaults to the measurement type appropriate for the current phase and shows the expected range as a gentle guide, so a value that looks wrong is caught at entry.

![Measurement entry sheet: type selector, large numeric value with ± steppers and unit, expected-range hint, Save/Cancel](images/10_gui_measurement_entry.png)

*Figure 10 — The manual measurement sheet. Type defaults by phase (here "Dough temp" for the mix step), the value has coarse ± steppers for touch use, and the green hint shows the expected range. On save it posts to `/api/steplogs/{id}/measurements` and appears as a chip on the card.*

## C6. Phased roadmap

| Milestone | Scope | Outcome |
|-----------|-------|---------|
| **M1 — Data & migration** | Entities, EF migration, seed types + step defaults from existing recipe | Schema in place; existing recipes unchanged |
| **M2 — Timer core** | `TimerService`, endpoints, state machine, server-derived elapsed | Steps can start/pause/complete reliably across refresh |
| **M3 — GUI: timing** | Step card with readout, progress bar, ± steppers wired to endpoints | The screenshot timeline becomes interactive |
| **M4 — Measurements** | `MeasurementService`, entry sheet, chips, validation | Manual readings captured and timestamped per step |
| **M5 — Visualization** | Live rise curve, planning Gantt, history run chart | Figures 1, 4, 5 rendered from real data |
| **M6 — Comparison & export** | Outcome capture, grain comparison view, CSV/JSON export | Figures 3–4; supports scaling, sharing, studies |
| **M7 (optional) — Notifications** | SignalR reminders, cross-device | "Start your folds", "bulk hit 50%" |

## C7. Validation, testing and edge cases

A few cases are worth designing for explicitly. **Long steps** (a 16-hour cold proof) must survive app restarts — covered by deriving elapsed from `StartedAt`. **Clock skew and time zones** are avoided by storing everything as `DateTimeOffset` UTC and formatting on display. **Out-of-range values** (a pH of 14, a dough temp of 250) are rejected against `MeasurementType.MinValid/MaxValid` at entry. **Pausing then adjusting duration** must change only the target, never the frozen elapsed — a unit test should assert that `AdjustPlannedAsync` leaves `StartedAt`/`EndedAt` untouched. **Skipped steps** (a recipe with no autolyse for einkorn) should allow marking a step "not applicable" rather than forcing a zero-length run.

## C8. Future extensions

The manual-entry foundation generalises cleanly. Bluetooth probes or a connected scale would simply create `Measurement` rows automatically instead of via the sheet — the schema does not change. A predictive layer could, once enough history exists, estimate "time to target rise" from current dough temperature and recent rise readings. And the same `BakeStepLog`/`Measurement` data is exactly what feeds the comparison and run-chart views, so analytics is a read over data you are already capturing rather than new instrumentation.

---

# Appendix

## A. Motivations → features traceability

| # | Motivation | Primary feature(s) | Figures |
|---|------------|--------------------|---------|
| 1 | Reproducibility | Actual per-step durations; full input capture; run chart | 4, 5 |
| 2 | Troubleshooting | Per-step measurement trail; status history | 7, 9 |
| 3 | Optimization | Clone-and-change-one-variable; outcome capture | 4 |
| 4 | Understanding | Live rise curve; pH/TTA panel | 1, 2 |
| 5 | Grain comparison | Per-grain defaults; comparison view | 3 |
| 6 | Environment control | Ambient temp/humidity, flour batch on `Bake` | 7 |
| 7 | Scaling & sharing | Baker's % ; CSV/JSON export | 7 |
| 8 | Method comparison | `Method` tag; identical outcome metrics | 3 |
| 9 | Record over time | Persistent `Bake` history; trends | 4 |
| 10 | Quality criteria | `BakeOutcome` objective + subjective fields | 7 |

## B. Measurement reference ranges (seedable defaults)

| Measurement | Typical target / range | Step / phase | Notes |
|-------------|------------------------|--------------|-------|
| Desired dough temp | 24–26 °C (75–78 °F) | Mix | Hit via water temp + friction factor |
| Dough temp (actual) | record every bake | Mix | Calibrates friction factor |
| Aliquot rise | 50–75% (temp-dependent) | Bulk | 30% @27 °C · 50% @25 °C · 75% @23 °C |
| pH | falls to ~3.8–4.3 | Bulk/proof | Amylase optimum 5.4–5.8 |
| TTA | rises through ferment | Bulk/proof | Advanced; needs titration |
| Internal temp | 96–98 °C (205–210 °F) lean | Bake | Lower for enriched; higher for dense rye |
| Bake loss | ~loaf ≈ 1.4–1.5× flour | Cool | Flags under/over-bake |

## C. Glossary

**Aliquot** — a small representative sample of the dough used to read rise %. **DDT** — desired dough temperature, the post-mix target. **Friction factor** — the heat a given mixing method adds to the dough; calibrated, not universal. **TTA** — total titratable acidity, the amount of base to neutralise dough acids. **Bake loss** — weight lost during baking. **Baker's percentage** — every ingredient expressed relative to flour (= 100%).

## D. Sources

- King Arthur Baking — Desired Dough Temperature and friction factor (kingarthurbaking.com)
- Wordloaf (Andrew Janjigian) — DDT formula; pH vs. TTA in sourdough (newsletter.wordloaf.org)
- Sourdoughtalk / Sourdough Archive — aliquot jar method and percentage rise (sourdoughtalk.com, sourdougharchive.com)
- Metrohm; American Society of Baking; BAKERpedia — pH and total titratable acidity in sourdough (metrohm.com, asbe.org, bakerpedia.com)
- "Online Monitoring of Sourdough Fermentation Using a Gas Sensor Array" — TTA/pH as fermentation variables (MDPI Sensors, 2023)
- The Pantry Mama; Rise & Sourdough; King Arthur Baking — internal doneness temperature (pantrymama.com, riseandsourdough.com)
- *Autolyse vs. Fermentolyse — A Professional Baker's Guide* v3 (your uploaded document) — ancient-grain handling parameters

*General baking figures (temperature ranges, rise targets, internal-temperature targets) are widely corroborated across the sources above and are presented as established practice rather than attributed to any single source.*
