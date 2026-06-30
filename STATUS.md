# AgentCFO — Project Status

**Updated:** Monday, June 30, 2026
**Deadline:** EOD Tuesday, June 30 (today)
**Submission:** 1–3 min demo video on X/Twitter + submission form
**Live Demo:** https://agentcfo.ryeye.xyz/
**Repo:** https://github.com/cyber-rye/agent-cfo.git

---

## Current Status: Demo-Ready

### What's Built

#### Backend (.NET 10 + Clean Architecture)
- **6 entities** — Organization, Transaction, Budget, Forecast, AgentDecision, AuditEntry
- **5 repositories** — all with EF Core + PostgreSQL
- **4 services** — StripeWebhookService, RevenueMetricsService, ForecastService, AgentService
- **2 pipeline behaviors** — AuditBehavior (logging), PolicyBehavior (budget enforcement)
- **8 controllers** — 22 API endpoints total
- **LLM integration** — OpenRouter → Nemotron 3 Super 120B (free), real AI-generated reasoning
- **Stripe integration** — Customer creation, subscriptions, payment links via Stripe.NET SDK
- **Auto-migrate** on startup
- **Docker Compose** — PostgreSQL + API

#### Dashboard (React + Vite + Tailwind + Recharts)
- **Agent-first layout** — Agent feed takes 2/3 width, context sidebar at 1/3
- **MetricCard** — MRR, burn rate, runway, net income with trend arrows
- **AgentFeed** — Expandable reasoning with typewriter effect, loading indicator during analysis
- **ExpenseEvaluator** — Submit expense → agent evaluates with LLM reasoning → approve/deny → record
- **ForecastChart** — Collapsible with scenario selector (Base Case, +2 Engineers, +15% MRR, etc.)
- **BudgetStatus** — Progress bars per category with color thresholds
- **AuditTrail** — Recent system events with actor icons and relative timestamps
- **GovernancePanel** — NemoClaw sandbox enforcement details, expanded by default
- **StripeConnection** — Simulated Stripe sync with customer/MRR/event summary
- **FinancialSummary** — On-demand markdown report from agent
- **QuickActions** — Individual agent capability buttons (anomalies, forecast, summary)
- **Toast notifications** — Color-coded by decision type
- **Agent status indicator** — Idle/Analyzing/Alert with animated dot
- **Founder persona banner** — "NovaCRM · B2B SaaS · Growing 10% MoM"
- **Keyboard shortcuts** — A = Analyze, R = Refresh
- **Sponsor branding** — Hermes · NemoClaw · Stripe in header and welcome screen

#### LLM Integration
- **Provider:** OpenRouter → Nemotron 3 Ultra (primary, free) → Nemotron 3 Super 120B (fallback, free)
- **Fallback:** NVIDIA API direct (deepseek-v3.2) → deterministic templates
- **Used by:** Anomaly detection, expense evaluation, financial summary, forecast reasoning
- **Response time:** ~5-8 seconds (8s timeout on Ultra, Super 120B responds in <1s)
- **Evidence:** Different reasoning text each call, references real budget numbers

#### Stripe Integration
- **Test mode:** Real Stripe test customer + subscription creation
- **Payment Links:** Agent can create real Stripe Payment Links
- **Webhook service:** Handles payment_intent.succeeded, invoice.paid, etc.
- **Dashboard:** Simulated sync flow showing customers, MRR, recent events

#### Deployment
- **VM:** Oracle Cloud (Ubuntu 22.04 ARM64)
- **API:** Port 5077 (dotnet process)
- **Dashboard:** Port 3000 (serve.cjs static server + API proxy)
- **Tunnel:** Cloudflare named tunnel → https://agentcfo.ryeye.xyz/
- **Database:** PostgreSQL via Docker (port 5432)
- **Startup:** `bash start-api.sh` (API) + `node serve.cjs` (dashboard)

---

## Demo Flow

1. **Welcome screen** — "Powered by Hermes · NVIDIA NemoClaw · Stripe"
2. **Load Demo Data** → auto-triggers analysis with LLM reasoning
3. **Agent Feed** — "Agent Thinking" indicator → typewriter reasoning appears
4. **Ask the Agent** — Submit expense → LLM evaluates → approve/deny → record to books
5. **Forecast Chart** — Scenario selector: "What if we hire 2 engineers?"
6. **Governance Panel** — NemoClaw sandbox enforcement visible
7. **Stripe Connection** — Simulated sync showing revenue data

---

## Remaining Work

| Task | Priority | Status |
|------|----------|--------|
| Record demo video (90-120s) | Required | ⬜ |
| Post to X/Twitter with video | Required | ⬜ |
| Fill submission form | Required | ⬜ |
