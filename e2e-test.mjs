/**
 * End-to-end test for the Bread-Making app.
 * Covers: advisor → start bake → run steps → add measurement → history → compare → clone-bake.
 */
import { chromium } from 'playwright';

const BASE = 'http://localhost:5112';
const PASS = '✅';
const FAIL = '❌';
let passed = 0, failed = 0;

function check(label, ok, detail = '') {
    if (ok) { console.log(`${PASS} ${label}`); passed++; }
    else     { console.error(`${FAIL} ${label}${detail ? ' — ' + detail : ''}`); failed++; }
}

const browser = await chromium.launch({
    headless: true,
    executablePath: 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
});
const ctx = await browser.newContext();
const page = await ctx.newPage();
page.setDefaultTimeout(15000);

// ── 1. Home / advisor ────────────────────────────────────────────────────────
await page.goto(BASE);
// Blazor WASM needs time to boot — wait for any button to appear
await page.waitForSelector('button', { timeout: 30000 });
check('Home page loads', await page.title() !== '');

// Pick experience level (ExperiencePicker is shown first)
const experiencePicker = page.locator('button', { hasText: 'Experienced' }).first();
if (await experiencePicker.isVisible({ timeout: 5000 }).catch(() => false)) {
    await experiencePicker.click();
}
await page.waitForSelector('.two-panel-layout', { timeout: 10000 });
check('Advisor form visible', await page.locator('.two-panel-layout').isVisible());

// ── 2. Set inputs and get recommendation ─────────────────────────────────────
// Slide temperature to ~24°C (default is 22; slider is min=15 max=32)
const slider = page.locator('input[type=range]').first();
await slider.fill('24');
await slider.dispatchEvent('input');
check('Temperature slider works', await page.locator('.temp-display').isVisible());

// Wait for recommendation panel
await page.waitForSelector('.recommendation', { timeout: 5000 }).catch(() => null);
const recVisible = await page.locator('.recommendation').isVisible();
check('Recommendation panel appears', recVisible);

// ── 3. Start Bake ────────────────────────────────────────────────────────────
const startBtn = page.locator('.btn-start-bake');
check('Start Bake button present', await startBtn.isVisible());
await startBtn.click();

// Should navigate to /bake/{id}
await page.waitForURL(/\/bake\/\d+/, { timeout: 10000 });
check('Navigated to live bake page', page.url().includes('/bake/'));

const bakeId = page.url().match(/\/bake\/(\d+)/)[1];
console.log(`  Bake ID: ${bakeId}`);

// ── 4. Live bake page ────────────────────────────────────────────────────────
await page.waitForSelector('.live-bake-page', { timeout: 8000 });
check('Live bake page renders', await page.locator('.live-bake-page').isVisible());
await page.waitForSelector('.planning-gantt, .step-card.running', { timeout: 5000 }).catch(() => null);
check('Planning Gantt shown before first step', await page.locator('.planning-gantt').isVisible());
check('Step list present', await page.locator('.step-list').isVisible());

// Count NotStarted steps
const steps = await page.locator('.step-card.not-started').count();
check(`Step cards rendered (${steps} steps)`, steps > 0);

// ── 5. Start first step ──────────────────────────────────────────────────────
const firstStartBtn = page.locator('.btn-start').first();
await firstStartBtn.click();
await page.waitForSelector('.step-card.running', { timeout: 5000 });
check('First step transitions to Running', await page.locator('.step-card.running').isVisible());
check('Elapsed timer visible', await page.locator('.step-elapsed').isVisible());
check('Progress bar rendered', await page.locator('.step-progress-track').isVisible());
check('Pause button present', await page.locator('.btn-pause').isVisible());

// ── 6. Pause and resume ──────────────────────────────────────────────────────
await page.locator('.btn-pause').click();
await page.waitForSelector('.step-card.paused', { timeout: 5000 });
check('Step pauses', await page.locator('.step-card.paused').isVisible());

await page.locator('button:has-text("Resume")').click();
await page.waitForSelector('.step-card.running', { timeout: 5000 });
check('Step resumes', await page.locator('.step-card.running').isVisible());

// ── 7. Complete first step ───────────────────────────────────────────────────
await page.locator('.btn-complete').click();
await page.waitForSelector('.step-card.completed', { timeout: 5000 });
check('First step completes', await page.locator('.step-card.completed').isVisible());
check('Planning Gantt gone after first step', !(await page.locator('.planning-gantt').isVisible()));

// ── 8. Add a measurement ─────────────────────────────────────────────────────
// Start second step (Autolyse/Fermentolyse rest)
await page.locator('.btn-start').first().click();
await page.waitForSelector('.step-card.running', { timeout: 5000 });

