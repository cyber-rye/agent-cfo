import type {
  DashboardSummary, ForecastResponse, RevenueMetrics,
  BudgetResponse, AgentDecision, ExpenseEvaluation,
  ExpenseRecordResult, AuditEntry, SummaryResponse,
} from './types';

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
  // Dashboard
  getDashboard: (orgId: string) => get<DashboardSummary>(`/dashboard/${orgId}`),

  // Forecast
  getForecast: (orgId: string, scenario?: string) =>
    get<ForecastResponse>(`/forecasts/${orgId}${scenario ? `?scenario=${encodeURIComponent(scenario)}` : ''}`),
  getRevenueMetrics: (orgId: string) => get<RevenueMetrics>(`/forecasts/${orgId}/revenue`),

  // Budgets
  getBudgets: (orgId: string) => get<BudgetResponse[]>(`/budgets/${orgId}`),

  // Agent
  getDecisions: (orgId: string, limit = 20) =>
    get<AgentDecision[]>(`/agent/${orgId}/decisions?limit=${limit}`),
  // Agent — individual actions
  runFullAnalysis: (orgId: string) => post(`/agent/${orgId}/run-full-analysis`),
  detectAnomalies: (orgId: string) => post(`/agent/${orgId}/detect-anomalies`),
  generateForecast: (orgId: string, scenario?: string) =>
    post(`/agent/${orgId}/generate-forecast${scenario ? `?scenario=${encodeURIComponent(scenario)}` : ''}`),
  generateSummary: (orgId: string) => post<SummaryResponse>(`/agent/${orgId}/generate-summary`),

  // Expense evaluation (agent evaluates, then we can record)
  evaluateExpense: (orgId: string, amount: number, description: string, currency = 'USD') =>
    post<ExpenseEvaluation>(`/agent/${orgId}/evaluate-expense`, { amount, description, currency }),

  // Record expense to books (after approval)
  recordExpense: (orgId: string, amount: number, description: string, category: number, currency = 'USD') =>
    post<ExpenseRecordResult>('/transactions/expense', {
      organizationId: orgId, amount, currency, description, category,
    }),

  // Audit trail
  getAuditTrail: (orgId: string, limit = 15) =>
    get<AuditEntry[]>(`/agent/${orgId}/audit?limit=${limit}`),

  // Stripe integration
  createStripeCustomer: (orgId: string) =>
    post<{ customerId: string; message: string }>(`/stripe/${orgId}/create-customer`),
  createStripeSubscription: (orgId: string) =>
    post<{ subscriptionId: string; status: string; message: string }>(`/stripe/${orgId}/create-subscription`),
  createStripePaymentLink: (amount: number, description: string) =>
    post<{ url: string; id: string; message: string }>('/stripe/create-payment-link', { amount, description }),

  // Seed
  seedDemo: () => post<{ organizationId: string }>('/seed/demo'),
};
