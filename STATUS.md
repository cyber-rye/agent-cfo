# AgentCFO — Project Status & Session Plan

**Assessed:** Saturday, June 27, 2026
**Deadline:** EOD Tuesday, June 30 (3.5 days)
**Submission:** 1–3 min demo video on X/Twitter + form

---

## Current Status: ~25% Complete

The Day 1 foundation scaffold is done. The .NET solution builds cleanly (0 errors, 0 warnings). Domain model is solid. But the agent, dashboard, forecasting, revenue metrics, pipeline behaviors, demo data, and video — the bulk of the project — are not started.

### What's Built (✅)

| Layer | Component | Notes |
|-------|-----------|-------|
| **Core** | All 6 entities | Organization, Transaction, Budget, Forecast, AgentDecision, AuditEntry |
| **Core** | All enums | TransactionType, ExpenseCategory, DecisionStatus, ActorType, etc. |
| **Core** | Money value object | Amount + Currency with Add/Subtract/Convert |
| **Core** | BaseEntity | Shared Id + audit fields |
| **Core** | 4 domain events | PaymentReceived, BudgetThresholdBreached, RunwayAlert, AgentDecisionMade |
| **Core** | IAgentService interface | 5 methods defined but NOT implemented |
| **Core** | Repository interfaces | ITransactionRepo, IBudgetRepo, IOrganizationRepo, IUnitOfWork |
| **Infra** | AppDbContext | Full DbContext with 6 DbSets, auto-audit on SaveChanges |
| **Infra** | 6 EF configurations | Fluent API for all entities |
| **Infra** | 3 repositories | Organization, Transaction, Budget |
| **Infra** | StripeWebhookService | Handles payment_intent.succeeded/failed, invoice.paid/failed, idempotency |
| **Infra** | DI registration | AddInfrastructure() wired up |
| **Application** | MediatR DI | AddApplication() with assembly scanning |
| **Application** | RecordExpense | With FluentValidation + budget enforcement check |
| **Application** | CreateBudget | With validation |
| **Application** | GetTransactions | Query by organization |
| **Application** | GetDashboardSummary | Revenue/expenses/net income + budget count |
| **Application** | GetCurrentForecast | Query (handler skeleton) |
| **API** | 5 controllers | StripeWebhook, Dashboard, Forecasts, Budgets, Transactions |
| **API** | Program.cs | Minimal API setup with CORS, Swagger |
| **API** | Dockerfile | Multi-stage build, .NET 10 preview |
| **Infra** | docker-compose.yml | PostgreSQL 16 + API service |

### What's Missing (❌ = Not Started, 🔶 = Partial)

| Priority | Component | SCOPE Item | Impact |
|----------|-----------|------------|--------|
| **P0** | EF Core migrations | Day 1 | DB won't start without schema |
| **P0** | Agent service implementation | Day 2 | The core differentiator — no demo without this |
| **P0** | Hermes ↔ API integration | Day 2 | Agent can't query data or submit decisions |
| **P0** | Revenue metrics (MRR, churn, growth) | Day 2 | Core financial intelligence |
| **P0** | Cash flow forecasting logic | Day 2 | Runway calculation — key demo scene |
| **P0** | MediatR pipeline behaviors | Day 2 | Policy enforcement + audit automation |
| **P0** | React dashboard scaffold | Day 3 | No visual for demo |
| **P0** | Dashboard: financial overview | Day 3 | Revenue, expenses, runway display |
| **P0** | Dashboard: agent activity feed | Day 3 | Shows agent reasoning (demo Scene 3) |
| **P0** | Dashboard: cash flow chart | Day 3 | Forecast visualization (demo Scene 5) |
| **P0** | Demo data seeder | Day 3 | 3 months realistic startup data |
| **P0** | Demo video recording | Day 4 | THE deliverable |
| **P1** | Forecast/Decision/Audit repos | Day 2 | Needed for agent + forecast features |
| **P1** | Agent prompt engineering | Day 2 | CFO persona + decision framework |
| **P1** | Budget status on dashboard | Day 3 | Agent decisions visible |
| **P2** | Scenario modeling | Nice-to-have | "What if we hire 2 engineers?" |
| **P2** | Investor report PDF | Nice-to-have | If time permits |

---