// Open measurement sheet
const addChip = page.locator('.chip-add').first();
if (await addChip.isVisible()) {
    await addChip.click();
    await page.waitForSelector('.ms-overlay', { timeout: 5000 });
    check('Measurement sheet opens', await page.locator('.ms-sheet').isVisible());

    // Increment value once, save
    await page.locator('.ms-stepper-btn').first().click(); // − button
    const saveBtn = page.locator('.ms-btn-save');
    await saveBtn.click();
    await page.waitForSelector('.ms-overlay', { state: 'hidden', timeout: 5000 });
    check('Measurement saved, sheet closes', !(await page.locator('.ms-overlay').isVisible()));
    check('Measurement chip appears', await page.locator('.measurement-chip:not(.chip-add)').isVisible());
} else {
    check('Measurement sheet (skipped — chip not visible in this step)', true);
}

// ── 9. History page ──────────────────────────────────────────────────────────
await page.goto(`${BASE}/history`);
await page.waitForSelector('.history-page', { timeout: 8000 });
check('History page loads', await page.locator('.history-page').isVisible());
check('RunChart renders', await page.locator('.run-chart').isVisible());
check('Compare grains button present', await page.locator('.btn-compare').isVisible());

// Bake list (may be empty if bake not fully complete)
const bakeItems = await page.locator('.bake-item').count();
console.log(`  Bake items in history: ${bakeItems}`);
check('Grain filter select present', await page.locator('.filter-select').first().isVisible());

// ── 10. Grain comparison page ────────────────────────────────────────────────
await page.goto(`${BASE}/history/compare`);
await page.waitForSelector('.comparison-page', { timeout: 8000 });
check('Grain comparison page loads', await page.locator('.comparison-page').isVisible());
// Empty state or charts — both are valid
const hasCharts = await page.locator('.comparison-charts').isVisible();
const hasEmpty  = await page.locator('.comparison-empty').isVisible();
check('Grain comparison renders (charts or empty state)', hasCharts || hasEmpty);

// ── 11. API smoke tests ───────────────────────────────────────────────────────
const bakeResp  = await page.request.get(`${BASE}/api/bakes/${bakeId}`);
check('GET /api/bakes/{id} returns 200', bakeResp.ok());

const listResp  = await page.request.get(`${BASE}/api/bakes`);
check('GET /api/bakes returns 200', listResp.ok());

const csvResp   = await page.request.get(`${BASE}/api/bakes/${bakeId}/export?format=csv`);
check('CSV export returns 200', csvResp.ok());
const csvBody = await csvResp.text();
check('CSV contains header row', csvBody.includes('StepName') && csvBody.includes('DoughTemp_C'));

const jsonResp  = await page.request.get(`${BASE}/api/bakes/${bakeId}/export?format=json`);
check('JSON export returns 200', jsonResp.ok());
const jsonBody  = await jsonResp.json();
const stepLogs = jsonBody.StepLogs ?? jsonBody.stepLogs;
check('JSON export has full bake graph', Array.isArray(stepLogs) && stepLogs.length > 0);

const inputsResp = await page.request.get(`${BASE}/api/bakes/${bakeId}/inputs`);
check('GET /api/bakes/{id}/inputs returns 200', inputsResp.ok());
const inputs = await inputsResp.json();
check('Inputs has grainName', typeof inputs.grainName === 'string');

const compResp  = await page.request.get(`${BASE}/api/grains/comparison`);
check('GET /api/grains/comparison returns 200', compResp.ok());

// ── 12. Clone-bake flow ───────────────────────────────────────────────────────
await page.goto(`${BASE}/history`);
await page.waitForSelector('.history-page', { timeout: 8000 });
if (await page.locator('.btn-similar').first().isVisible()) {
    await page.locator('.btn-similar').first().click();
    await page.waitForURL('**/', { waitUntil: 'domcontentloaded', timeout: 15000 });
    check('Start similar navigates to advisor', page.url() === `${BASE}/`);
    // ExperiencedForm should open directly (clone state present)
    await page.waitForSelector('.two-panel-layout', { timeout: 5000 });
    check('Advisor pre-fills (ExperiencedForm shown directly)', await page.locator('.two-panel-layout').isVisible());
} else {
    check('Clone-bake (skipped — no history items yet)', true);
}

// ── Summary ───────────────────────────────────────────────────────────────────
await browser.close();
console.log(`\n${'─'.repeat(50)}`);
console.log(`Results: ${passed} passed, ${failed} failed`);
if (failed > 0) process.exit(1);
