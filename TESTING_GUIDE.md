# AgentCFO — Interactive Testing Guide

Walk through these scenarios to understand how AgentCFO works end-to-end. Each scenario builds on the previous one.

---

## Prerequisites

```bash
# Terminal 1: Start PostgreSQL
cd /home/ubuntu/projects/agent-cfo
docker compose up -d postgres

# Terminal 2: Start the API
cd /home/ubuntu/projects/agent-cfo/src/AgentCfo.Api
ConnectionStrings__DefaultConnection="Host=localhost;Database=agentcfo;Username=agentcfo;Password=agentcfo" \
  dotnet run --urls "http://localhost:5000"

# Terminal 3: Start the dashboard
cd /home/ubuntu/projects/agent-cfo/dashboard
npm run dev
```

Open **http://localhost:5173** for the dashboard.
Open **http://localhost:5000/swagger** for the API docs.

---

## Scenario 1: First Boot — Empty State

**Goal:** See what an empty AgentCFO looks like.

1. Open http://localhost:5173
2. You'll see the welcome screen with a "Seed Demo Data" button
3. Click **Seed Demo Data**
4. The dashboard loads with 90 transactions, 5 budgets, and 4 pre-seeded agent decisions

**What happened:** The `POST /api/seed/demo` endpoint created a demo organization ("Acme SaaS") with 3 months of realistic startup financial data — subscription revenue, cloud hosting costs, marketing spend, contractor payments, and office expenses.

**Explore:**
- The 4 metric cards show MRR, burn rate, runway, and net income
- The agent feed shows 4 pre-seeded decisions with reasoning
- The forecast chart shows 3-month cash flow projection
- The budget bars show spending per category

---

## Scenario 2: The Agent Thinks — Anomaly Detection

**Goal:** Watch the agent analyze financial data and detect issues.

1. Click the **Run Analysis** button in the header
2. Wait a few seconds — the agent runs 3 analyses
3. New decisions appear in the agent feed

**What happened:** The agent ran three analyses:
- **Anomaly Detection:** Scanned month-over-month expense growth and budget utilization
- **Forecast Generation:** Calculated runway and generated 3-month projections
- **Financial Summary:** Produced a comprehensive markdown report

**Explore:**
- Click on the "AnomalyDetected" decision to expand its reasoning
- Read the agent's analysis — it explains *what* it found and *what to do about it*
- Notice the forecast updated with a runway warning

---

## Scenario 3: The Agent Acts — Expense Approval

**Goal:** Submit an expense request and watch the agent evaluate it.

