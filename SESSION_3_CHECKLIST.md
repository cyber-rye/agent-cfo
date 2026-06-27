# Session 3 Checklist — React Dashboard (Sunday Jun 28)

**Goal:** Working dashboard that shows seeded financial data and agent reasoning.
**Time budget:** 6-8 hours
**Prerequisite:** Docker Compose running (PostgreSQL + API)

---

## Pre-Flight

- [ ] `cd /home/ubuntu/projects/agent-cfo && docker compose up -d`
- [ ] Wait for PostgreSQL healthy, API starts on port 5000
- [ ] `curl http://localhost:5000/swagger` — verify Swagger loads
- [ ] `curl -X POST http://localhost:5000/api/seed/demo` — seed demo data
- [ ] Save the returned `OrganizationId` — you'll need it for all API calls

## Step 1: Scaffold (30 min)

```bash
cd /home/ubuntu/projects/agent-cfo
npm create vite@latest dashboard -- --template react-ts
cd dashboard
npm install
npm install -D tailwindcss @tailwindcss/vite
npm install recharts lucide-react
```

**Tailwind setup:**
- Add `@import "tailwindcss"` to `src/index.css`
- Add Tailwind Vite plugin to `vite.config.ts`

**Vite proxy** (in `vite.config.ts`):
```ts
server: {
  proxy: {
    '/api': 'http://localhost:5000'
  }
}
```

- [ ] `npm run dev` — app loads at localhost:5173
- [ ] Tailwind classes work

## Step 2: API Client + Types (30 min)

Create `src/api/client.ts`:
- Base fetch wrapper
- Type definitions matching API responses (copy from Swagger)

Key types needed:
- `DashboardSummary` (from `/api/dashboard/{orgId}`)
- `ForecastResponse` (from `/api/forecasts/{orgId}`)
- `RevenueMetrics` (from `/api/forecasts/{orgId}/revenue`)
- `BudgetResponse[]` (from `/api/budgets/{orgId}`)
- `AgentDecision[]` (from `/api/agent/{orgId}/decisions`)

- [ ] API client fetches from all 5 endpoints
- [ ] Types are correct (check against Swagger)

## Step 3: Financial Overview Cards (1.5 hr)

Create `src/components/FinancialOverview.tsx`:
- **MRR** card — from revenue metrics
- **Monthly Burn** card — from forecast
- **Runway** card — from forecast (days + end date)
- **Net Income** card — revenue minus expenses

Design: Dark card layout, large numbers, trend indicators (↑↓).

- [ ] 4 metric cards render with real data
- [ ] Numbers match API response

## Step 4: Agent Activity Feed (1 hr)

Create `src/components/AgentFeed.tsx`:
- List of recent decisions from `/api/agent/{orgId}/decisions`
- Each item: type badge, description, timestamp
- Expandable reasoning text (click to show full reasoning)
- Color-coded by type (green=approved, red=denied, yellow=anomaly, blue=report)

- [ ] Decisions list renders
- [ ] Reasoning expands on click
- [ ] Color coding works

## Step 5: Cash Flow Forecast Chart (1 hr)

Create `src/components/ForecastChart.tsx`:
- Recharts `AreaChart` or `LineChart`
- X-axis: projection dates (3 months)
- Y-axis: projected balance
- Include revenue and expense lines
- Call `/api/forecasts/{orgId}`

- [ ] Chart renders with projection data
- [ ] Lines are labeled and colored

## Step 6: Budget Status (1 hr)

Create `src/components/BudgetStatus.tsx`:
- Progress bars per category (Infrastructure, Marketing, Tools, Contractors, Office)
- Show: current spend / limit, percent used
- Color: green (<75%), yellow (75-90%), red (>90%)
- Call `/api/budgets/{orgId}`

- [ ] Budget bars render
- [ ] Colors reflect utilization

## Step 7: Layout + Dark Theme (30 min)

Create `src/App.tsx`:
- Header with "AgentCFO" branding
- Grid layout: overview cards on top, chart + feed below
- Dark theme (bg-gray-900, text-white)
- Seed button in header (calls `/api/seed/demo`, refreshes page)

- [ ] Dark theme looks good
- [ ] Layout is responsive
- [ ] Seed button works

## Step 8: Integration Test (30 min)

- [ ] Start fresh: `docker compose down -v && docker compose up -d`
- [ ] Seed data via dashboard button
- [ ] All dashboard components show data
- [ ] Trigger agent analysis: `curl -X POST http://localhost:5000/api/agent/{orgId}/run-full-analysis`
- [ ] Refresh dashboard — new decisions appear in feed

---

## Exit Criteria

Dashboard loads → shows MRR/burn/runway → agent feed with reasoning → forecast chart → budget bars → all from seeded data.

## If Behind Schedule

**Cut to MVP (2-3 hours):**
1. Skip budget status chart
2. Skip fancy styling — minimal Tailwind
3. Focus on: 3 metric cards + agent feed + forecast chart
4. That's enough for the demo

**Emergency fallback (1 hour):**
Use Swagger UI as the "dashboard" — it's already running and shows all endpoints. Not as pretty but functional.
