# Design guide

This document covers the visual design system, the new screens and components introduced by the measurement roadmap, and the interface changes to existing components. It is a companion to ARCHITECTURE.md (what is built) and DEVELOPMENT.md (how it is built).

---

## Existing design system

The app uses a hand-crafted bakery theme (`wwwroot/css/bakery.css`).

### Palette (CSS custom properties)

| Variable | Value | Role |
|----------|-------|------|
| `--bg` | `#f9f3ea` | Page background (warm cream) |
| `--panel` | `#ffffff` | Card / panel surface |
| `--border` | `#e8ddd0` | Borders, dividers, muted elements |
| `--accent` | `#b5651d` (amber-brown) | Primary interactive colour, running state |
| `--text` | `#3d2b1a` | Body text (warm dark brown) |
| `--radius` | `12px` | Standard card corner radius |

### Temperature zone colours (already in use)

| Zone | Intended meaning | Repurposed step-state use |
|------|-----------------|--------------------------|
| `--zone-cold` | Blue-grey | — |
| `--zone-cool` | Steel blue | Paused state accent |
| `--zone-ideal` | Olive green | Completed state, expected-range hint |
| `--zone-warm` | Amber | Running state (same as `--accent`) |
| `--zone-hot` | Terracotta | Overrun warning |

New step-state colours reuse this existing set — no new palette variables are needed.

### Typography

- **Headings:** Playfair Display (serif) — craft feel, used for step names, measurement values
- **Body:** Lato — clean, readable, used for descriptions, metadata, hint text
- **Elapsed readout:** system monospace or `font-variant-numeric: tabular-nums` on a proportional font — prevents layout shift as digits change

---

## Step card states

Cards for each baking step change appearance with their `StepStatus`.

| State | Colour accent | Pill label | Layout |
|-------|--------------|------------|--------|
| `NotStarted` | `--border` (muted) | none | Compact single row: step name, default duration, temperature |
| `Running` | `--accent` (amber) | `● RUNNING` (amber pill) | Expanded: timer, progress bar, ± stepper, Pause + Done, measurements |
| `Paused` | steel blue (`--zone-cool`) | `⏸ PAUSED` (blue pill) | Expanded: timer frozen, Resume replaces Pause |
| Overrun (derived) | terracotta (`--zone-hot`) | stays `● RUNNING` + dashed border | Soft warning overlay; baker still presses Done |
| `Completed` | olive green (`--zone-ideal`) | `✓ DONE` (green pill) | Compact: step name, actual duration in green |
| `Skipped` | `--border` | `— SKIPPED` (muted pill) | Compact single row |

Overrun is not a `StepStatus` enum value — it is a display class applied when `status == Running && elapsed > plannedDuration`.

---

## New screens

### `/bake/{id}` — Live bake page

The main execution view. This is where the baker spends the majority of bake time.

**Before any step is started (planning state):**

```
┌──────────────────────────────────────────────────┐
│  ← Back    Spelt Autolyse · 2 June 2026          │
│  Kitchen 22 °C  ·  Batch mill-260601  ·  [Edit]  │
├──────────────────────────────────────────────────┤
│  STEP TIMELINE                                   │
│  [Planning Gantt — horizontal bar chart]         │
│  Each bar = default duration, thin line = range  │
├──────────────────────────────────────────────────┤
│  ○  Mix flour + water          5 min             │
│  ○  Autolyse rest             30 min             │
│  ○  Add salt + starter         5 min             │
│  ○  Bulk fermentation          5 h               │
│  ○  ...                                          │
│                                                  │
│  [Start first step ▶]                            │
└──────────────────────────────────────────────────┘
```

**While baking (active state):**

```
┌──────────────────────────────────────────────────┐
│  ← Back    Spelt Autolyse · 2 June 2026          │
│  Kitchen 22 °C  ·  Batch mill-260601             │
├──────────────────────────────────────────────────┤
│  ✓ DONE  Mix flour + water              5 min    │
│  ✓ DONE  Autolyse rest                 32 min    │
│  ✓ DONE  Add salt + starter             6 min    │
├──────────────────────────────────────────────────┤
│  ● RUNNING  Bulk fermentation  [expanded card]   │
├──────────────────────────────────────────────────┤
│  ○  Pre-shape + bench                  30 min    │
│  ○  ...                                          │
└──────────────────────────────────────────────────┘
```

Only the active step is expanded. Completed steps collapse to a single row showing actual duration. Not-started steps are muted rows.

---

### Step card — running state (from mockup `06_gui_step_card.png`)

```
┌──────────────────────────────────────────────────┐
│  ● RUNNING   Bulk fermentation                   │
│  ⏱ 4–6 hours (default 5h)   🌡 22–24 °C         │
│  4–6 sets of stretch & folds every 30 min.       │
│  Dough should grow 50–75%.                       │
│                                                  │
│  02:47:12          elapsed / 05:00:00 planned    │
│  ████████████████░░░░░░░░░░░░░░░░░░░░░           │
│                                                  │
│  ADJUST DURATION                                 │
│  [−]   4h 30m   [+]        ±15 min steps         │
│                             [Pause]  [Done ✓]    │
├──────────────────────────────────────────────────┤
│  MEASUREMENTS LOGGED THIS STEP                   │
│  [Dough temp  25.2 °C]  [Aliquot rise  48 %]    │
│  [pH  4.6]              [+ Add]                  │
│  Tap a chip to edit · values are auto-timestamped│
└──────────────────────────────────────────────────┘
```

Key design rules (derived from the mockup):

