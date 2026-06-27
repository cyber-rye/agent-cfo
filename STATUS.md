# AgentCFO — Project Status & Session Plan

**Assessed:** Saturday, June 27, 2026
**Deadline:** EOD Tuesday, June 30 (3.5 days)
**Submission:** 1–3 min demo video on X/Twitter + form

---

## Current Status: ~60% Complete

The .NET backend is essentially done. Clean Architecture scaffold, all entities, all repositories, agent service with full reasoning, revenue metrics, forecasting, pipeline behaviors, demo data seeder, EF Core migration — all built and compiling with 0 errors/0 warnings. The remaining work is the React dashboard and demo recording.

### What's Built (✅)

| Layer | Component | Notes |
|-------|-----------|-------|
| **Core** | All 6 entities | Organization, Transaction, Budget, Forecast, AgentDecision, AuditEntry — with factory methods, encapsulated behavior |
| **Core** | All enums | TransactionType, ExpenseCategory, DecisionStatus, ActorType, ForecastSource, ForecastConfidence, BudgetPeriod, TransactionStatus |
| **Core** | Money value object | Amount + Currency with Add/Subtract/Multiply, currency validation |
| **Core** | BaseEntity | Shared Id + audit fields + domain event support |
| **Core** | 4 domain events | PaymentReceived, BudgetThresholdBreached, RunwayAlert, AgentDecisionMade |
| **Core** | IAgentService interface | 5 methods — all IMPLEMENTED in Infrastructure |
| **Core** | All repository interfaces | ITransactionRepo, IBudgetRepo, IOrganizationRepo, IAgentDecisionRepo, IAuditEntryRepo, IUnitOfWork |
| **Infra** | AppDbContext | Full DbContext with 6 DbSets, auto-audit on SaveChanges |
| **Infra** | 6 EF configurations | Fluent API for all entities with owned types for Money |
| **Infra** | 5 repositories | Organization, Transaction, Budget, AgentDecision, AuditEntry |
| **Infra** | StripeWebhookService | Handles payment_intent.succeeded/failed, invoice.paid/failed, idempotency |
| **Infra** | RevenueMetricsService | MRR, churn rate, growth rate, ARPU, NRR, 6-month trend |
| **Infra** | ForecastService | 3-month projection with growth-adjusted revenue, runway calculation |
| **Infra** | AgentService | Full implementation: anomaly detection, expense evaluation, forecast generation, financial summary — all with rich multi-paragraph reasoning |
| **Infra** | DI registration | AddInfrastructure() wired up |
| **Application** | MediatR DI | AddApplication() with assembly scanning |
| **Application** | RecordExpense | With FluentValidation + budget enforcement check |
| **Application** | CreateBudget | With validation |
| **Application** | GetTransactions | Query by organization with type filter |
| **Application** | GetDashboardSummary | Revenue/expenses/net income + budget count + recent decisions |
| **Application** | GetCurrentForecast | Generates forecast via ForecastService |
| **Application** | GetRevenueMetrics | Revenue metrics with monthly trend |
| **Application** | AuditBehavior | MediatR pipeline — structured logging for all requests |
| **Application** | PolicyBehavior | MediatR pipeline — budget limit checks (⚠️ never blocks due to bug — logs only) |
| **API** | 8 controllers | Dashboard, Transactions, Budgets, Forecasts, Agent (7 endpoints), Organizations, StripeWebhook, Seed |
| **API** | Program.cs | Minimal API setup with CORS, Swagger, JSON enum serialization |
| **API** | Dockerfile | Multi-stage build, .NET 10 preview |
| **Infra** | docker-compose.yml | PostgreSQL 16 + API service |
| **Infra** | EF Core InitialCreate migration | Applied — full schema for all 6 entities |

### What's Missing (❌ = Not Started, 🔶 = Partial)

| Priority | Component | Impact |
|----------|-----------|--------|
| **P0** | React dashboard | THE visual for demo — 0 files exist |
| **P0** | Demo video recording | THE deliverable |
| **P1** | Fix PolicyBehavior bug | Never actually blocks — logs only (line 47 dead code) |
| **P1** | Fix SeedController budget spend | Budgets show 0% utilization despite expenses existing |
| **P1** | Stripe webhook secret | Empty in appsettings — webhooks won't verify |
| **P2** | Unit tests | No test project — not a demo blocker |
| **P2** | NemoClaw config | Not needed for demo — mention in narration |

