# AgentCFO

**An autonomous financial operations agent for funding-stage startups.**

AgentCFO is a Hermes AI agent running in an NVIDIA NemoClaw sandbox that autonomously manages a startup's financial operations — revenue tracking, expense governance, cash flow forecasting, and investor-readiness — all orchestrated through a .NET control plane with Stripe as the financial backbone.

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
│  │  Inference: Nemotron 3 Ultra                    │  │
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
| **Frontend** | React + TypeScript (dashboard) |
| **Agent** | Hermes Agent in NemoClaw sandbox |
| **Inference** | Nemotron 3 Ultra (via NVIDIA) |
| **Payments** | Stripe (subscriptions, invoices, payment links) |
| **Container** | Docker, Docker Compose |

## Project Structure

```
agent-cfo/
├── src/
│   ├── AgentCfo.Api/              # ASP.NET Core Web API
│   ├── AgentCfo.Core/             # Domain entities, interfaces, domain events
│   ├── AgentCfo.Application/      # Use cases, CQRS (MediatR), policies
│   ├── AgentCfo.Infrastructure/   # EF Core, Stripe SDK, external services
│   └── AgentCfo.Agent/            # Hermes agent integration, NemoClaw config
├── dashboard/                     # React + TypeScript frontend
├── docs/                          # Architecture, vision, scope docs
├── docker/                        # Dockerfiles, compose, NemoClaw config
└── tests/                         # xUnit tests
```

## Quick Start

> ⚠️ Under active development — Hackathon submission due EOD Tuesday, June 30.

```bash
# Prerequisites
# .NET 10 SDK, Docker, Node 22+, Stripe test account

# Clone and run
git clone <repo>
cd agent-cfo
docker compose up -d          # PostgreSQL + NemoClaw
dotnet run --project src/AgentCfo.Api
cd dashboard && npm install && npm run dev
```

## License

MIT
