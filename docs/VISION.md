# Vision

## What Is AgentCFO?

AgentCFO is an autonomous financial agent for funding-stage startups. It replaces the spreadsheet-and-gut-feeling approach to early-stage finance with an AI agent that actually understands your financial position and acts on it.

## Who Is It For?

**The solo founder or small team (2-5 people) who:**
- Has Stripe processing their revenue (SaaS, marketplace, services)
- Doesn't have a CFO, accountant, or finance person
- Makes spending decisions based on vibes, not data
- Needs to answer "how much runway do we have left?" without building a spreadsheet
- Will eventually need to show investors a financial picture

## What Does It Do?

### Revenue Intelligence
- Monitors Stripe transactions in real-time
- Tracks MRR, churn, expansion revenue, and net revenue retention
- Identifies payment failures and automatically retries or flags them
- Detects revenue anomalies (unexpected drops, unusual spikes)

### Expense Governance
- Enforces spending budgets per category (infra, tools, marketing, etc.)
- Approves or rejects expense requests against policy
- Tracks recurring vs one-time expenses
- Alerts when spending patterns deviate from plan

### Cash Flow Forecasting
- Projects runway based on current burn rate and revenue trajectory
- Models scenarios: "What if we hire 2 engineers?" "What if MRR grows 15%?"
- Alerts when runway drops below configurable thresholds
- Produces 30/60/90 day cash flow projections

### Investor Readiness
- Generates financial summaries investors expect (MRR, burn, runway, unit economics)
- Tracks key SaaS metrics: LTV, CAC payback, gross margin
- Produces board-ready monthly financial reports
- Flags when the company is "fundraise-ready" based on financial metrics

### Self-Governance
- Every decision has an audit trail with reasoning
- Spending limits are enforced at the NemoClaw sandbox level (network policy)
- The agent cannot exceed its authority — policy engine blocks out-of-bounds actions
- Transparent: every action the agent takes is visible in the dashboard

## What Does It NOT Do? (Scope Boundaries)

- ❌ Tax filing or compliance (that's an accountant's job)
- ❌ Payroll processing (too sensitive, too regulated)
- ❌ Legal or contractual decisions
- ❌ Direct investor communication (it prepares data, you present it)
- ❌ Multi-entity or subsidiary management
- ❌ Inventory or supply chain management

## The Pitch

> "Every startup founder is a part-time CFO with none of the training.
> AgentCFO gives them an autonomous financial brain that watches their
> Stripe revenue, enforces their budget, forecasts their runway, and
> prepares them for their next fundraise — all running in a sandboxed
> environment with guardrails that prevent it from going rogue.
> It's not a dashboard. It's a decision-maker."

## Design Principles

1. **Deep, not wide.** One agent, one domain, done right.
2. **Trust through transparency.** Every decision is logged and explainable.
3. **Guardrails, not guardtowers.** The agent has real authority within defined limits.
4. **Stripe-native.** We don't replicate what Stripe already does well.
5. **Enterprise-grade .NET.** This isn't a script — it's production architecture.
6. **Show, don't tell.** The dashboard makes the agent's thinking visible.