### Known Bugs

1. **PolicyBehavior never blocks** (`Application/Common/PolicyBehavior.cs:47`): `if (request is not IRequest<TResponse>)` is always false due to generic constraint. Policy violation exception is dead code. For demo: logging still works, which is what matters visually.

2. **SeedController budget CurrentSpend = 0**: Budgets are created with zero CurrentSpend even though expenses are seeded. Dashboard budget utilization will show 0%. Fix: either call `budget.RecordSpend()` for each seeded expense, or compute CurrentSpend from transactions at query time.

---

## Session Plan: 3 Days to Demo

### Session 3 (Sun Jun 28) — React Dashboard ⬅️ NEXT
**Goal:** Dashboard showing financial overview + agent activity + charts.

1. **Scaffold** — Vite + React + TypeScript + Tailwind CSS
2. **API client** — TypeScript types matching API responses, fetch wrapper with base URL
3. **Financial overview** — MRR, burn rate, runway, net income cards
4. **Agent activity feed** — Recent decisions with reasoning (expandable)
5. **Cash flow forecast chart** — Line chart with 3-month projection (Recharts)
6. **Budget status** — Progress bars per category showing spend vs limit
7. **Dark theme** — Tailwind dark mode (looks better on video)
8. **Seed button** — Call `/api/seed/demo`, auto-refresh

**Exit criteria:** Dashboard loads → shows seeded data → agent feed updates

**API endpoints to consume:**
- `GET /api/dashboard/{orgId}` — summary stats + recent decisions
- `GET /api/forecasts/{orgId}` — forecast with projection points
- `GET /api/forecasts/{orgId}/revenue` — MRR, churn, monthly trend
- `GET /api/budgets/{orgId}` — budget utilization
- `GET /api/agent/{orgId}/decisions` — agent reasoning feed
- `GET /api/transactions/{orgId}` — transaction list
- `POST /api/seed/demo` — seed data (returns orgId)

---

### Session 4 (Mon Jun 29) — Integration + Polish
**Goal:** End-to-end flow works, polish for recording.

1. Fix SeedController budget spend sync
2. End-to-end test: seed → dashboard → trigger agent analysis → decisions appear
3. Dashboard polish: responsive layout, loading states, error handling
4. Rehearse demo script
5. Bug fixes from integration testing

**Exit criteria:** Full flow works with seeded data, ready to record

---

### Session 5 (Tue Jun 30 AM) — Record + Submit
**Goal:** Demo video recorded and submitted.

1. Demo script rehearsal — 6 scenes, 90–120 seconds total
2. Screen recording — OBS at 1080p, dark theme dashboard
3. Voiceover — Either live or post-record
4. Twitter thread — Video + writeup
5. Submission form — Fill and submit

**Exit criteria:** Video posted on X, form submitted, done.

---

## Key Risks

| Risk | Mitigation |
|------|-----------|
| **Dashboard takes too long** | Cut features. MVP: 3 cards (MRR, burn, runway) + 1 chart (forecast) + 1 list (agent decisions). That's enough for the demo. |
| **NemoClaw not ready** | Demo with API directly. Mention NemoClaw in narration as target deployment. |
| **Time crunch** | Dashboard is the bottleneck. If behind by Sunday night, use Swagger UI as the "dashboard" and focus on terminal/API demo. |
| **Stripe test setup** | Use Stripe CLI (`stripe listen --forward-to localhost:5000/api/webhooks/stripe`) for local testing. Not needed for demo — SeedController provides data. |

## Decision Log

- **Dashboard framework:** React + Vite + Tailwind (fast, familiar)
- **Charts:** Recharts (React-native, good docs)
- **Agent integration:** Deterministic analysis (not LLM) for demo reliability — produces rich, realistic reasoning
- **Demo data:** Pre-seeded via SeedController, not live Stripe (reproducible for recording)
- **Auth:** Skipped (demo mode, per SCOPE.md)
- **AgentService approach:** Deterministic financial analysis with compelling reasoning text (hackathon pragmatism — LLM integration would add latency and non-determinism)
