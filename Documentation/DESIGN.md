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
- **Internal temp hint (Bake step):** When `MeasurementType == InternalTempC` and the step is in the Bake phase, append a target line to the expected-range panel: `"Target: ~96 °C lean / ~88 °C enriched"`. This context (from baker's guide §18) is hardcoded in the hint, not derived from the DB, since lean vs enriched is not currently a `Recipe` field.

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

**Grain profile card (M17):** Once `GrainProfile.FlavorNotes` / `NutritionHighlights` / `UsageNotes` are seeded, each grain bar on the comparison page should be tappable, opening a compact detail card:

```
┌──────────────────────────────────┐
│  🌾 Emmer                        │
│  Earthy · nutty · rustic         │
│  High fibre · iron · magnesium   │
│  30–50% in sourdough blends      │
│  Fertile Crescent, ~8000 BC      │
└──────────────────────────────────┘
```

Card uses `--panel` background, `--border` border, `--radius` corners. Flavour note in `--accent` italic; nutrition and usage lines in body Lato. Historical origin in muted `--border` colour at the foot.

---

## Chart specifications

All charts use `ApexCharts.Blazor`. They should match the bakery palette and feel native to the app.

| Chart | Type | Key colours | Location |
|-------|------|-------------|----------|
| Live rise curve | Line | Three reference lines (amber / olive / steel); shaded 50–75% band | Bulk step while running |
| Planning Gantt | Horizontal bar | `--accent` solid bar; `--border` thin range extension | Pre-bake on `/bake/{id}` |
| Run chart | Line | `--accent` line with open circle markers; dashed `--zone-ideal` target line | `/history`, single metric |
| Grain comparison | Grouped bar | Amber / olive / steel blue per panel | `/history/compare` |
| Starter activity | Line | `--accent` peak-hours trend per starter | `/starter` per-starter view |

**Global chart theme:** Page background `--bg`, axis labels in `--text` at 60% opacity, faint `--border` horizontal grid lines only, tooltips in `--panel` with a `--border` shadow.

---

### `/calculators` — Baker's calculators page (M19)

Six stateless calculators on one page. No data persisted; all results are ephemeral.

**Layout — desktop (≥768 px):**

```
┌──────────────────────────────────────────────────────────────────┐
│  ← Back    Baker's calculators                                   │
│  Derived from baker's guide §48 + §54                           │
├────────────────┬─────────────────────────────────────────────────┤
│  ■ Scaling     │  BAKER'S PERCENTAGE SCALING                     │
│  ○ Batch       │  Target dough weight  [______] g                │
│  ○ DDT         │                                                  │
│  ○ Hydration   │  Ingredient     Baker's %                       │
│  ○ Cost        │  Flour          100%                            │
│  ○ Water roux  │  Water          [72]%                           │
│                │  Levain         [20]%                           │
│                │  Salt           [ 2]%                           │
│                │  [+ Add ingredient]   [Calculate]               │
│                ├─────────────────────────────────────────────────┤
│                │  RESULT                                          │
│                │  Flour  457 g  ·  Water  329 g                  │
│                │  Levain  91 g  ·  Salt    9 g                   │
│                │  Total formula %: 194%  ·  Sum: 886 g ✓         │
└────────────────┴─────────────────────────────────────────────────┘
```

**Layout — mobile:** Tabs collapse to a horizontal scrolling chip row above the form area.

**DDT tab:**

```
┌──────────────────────────────────────────────────┐
│  DESIRED DOUGH TEMPERATURE                       │
│  Target DDT    [25]  °C                          │
│  Flour temp    [20]  °C                          │
│  Room temp     [22]  °C                          │
│  Mix method                                      │
│  ● Hand folds  ○ Hand knead  ○ Stand mixer       │
│  ○ Spiral      ○ Intensive   ○ Custom [__] °C    │
│  Preferment temp  [──]  °C  (leave blank if none)│
│                                                  │
│  [Calculate]                                     │
│                                                  │
│  ┌── Result ─────────────────────────────────┐   │
│  │  Water temperature: 31 °C                │   │
│  │  (3 factors: flour, room, water + 2°C    │   │
│  │   friction; add preferment for 4-factor) │   │
│  └───────────────────────────────────────────┘   │
└──────────────────────────────────────────────────┘
```

**Water-roux tab:**

```
┌──────────────────────────────────────────────────┐
│  WATER-ROUX FOLD (§54)                           │
│  Total flour     [500]  g                        │
│  Target hydration [ 70]  %                       │
│  Roux flour share [  6]  %                       │
│  Method  ● Tangzhong (1:5)  ○ Yudane (1:1)       │
│                                                  │
│  [Calculate]                                     │
│                                                  │
│  ┌── Result ─────────────────────────────────┐   │
│  │           Flour    Liquid                │   │
│  │  Roux      30 g    150 g                 │   │
│  │  Dough    470 g    200 g                 │   │
│  │  ─────────────────────────               │   │
│  │  Totals   500 g    350 g  ✓ (70%)        │   │
│  └───────────────────────────────────────────┘   │
│  Tangzhong: cook flour + liquid to ~65 °C        │
│  Yudane: scald with boiling water; rest overnight│
└──────────────────────────────────────────────────┘
```

Design rules for the calculators page:
- Input fields use the standard `--border` outline style; focused field has `--accent` amber border.
- Result cards use `--panel` background with a thin `--zone-ideal` left border to signal success.
- Error states (negative values, hydration > 200%, water temp below 0 °C) show a `--zone-hot` terracotta inline hint below the affected field — no toast or modal.
- "Totals check" rows use `--zone-ideal` text colour when balanced; `--zone-hot` if there is a rounding discrepancy > 1 g.
- Each calculator result card has a **Copy to clipboard** icon (📋) that serialises the result as plain text for pasting into notes.
- Navigation entry: add a **Calculators** link to the app nav alongside **History** and **Starter**.

---

## Changes to existing components

### `RecommendationPanel.razor`

Add below the existing recommendation output:

- A **Start Bake** button (full-width, primary amber fill, Playfair Display label "Start Bake ▶")
- A sub-label in body text: "Records your actual durations and measurements"
- Disabled + tooltip when `RestMethod == Skip` (no timeline steps to run)

### `/starter` page — health indicators

Each feed log entry on the `/starter` page carries a compact health chip derived from the free-form `Notes` field and from `PeakHours` vs `AmbientTempC`:

| Chip | Colour | Trigger |
|------|--------|---------|
| `🟢 Active` | `--zone-ideal` | `PeakHours` is set and within expected range for the recorded temp |
| `🟡 Hungry` | `--zone-warm` | Baker notes "hooch" or entry is >36 h old without a subsequent feed |
| `🔴 Discard` | `--zone-hot` | Baker notes "pink", "orange", or "mould" in Notes field |

Below the feed timeline, show a **feeding ratio chip row** for the last 5 feeds — `1:2:2`, `1:5:5` etc. displayed as compact pills in chronological order. Tapping a chip opens a tooltip with the computed peak estimate at the recorded `AmbientTempC`.

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
9. **Crumb notes in OutcomeSheet.** Add a `CRUMB NOTES` textarea group below the SCORES section (before PHOTO). Free-form text, auto-saved on blur, bound to `BakeOutcome.CrumbNotes`. Placeholder: `"Open and even? Tight or gummy? Flying crust? Note what you see."` — prompts the baker to apply the crumb-reading framework from baker's guide §20 without prescribing the vocabulary. On the history list, a 📋 chip appears beside the outcome summary when CrumbNotes is set.

---

### `/safety` — Food safety & shelf life (M20)

A reference page (no data, no auth). Three fault cards + a temperature ladder.

```
┌──────────────────────────────────────────────────┐
│  ← Back    Food safety & shelf life             │
│  Baker's guide §49                              │
│                                                  │
│  [Temperature ladder — inline bar]               │
│  ░░ < 5 °C  Safe cold (stales fastest)          │
│  ▓▓ 5–60 °C  Danger zone  (rope / mould grow)  │
│  ░░ > 140 °C  Crust formation                   │
│                                                  │
│  ┌── Rope ──────────────────────────────────┐   │
│  │  Cause: Bacillus subtilis (soil spores)  │   │
│  │  Symptoms: sticky centre, melon smell    │   │
│  │  Fix: acidify (sourdough); cool fast;    │   │
│  │       discard + sanitise surfaces        │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌── Mould ─────────────────────────────────┐   │
│  │  Lands after baking (air, hands, cut)    │   │
│  │  Never cut off — discard whole loaf      │   │
│  │  Defence: lower crust moisture; acidity  │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌── Staling ───────────────────────────────┐   │
│  │  Fridge is worst (retrogradation peak)   │   │
│  │  Freezer nearly halts staling            │   │
│  │  Refresh once (60 °C / 10 min); stales   │   │
│  │  faster after second bake                │   │
│  └──────────────────────────────────────────┘   │
└──────────────────────────────────────────────────┘
```

**StorageAdvisor chip** (inline in outcome section of `LiveBake.razor`):

```
┌────────────────────────────────────────────┐
│  🧺  Room temp · cloth or paper · 3–5 days │
│  ↳ Sourdough acidity inhibits rope & mould │
└────────────────────────────────────────────┘
```

Tap/hover expands to show the temperature ladder. Use `.zone-ideal` (green) background for the chip — signals safe storage, not a warning.

---

### `/kit` — Equipment & kit guide (M21)

Four accordion sections. Default: first section open, rest collapsed.

```
┌──────────────────────────────────────────────────┐
│  ← Back    Equipment & kit guide                │
│  Baker's guide §50 + §51                        │
├──────────────────────────────────────────────────┤
│  ▼ Oven preheat calculator                      │
│  Oven type   ○ Conventional  ● Convection  ○ Gas │
│  Surface     ● Steel  ○ Stone  ○ Bare tray       │
│                                                  │
│  Preheat for  45 min                            │
│  (Heat steel to full thermal mass before loading)│
├──────────────────────────────────────────────────┤
│  ▶ Steam methods                                 │
├──────────────────────────────────────────────────┤
│  ▶ Buying guide (Starter / Serious / Semi-pro)   │
├──────────────────────────────────────────────────┤
│  ▶ Scoring & shaping tools                      │
└──────────────────────────────────────────────────┘
```

**Steam methods section (expanded):**

```
┌──────────────────────────────────────────────────┐
│  ★★★★★  Dutch oven / combo cooker               │
│          Lid on 20 min → lid off 15–25 min       │
│  ★★★★☆  Cast-iron + lava rocks                  │
│  ★★★☆☆  Steam pan                               │
│  ★★☆☆☆  Ice cubes                               │
│  ★☆☆☆☆  Spray bottle                            │
└──────────────────────────────────────────────────┘
```

**Tiered buying guide section (expanded):**

```
┌──────────────────────────────────────────────────┐
│  STARTER                                         │
│  ✓ Digital scale (0.1 g)   ✓ Dutch oven         │
│  ✓ Bench knife             ✓ Banneton           │
│  ✓ Lame / razor            ✓ Probe thermometer  │
│                                                  │
│  SERIOUS                                         │
│  □ Baking steel            □ Stand mixer        │
│  □ Oven thermometer        □ Couche             │
│  □ Proofing box                                 │
│                                                  │
│  SEMI-PRO                                        │
│  □ Deck / steam-injection oven                  │
│  □ Spiral mixer            □ pH meter           │
│  □ Dedicated retarder      □ Wire racks         │
└──────────────────────────────────────────────────┘
```

Checkboxes persist in `localStorage` (key `kit-owned-{itemSlug}`). Checked items render with `--zone-ideal` text; unchecked with `--text-muted`.

---

### Steamed bake — Live Bake additions (M22)

**Steam step note** — rendered inside `StepCard.razor` when `Step.Phase == "Bake"` and method is Steamed:

```
┌──────────────────────────────────────────────────┐
│  ⚠ Steam protocol                               │
│  Line lid with a cloth — condensation drops     │
│  cause wrinkles on the skin.                    │
│  Do not open lid for first 10 min.              │
│  Rest 2–3 min with lid ajar before removing.   │
└──────────────────────────────────────────────────┘
```

Uses `.zone-warm` (amber) background — a caution, not an error. Same pattern as the overrun badge.

**Outcome troubleshooting selector** (new section in `OutcomeSheet.razor` when method is Steamed):

| Symptom chip | Cause shown | Fix shown |
|---|---|---|
| Wrinkles | Over-proof or lid shock | Proof less; keep lid closed |
| Dense crumb | Under-proof or wrong flour | Proof longer; use low-protein flour |
| Yellow tinge | Too much yeast | Reduce yeast; proof cooler |
| Rough skin | Steam too vigorous / lid drips | Cloth-line lid; lower heat slightly |

---

### Enriched dough — Advisor & Live Bake additions (M23)

**Advisor enrichment panel** (appears when `BakeMethod == Enriched`):

```
┌──────────────────────────────────────────────────┐
│  Enrichment                                      │
│  Butter  [10]%   Egg    [10]%   Sugar  [10]%    │
│  Milk    [60]%   Milk powder  [ 3]% (optional)  │
│  [✓] Use Tangzhong (roux pre-gelatinisation)     │
│      Roux flour share  [ 6]%                    │
└──────────────────────────────────────────────────┘
```

The "Use Tangzhong" toggle calls `/api/calculators/roux` on change (client-side, debounced 400 ms) and shows a live preview: `"Tangzhong: 30 g flour + 150 g milk"`.

**Live Bake formula summary line** (M23 additions to existing pattern):

```
78% · 500 g flour · 2% salt · 10% butter · 10% sugar  [Pullman tin]
```

`[Pullman tin]` badge uses `.zone-cool` (steel blue) — a neutral descriptor, not a warning.
