# Hackathon Scope

**Deadline:** EOD Tuesday, June 30 (4 days)
**Submission:** 1-3 min demo video on X/Twitter + submission form

## Must-Have (Demo Blockers)

These features must work end-to-end for the demo to land.

### 1. .NET Control Plane
- [ ] ASP.NET Core 10 Web API with Clean Architecture
- [ ] EF Core + PostgreSQL — transactions, budgets, forecasts, audit log
- [ ] MediatR CQRS for all operations
- [ ] Policy pipeline behaviors (spending limits, approval gates)
- [ ] Stripe webhook receiver (payment_intent.succeeded, invoice.paid, etc.)

### 2. Stripe Integration
- [ ] Real-time transaction ingestion from Stripe webhooks
- [ ] Revenue metrics calculation (MRR, churn, growth rate)
- [ ] Invoice generation and payment tracking
- [ ] Expense recording (manual or categorized)
- [ ] Budget creation and enforcement

### 3. Agent (Hermes + NemoClaw)
- [ ] Hermes agent running in NemoClaw sandbox
- [ ] Agent can query financial data via API
- [ ] Agent can make decisions (approve expense, flag anomaly, generate report)
- [ ] Agent decisions go through policy engine before execution
- [ ] Full audit trail of agent reasoning and actions

### 4. Dashboard (React)
- [ ] Financial overview: revenue, expenses, runway, burn rate
- [ ] Agent activity feed (what it's doing, why)
- [ ] Cash flow forecast chart
- [ ] Budget status with agent decisions

### 5. Demo
- [ ] Pre-loaded with realistic startup financial data
- [ ] 1-3 minute video showing agent making real decisions
- [ ] Clear narrative arc: problem → agent acts → result

## Nice-to-Have (If Time Permits)

- [ ] Scenario modeling ("what if we hire 3 engineers?")
- [ ] Investor report PDF generation
- [ ] Slack/Discord notifications for agent decisions
- [ ] Multi-organization support
- [ ] Agent self-improvement (learning from past decisions)

## Explicitly Out of Scope

- Authentication/authorization (demo mode is fine)
- Production deployment
- Tax/legal compliance
- Payroll
- Mobile app
- CI/CD pipeline

## Day-by-Day Plan

### Day 1 (Friday) — Foundation
- [ ] .NET solution scaffold with Clean Architecture layers
- [ ] Domain entities: Transaction, Budget, Forecast, AuditEntry, AgentDecision
- [ ] EF Core DbContext + migrations
- [ ] Stripe webhook endpoint (basic)
- [ ] Docker Compose: PostgreSQL + API
- [ ] NemoClaw setup and Hermes agent bootstrap

### Day 2 (Saturday) — Agent + Stripe
- [ ] Stripe integration: webhook processing, transaction sync
- [ ] Revenue metrics service (MRR, churn, growth)
- [ ] Agent ↔ API communication (agent queries data, submits decisions)
- [ ] Policy engine: MediatR pipeline behaviors for spending limits
- [ ] Cash flow forecasting logic (simple projection model)
- [ ] Agent prompt engineering: CFO persona, decision framework

### Day 3 (Sunday) — Dashboard + Polish
- [ ] React dashboard scaffold
- [ ] Financial overview page (metrics, charts)
- [ ] Agent activity feed
- [ ] Cash flow forecast visualization
- [ ] Pre-load demo data (realistic 3-month startup history)
- [ ] End-to-end integration testing

### Day 4 (Monday) — Demo + Submit
- [ ] Demo script: 5 scenes, 90 seconds each max
- [ ] Record demo video (screen capture + narration)
- [ ] Write Twitter thread with video
- [ ] Fill submission form
- [ ] Final polish and bug fixes

## Key Risk: Time

4 days is tight. The biggest risk is spending too long on infrastructure and not enough on the demo. **The demo is the deliverable.** Every feature we build should answer: "Does this help the demo?"

If we're behind on Day 3, we cut dashboard features and keep the agent demo compelling. A great agent demo with a basic dashboard beats a polished dashboard with a boring agent.