Use Swagger (http://localhost:5000/swagger) or curl:

```bash
# First, get your org ID from the dashboard URL or:
curl -s http://localhost:5000/api/organizations | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['id'])"

# Submit a reasonable expense — should be APPROVED
curl -X POST http://localhost:5000/api/agent/{ORG_ID}/evaluate-expense \
  -H "Content-Type: application/json" \
  -d '{"amount": 500, "description": "Figma team upgrade", "currency": "USD"}'
```

**Read the response:** The agent evaluated the request against the Tools budget and approved it.

Now try a large expense:

```bash
# Submit an expensive request — should be DENIED
curl -X POST http://localhost:5000/api/agent/{ORG_ID}/evaluate-expense \
  -H "Content-Type: application/json" \
  -d '{"amount": 25000, "description": "Enterprise Salesforce license", "currency": "USD"}'
```

**Read the response:** The agent denied it — the expense ratio is too high relative to the total budget.

**Explore:**
- Refresh the dashboard — both decisions now appear in the agent feed
- Click each to see the agent's reasoning
- Notice how the reasoning references actual budget numbers

---

## Scenario 4: Budget Enforcement — Hard Limits

**Goal:** See the policy engine block an expense that exceeds a hard limit.

The Infrastructure budget has `HardLimit: true` and a $3,000 monthly limit. Let's test it:

```bash
# Record an expense that exceeds the Infrastructure hard limit
curl -X POST http://localhost:5000/api/transactions/expense \
  -H "Content-Type: application/json" \
  -d '{"organizationId": "{ORG_ID}", "amount": 2000, "currency": "USD", "description": "AWS reserved instances", "category": 2}'
```

**What happened:** The `RecordExpense` command checks the Infrastructure budget. Since the budget already has ~$2,344 in spend and this $2,000 expense would exceed the $3,000 hard limit, the request is rejected with a `409 Conflict`.

**Explore:**
- Read the rejection message — it explains exactly why
- Try a smaller amount that fits within the remaining budget
- Compare with Marketing (soft limit) — expenses always go through there

---

## Scenario 5: Revenue Intelligence — MRR and Growth

**Goal:** Understand how the agent tracks revenue metrics.

```bash
# Get revenue metrics
curl -s http://localhost:5000/api/forecasts/{ORG_ID}/revenue | python3 -m json.tool
```

**Explore the response:**
- `monthlyRecurringRevenue`: Current month's revenue
- `growthRatePercent`: Month-over-month growth
- `monthlyTrend`: 6-month revenue history with transaction counts
- `averageRevenuePerUser`: Revenue per customer
- `activeSubscriptions`: Number of paying customers

**What to notice:**
- Revenue grew from April → June (the seed data simulates a growing startup)
- The agent uses these metrics in its financial summaries and anomaly detection

---

## Scenario 6: Cash Flow Forecasting

**Goal:** See how the agent projects runway.

```bash
# Get the forecast
curl -s http://localhost:5000/api/forecasts/{ORG_ID} | python3 -m json.tool
```

**Explore the response:**
- `cashBalance`: All-time revenue minus expenses
- `monthlyBurnRate`: Current month's expenses
- `monthlyRevenue`: Current month's revenue
- `runwayDays`: How many days until cash runs out
- `projections`: 3-month forecast with balance, revenue, and expenses per month

**Try a scenario:**
```bash
# What if we hire 2 engineers? (adds ~$20K/mo burn)
curl -s "http://localhost:5000/api/forecasts/{ORG_ID}?scenario=hire-2-engineers" | python3 -m json.tool
```

Notice the `scenario` field in the response — the forecast engine tags projections with the scenario name.

---

## Scenario 7: The Full Dashboard Flow

**Goal:** Experience the demo as it would be recorded.

1. Open http://localhost:5173
2. Click **Seed Demo Data** (fresh start)
3. Observe the dashboard with pre-seeded data
4. Click **Run Analysis** — watch new decisions appear in the feed
5. Click on a decision to expand its reasoning
6. Look at the forecast chart — notice the runway warning
7. Look at the budget bars — Marketing is at 76%, Office at 78%

**This is the demo flow:**
- Scene 1: Empty state → seed data (15s)
- Scene 2: Dashboard loads with 3 months of data (15s)
- Scene 3: Click Run Analysis → agent reasoning appears (20s)
- Scene 4: Show an expense approval/denial (25s)
- Scene 5: Forecast chart with runway (20s)
- Scene 6: Full dashboard overview (10s)

---

## Scenario 8: API Exploration

**Goal:** Discover the full API surface.

Open http://localhost:5000/swagger and explore:

| Endpoint | Try it | What you'll see |
|----------|--------|-----------------|
| `GET /api/organizations` | Click Execute | List of demo orgs |
| `GET /api/dashboard/{orgId}` | Click Execute | Financial summary with decisions |
| `GET /api/transactions/{orgId}` | Click Execute | 90 transactions across 3 months |
| `GET /api/transactions/{orgId}?type=0` | Set type=0 | Revenue only |
| `GET /api/transactions/{orgId}?type=1` | Set type=1 | Expenses only |
| `GET /api/budgets/{orgId}` | Click Execute | Budget utilization |
| `GET /api/forecasts/{orgId}` | Click Execute | Cash flow forecast |
| `GET /api/forecasts/{orgId}/revenue` | Click Execute | MRR, churn, growth |
| `GET /api/agent/{orgId}/decisions` | Click Execute | Agent reasoning feed |
| `GET /api/agent/{orgId}/audit` | Click Execute | Full audit trail |
| `POST /api/agent/{orgId}/detect-anomalies` | Click Execute | Agent scans for anomalies |
| `POST /api/agent/{orgId}/generate-summary` | Click Execute | Full markdown report |
| `POST /api/seed/demo` | Click Execute | Reset with fresh data |

---

## Scenario 9: Stripe Webhook Simulation

**Goal:** See how Stripe events flow into the system.

The webhook endpoint is at `POST /api/webhooks/stripe`. In production, Stripe sends events here. For testing, you'd use the Stripe CLI:

```bash
# Install Stripe CLI (if not installed)
# brew install stripe/stripe-cli/stripe  (macOS)
# snap install stripe  (Linux)

# Login and forward events
stripe login
stripe listen --forward-to localhost:5000/api/webhooks/stripe

# Trigger a test event
stripe trigger payment_intent.succeeded
```

**What would happen:** The webhook service receives the event, finds the organization by Stripe customer ID, creates a Transaction, and the agent can then analyze it.

**Note:** For the hackathon demo, we use the SeedController instead of live Stripe — it's more reliable and reproducible.

---

## Key Concepts to Understand

### Clean Architecture
```
Core (entities, interfaces)  ← no dependencies
    ↑
Application (MediatR handlers, validators)
    ↑
Infrastructure (EF Core, services, repos)
    ↑
API (controllers, config)
```

### MediatR CQRS Pattern
- **Commands** change state: `RecordExpense`, `CreateBudget`
- **Queries** read state: `GetDashboardSummary`, `GetRevenueMetrics`
- **Pipeline behaviors** run on every request: `AuditBehavior` (logging), `PolicyBehavior` (budget enforcement)

### Agent Decision Flow
```
Trigger (webhook/expense/analysis)
    → AgentService analyzes data
    → Generates AgentDecision with reasoning
    → PolicyBehavior checks limits
    → Decision persisted to DB
    → Dashboard fetches and displays
```

### Money Value Object
Never raw decimals for currency. `Money.From(100, "USD")` ensures currency-aware arithmetic with validation.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Dashboard shows "Welcome" with no data | Click "Seed Demo Data" |
| API returns 500 | Check PostgreSQL is running: `docker ps` |
| Dashboard can't reach API | Verify API on port 5000: `curl localhost:5000/swagger` |
| Budget shows 0% | FIXED in Session 3 — budgets now sync spend. If stale, re-seed: `curl -X POST localhost:5000/api/seed/demo` |
| Agent decisions missing | Click "Run Analysis" or call `POST /api/agent/{id}/run-full-analysis` |
| TypeScript errors in dashboard | Run `cd dashboard && npx tsc -b` to see details |