## Session Plan: 3.5 Days to Demo

### Session 1 (Sat Jun 27) — DB + Stripe + Revenue Metrics
**Goal:** API runs against real PostgreSQL, Stripe data flows in, revenue metrics computed.

1. **EF migrations** — Generate + apply initial migration against Docker PostgreSQL
2. **Verify Docker Compose** — `docker compose up` → API + PG working end-to-end
3. **Stripe test data** — Set up Stripe test mode, configure webhook secret
4. **Revenue metrics service** — MRR, churn rate, growth rate calculations
5. **Transaction categorization** — Agent/service logic to categorize Stripe events
6. **Pipeline behaviors** — MediatR behaviors for audit trail + policy enforcement

**Exit criteria:** `POST /api/stripe/webhook` with test event → Transaction persisted → Revenue metrics queryable

---

### Session 2 (Sun Jun 28 AM) — Agent Integration
**Goal:** Hermes agent can query financial data and make decisions.

1. **Agent service implementation** — Implement IAgentService (the Hermes ↔ API bridge)
2. **Forecast engine** — Simple projection model: burn rate × time = runway
3. **Agent prompt** — CFO persona with decision framework
4. **Agent ↔ API comms** — Agent queries data, submits decisions, decisions go through policy engine
5. **Audit trail** — Agent reasoning captured in AgentDecision + AuditEntry
6. **Remaining repositories** — Forecast, AgentDecision, AuditEntry repos

**Exit criteria:** Agent can answer "how much runway?" → decision logged → visible in audit trail

---

### Session 3 (Sun Jun 28 PM) — Dashboard
**Goal:** React dashboard showing financial overview + agent activity.

1. **React scaffold** — Vite + React + TypeScript + Tailwind
2. **Financial overview page** — Revenue, expenses, net income, runway
3. **Agent activity feed** — Recent decisions with reasoning
4. **Cash flow forecast chart** — 30/60/90 day projection (Chart.js or Recharts)
5. **Budget status** — Per-category spend vs limit
6. **Wire to API** — Fetch from /api/dashboard, /api/forecasts, etc.

**Exit criteria:** Dashboard loads → shows data → agent feed updates

---

### Session 4 (Mon Jun 29) — Demo Data + Integration Testing
**Goal:** Realistic data loaded, end-to-end flow works, polish.

1. **Demo data seeder** — 3 months of realistic startup financials (not "Test Customer 1")
2. **Seed script** — Run against API to populate DB
3. **End-to-end test** — Stripe webhook → transaction → agent analyzes → decision appears on dashboard
4. **Bug fixes** — Whatever broke during integration
5. **Dashboard polish** — Dark theme, responsive, clean

**Exit criteria:** Full flow works with seeded data, ready to record

---

### Session 5 (Tue Jun 30 AM) — Record + Submit
**Goal:** Demo video recorded and submitted.

1. **Demo script rehearsal** — 5 scenes, 90–120 seconds total
2. **Screen recording** — OBS at 1080p, dark theme dashboard
3. **Voiceover** — Either live or post-record
4. **Twitter thread** — Video + writeup
5. **Submission form** — Fill and submit
6. **Final polish** — Any last fixes

**Exit criteria:** Video posted on X, form submitted, done.

---

## Key Risks

| Risk | Mitigation |
|------|-----------|
| **Agent integration complexity** | Start with simplest possible Hermes ↔ API (HTTP calls from agent tool). Pre-cache responses for demo. |
| **NemoClaw unknowns** | If NemoClaw isn't ready, demo with Hermes directly. Mention NemoClaw in narration as the target deployment. |
| **Time crunch** | Dashboard is cuttable. A great agent demo with a basic terminal output beats a polished dashboard with a boring agent. |
| **Stripe test setup** | Use Stripe CLI (`stripe listen --forward-to localhost:5000/api/stripe/webhook`) for local testing. |

## Decision Log

- **Dashboard framework:** React + Vite + Tailwind (fast, familiar)
- **Charts:** Recharts or Chart.js (whichever is faster to wire up)
- **Agent integration:** Hermes makes HTTP calls to API endpoints (simplest path)
- **Demo data:** Pre-seeded, not live Stripe (reproducible for recording)
- **Auth:** Skipped (demo mode, per SCOPE.md)
