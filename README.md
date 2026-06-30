# AgentCFO

**An autonomous financial operations agent for funding-stage startups.**

AgentCFO is an AI-powered financial agent that autonomously manages a startup's financial operations — revenue tracking, expense governance, cash flow forecasting, and investor-readiness — running in an NVIDIA NemoClaw sandbox with Stripe as the financial backbone.

## The Problem

Early-stage startup founders spend 30-40% of their time on financial operations they're not equipped for: chasing invoices, tracking burn rate, forecasting runway, preparing investor reports, and making spending decisions without data. Hiring a CFO costs $15K+/month. Traditional accounting software requires manual input and financial literacy.

## The Solution

An AI agent that *is* your CFO. It watches your Stripe transactions in real-time, understands your financial position, makes governance decisions within guardrails you set, and produces the reports investors actually want to see — all running in a security-sandboxed environment with full audit trails.

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                   .NET 10 Control Plane              │
│  ┌──────────┐  ┌──────────┐  ┌───────────────────┐  │
│  │  API      │  │ Dashboard │  │ Policy Engine     │  │
│  │  (REST)   │  │ (React)   │  │ (MediatR Pipeline)│  │
│  └────┬─────┘  └────┬─────┘  └────────┬──────────┘  │
│       │              │                 │              │
│  ┌────┴──────────────┴─────────────────┴──────────┐  │
│  │              Domain Layer (EF Core)             │  │
│  │  Transactions · Budgets · Forecasts · Alerts   │  │
│  └────────────────────┬───────────────────────────┘  │
└───────────────────────┼──────────────────────────────┘
                        │
┌───────────────────────┼──────────────────────────────┐
│              NemoClaw Sandbox                         │
│  ┌────────────────────┴───────────────────────────┐  │
│  │           Hermes Agent (AgentCFO)               │  │
│  │  Financial analysis · Decision making · Reports │  │
│  │  Inference: Nemotron 3 Ultra (via OpenRouter)   │  │
│  └────────────────────┬───────────────────────────┘  │
│                       │ Network Policy                │
└───────────────────────┼──────────────────────────────┘
                        │
              ┌─────────┴─────────┐
              │     Stripe API    │
              │  Revenue · Bills  │
              │  · Subscriptions  │
              └───────────────────┘
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **API** | ASP.NET Core 10, MediatR, FluentValidation |
| **Data** | EF Core, PostgreSQL |
| **Frontend** | React + TypeScript + Tailwind + Recharts |
| **Agent** | Hermes Agent in NemoClaw sandbox |
| **Inference** | Nemotron 3 Ultra / Super 120B (via OpenRouter, free) |
| **Payments** | Stripe (subscriptions, invoices, payment links) |
| **Container** | Docker, Docker Compose |

## Live Demo

**https://agentcfo.ryeye.xyz/**

## Quick Start

```bash
# Prerequisites
# .NET 10 SDK, Docker, Node 22+, Stripe test account

# Clone and run
git clone https://github.com/cyber-rye/agent-cfo.git
cd agent-cfo

# Start PostgreSQL
docker compose up -d postgres

# Start API (with env vars for LLM + Stripe)
bash start-api.sh

# Start Dashboard
cd dashboard && npm install && npm run build
cd .. && node serve.cjs
```

## Project Structure

```
agent-cfo/
├── src/
│   ├── AgentCfo.Api/              # ASP.NET Core Web API
│   ├── AgentCfo.Core/             # Domain entities, interfaces, domain events
│   ├── AgentCfo.Application/      # Use cases, CQRS (MediatR), policies
│   ├── AgentCfo.Infrastructure/   # EF Core, Stripe SDK, LLM service, external services
│   └── AgentCfo.Agent/            # Hermes agent integration, NemoClaw config
├── dashboard/                     # React + TypeScript frontend
├── docs/                          # Architecture, vision, scope docs
├── docker/                        # Dockerfiles, compose, NemoClaw config
├── start-api.sh                   # API startup script with env vars
└── serve.cjs                      # Dashboard static server + API proxy
```

## License

MIT
