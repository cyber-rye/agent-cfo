import type { DashboardSummary, ForecastResponse, RevenueMetrics, BudgetResponse, AgentDecision } from './types';

const BASE = '/api';

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE}${path}`);
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return res.json();
}

async function post<T>(path: string, body?: unknown): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    method: 'POST',
    headers: body ? { 'Content-Type': 'application/json' } : {},
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return res.json();
}

export const api = {
  getDashboard: (orgId: string) => get<DashboardSummary>(`/dashboard/${orgId}`),
  getForecast: (orgId: string) => get<ForecastResponse>(`/forecasts/${orgId}`),
  getRevenueMetrics: (orgId: string) => get<RevenueMetrics>(`/forecasts/${orgId}/revenue`),
  getBudgets: (orgId: string) => get<BudgetResponse[]>(`/budgets/${orgId}`),
  getDecisions: (orgId: string, limit = 20) => get<AgentDecision[]>(`/agent/${orgId}/decisions?limit=${limit}`),
  runFullAnalysis: (orgId: string) => post(`/agent/${orgId}/run-full-analysis`),
  seedDemo: () => post<{ organizationId: string }>('/seed/demo'),
};