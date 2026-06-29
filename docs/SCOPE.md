# Hackathon Scope

**Deadline:** EOD Tuesday, June 30 (2 days remaining)
**Submission:** 1-3 min demo video on X/Twitter + submission form

## Must-Have (Demo Blockers)

These features must work end-to-end for the demo to land.

### 1. .NET Control Plane ✅
- [x] ASP.NET Core 10 Web API with Clean Architecture
- [x] EF Core + PostgreSQL — transactions, budgets, forecasts, audit log
- [x] MediatR CQRS for all operations
- [x] Policy pipeline behaviors (spending limits, approval gates)
- [x] Stripe webhook receiver (payment_intent.succeeded, invoice.paid, etc.)

### 2. Stripe Integration ✅
- [x] Real-time transaction ingestion from Stripe webhooks
- [x] Revenue metrics calculation (MRR, churn, growth rate)
- [ ] Invoice generation and payment tracking (not needed for demo)
- [x] Expense recording (manual or categorized)
- [x] Budget creation and enforcement

### 3. Agent (Hermes + NemoClaw) ✅
- [ ] Hermes agent running in NemoClaw sandbox (use API directly for demo)
- [x] Agent can query financial data via API
- [x] Agent can make decisions (approve expense, flag anomaly, generate report)
- [x] Agent decisions go through policy engine before execution
- [x] Full audit trail of agent reasoning and actions

### 4. Dashboard (React) ✅
- [x] Financial overview: revenue, expenses, runway, burn rate
- [x] Agent activity feed with typewriter reasoning
- [x] Cash flow forecast (collapsible: compact numbers + expandable chart)
- [x] Budget status with agent decisions
- [x] Expense evaluator (ask → evaluate → approve → record)
- [x] Audit trail panel
- [x] Agent-first layout (feed = 2/3 width, sidebar = 1/3)

### 5. Demo 🔶
- [x] Pre-loaded with realistic startup financial data
- [ ] 1-3 minute video showing agent making real decisions
- [ ] Clear narrative arc: problem → agent acts → result

## Nice-to-Have (If Time Permits)

- [ ] Scenario modeling ("what if we hire 3 engineers?") — API supports `?scenario=`, no UI dropdown yet
- [ ] Quick action buttons (individual agent capabilities)
- [ ] Financial summary viewer (generate-summary returns markdown)
- [ ] Governance panel (what the agent can/can't do)
- [ ] Investor report PDF generation
- [ ] Slack/Discord notifications for agent decisions
- [ ] Multi-organization support
- [ ] Agent self-improvement (learning from past decisions)
- [x] Animated typing effect for agent reasoning ✅
- [x] Toast notifications for agent decisions ✅
- [x] Agent status indicator ✅
- [x] Error handling + stale localStorage recovery ✅
- [x] Founder persona banner ✅
- [x] Expense evaluator (ask → evaluate → approve → record) ✅
- [x] Audit trail panel ✅
- [x] Collapsible forecast chart ✅
- [x] Agent-first layout restructure ✅
- [ ] Agent confidence scoring

## Explicitly Out of Scope

- Authentication/authorization (demo mode is fine)
- Production deployment
- Tax/legal compliance
- Payroll
- Mobile app
- CI/CD pipeline

## Day-by-Day Plan

### Day 1 (Friday) — Foundation ✅
- [x] .NET solution scaffold with Clean Architecture layers
- [x] Domain entities: Transaction, Budget, Forecast, AuditEntry, AgentDecision
- [x] EF Core DbContext + migrations
- [x] Stripe webhook endpoint (basic)
- [x] Docker Compose: PostgreSQL + API
- [ ] NemoClaw setup and Hermes agent bootstrap (not needed for demo)

### Day 2 (Saturday) — Agent + Stripe ✅
- [x] Stripe integration: webhook processing, transaction sync
- [x] Revenue metrics service (MRR, churn, growth)
- [x] Agent ↔ API communication (agent queries data, submits decisions)
- [x] Policy engine: MediatR pipeline behaviors for spending limits
- [x] Cash flow forecasting logic (simple projection model)
- [x] Agent prompt engineering: CFO persona, decision framework

### Day 3 (Sunday) — Dashboard + Polish ✅
- [x] React dashboard scaffold
- [x] Financial overview page (metrics, charts)
- [x] Agent activity feed
- [x] Cash flow forecast visualization
- [x] Pre-load demo data (realistic 3-month startup history)
- [x] End-to-end integration testing

### Day 4 (Monday) — Demo + Submit ⬅️ NEXT
- [x] Polish dashboard (loading states, error handling, wow factors) ✅
- [ ] Demo script rehearsal — 6 scenes, 90-120 seconds total
- [ ] Record demo video (screen capture + narration)
- [ ] Write Twitter thread with video
- [ ] Fill submission form
- [ ] Final polish and bug fixes

## Key Risk: Time

4 days is tight. The biggest risk is spending too long on infrastructure and not enough on the demo. **The demo is the deliverable.** Every feature we build should answer: "Does this help the demo?"

If we're behind on Day 3, we cut dashboard features and keep the agent demo compelling. A great agent demo with a basic dashboard beats a polished dashboard with a boring agent.