- Elapsed time is large (heading-scale Playfair Display), always visible while running
- Progress bar: `--accent` amber fill on a `--border` track; transitions to dashed terracotta when overrun
- ± stepper shows the step's increment label ("±15 min steps"); value is clamped to `[MinDurationMin, MaxDurationMin]`
- Measurement chips are compact rounded cards — tap to edit, `+ Add` is always the last chip
- The two action buttons (Pause, Done) are right-aligned, distinct weights (Pause = secondary/outline, Done = primary/filled green)

---

### Measurement entry sheet (from mockup `10_gui_measurement_entry.png`)

A bottom sheet (modal drawer), not a full page navigation.

```
┌──────────────────────────────────────────────────┐
│  ━━━━ (drag handle)                              │
│  Log measurement                                 │
│  Bulk fermentation · timestamped to this step    │
│                                                  │
│  TYPE                                            │
│  [■ Dough temp]  [Aliquot rise]  [pH]  [TTA]    │
│  [+ Custom]                                      │
│                                                  │
│  VALUE                                           │
│  [−]        25.2        °C       [+]             │
│                                                  │
│  ┌─ Expected for this phase ──────────────────┐  │
│  │  Dough temp 22–26 °C                       │  │
│  │  Warmer ≈ faster fermentation (×2 / +9 °C) │  │
│  └────────────────────────────────────────────┘  │
│                                                  │
│  [       Cancel       ]  [  Save measurement  ]  │
└──────────────────────────────────────────────────┘
```

Design rules:

- Type chips: selected chip uses `--accent` amber fill; unselected use `--border` outline
- Value display: large Playfair Display numeral, unit in body text to the right
- ± step increments: 0.5 for temperature, 1 for rise %, 0.1 for pH, 1 for TTA
- Expected-range panel: green tint (`--zone-ideal` at low opacity) when value is in range; terracotta tint when out of range
- Save button: dark filled (matches Done button on step card); Cancel: outline

---

### `/history` — Bake history list

```
┌──────────────────────────────────────────────────┐
│  Bake history                                    │
│  [All grains ▾]  [All methods ▾]   [Compare ▶]  │
├──────────────────────────────────────────────────┤
│  🌾 Spelt Autolyse    2 Jun 2026                 │
│     Oven spring 70%  ·  Crumb 7/10  ·  ✓        │
│     [View]  [Start similar]                      │
│  🌾 Einkorn Fermentolyse  28 May 2026            │
│     Oven spring 45%  ·  Crumb 4.5/10  ·  ✓      │
│     [View]  [Start similar]                      │
│  ...                                             │
└──────────────────────────────────────────────────┘
```

- Grain emoji / icon from the existing `GrainCatalogue` data (the `Icon` property already defined)
- Filter chips match the style of existing flour-type chips
- "Start similar bake" clones the bake's `BreadInputs` and opens the advisor with them pre-filled

---

### `/history/compare` — Grain comparison (from mockup `04_grain_comparison.png`)

Three side-by-side ApexCharts bar charts:

| Panel | Metric | Bar colour |
|-------|--------|-----------|
| Left | Oven spring (% height gain) | `--accent` amber |
| Centre | Crumb openness (0–10) | `--zone-ideal` olive green |
| Right | Time to 50% rise (hours) | `--zone-cool` steel blue |

Grains on the x-axis; bars computed from all logged `BakeOutcome` records. A placeholder grey bar with "Log more bakes" label is shown for grains with fewer than 3 bakes.

---

## Chart specifications

All charts use `ApexCharts.Blazor`. They should match the bakery palette and feel native to the app.

| Chart | Type | Key colours | Location |
|-------|------|-------------|----------|
| Live rise curve | Line | Three reference lines (amber / olive / steel); shaded 50–75% band | Bulk step while running |
| Planning Gantt | Horizontal bar | `--accent` solid bar; `--border` thin range extension | Pre-bake on `/bake/{id}` |
| Run chart | Line | `--accent` line with open circle markers; dashed `--zone-ideal` target line | `/history`, single metric |
| Grain comparison | Grouped bar | Amber / olive / steel blue per panel | `/history/compare` |

**Global chart theme:** Page background `--bg`, axis labels in `--text` at 60% opacity, faint `--border` horizontal grid lines only, tooltips in `--panel` with a `--border` shadow.

---

## Changes to existing components

### `RecommendationPanel.razor`

Add below the existing recommendation output:

- A **Start Bake** button (full-width, primary amber fill, Playfair Display label "Start Bake ▶")
- A sub-label in body text: "Records your actual durations and measurements"
- Disabled + tooltip when `RestMethod == Skip` (no timeline steps to run)

### `BakingTimeline.razor`

The existing static timeline becomes the **planning preview** — it is kept as-is and shown before the baker starts a session. No live timer logic goes here.

Add a **Start Bake** CTA at the bottom of the timeline, visually consistent with the one on `RecommendationPanel.razor`. Both buttons call the same action (the one on `RecommendationPanel` may be sufficient; include the timeline one for discoverability).

---

## Design rules for new components

1. **No new colour variables.** Map all new states to existing palette variables.
2. **Consistent card elevation.** Step cards use the same `--panel` background + `--border` border + `--radius` corner pattern as existing flour and starter cards.
3. **Touch-first tap targets.** ± stepper buttons and Pause / Done must be at least 44 × 44 px.
4. **Progressive disclosure.** A NotStarted card is one compact row (~56 px). Only the active card is expanded. Completed cards collapse, showing actual duration in `--zone-ideal` green.
5. **Monospace digits.** Use `font-variant-numeric: tabular-nums` on the elapsed readout so the layout does not shift as digits increment.
6. **Accessibility.** Each step card has an `aria-label` including step name and current status. The progress bar uses `role="progressbar"` with `aria-valuenow` / `aria-valuemax`.
