export interface DashboardSummary {
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;
  currency: string;
  transactionCount: number;
  budgetCount: number;
  overBudgetCount: number;
  recentDecisions: AgentDecision[];
}

export interface AgentDecision {
  id: string;
  type: string;
  description: string;
  reasoning: string;
  status: string;
  createdAt: string;
  executedAt: string | null;
}

export interface ForecastResponse {
  id: string;
  cashBalance: number;
  currency: string;
  monthlyBurnRate: number;
  monthlyRevenue: number;
  runwayDays: number;
  runwayEndDate: string;
  scenario: string;
  confidence: string;
  projections: ProjectionPoint[];
}

export interface ProjectionPoint {
  date: string;
  projectedBalance: number;
  projectedRevenue: number;
  projectedExpenses: number;
}

export interface RevenueMetrics {
  monthlyRecurringRevenue: number;
  currency: string;
  previousMonthRevenue: number;
  growthRatePercent: number;
  churnRatePercent: number;
  averageRevenuePerUser: number;
  activeSubscriptions: number;
  churnedSubscriptions: number;
  netRevenueRetentionPercent: number;
  monthlyTrend: MonthlyRevenuePoint[];
}

export interface MonthlyRevenuePoint {
  month: string;
  revenue: number;
  transactionCount: number;
}

export interface BudgetResponse {
  id: string;
  category: string;
  monthlyLimit: number;
  currency: string;
  currentSpend: number;
  percentUsed: number;
  isNearLimit: boolean;
  isOverBudget: boolean;
}

export interface ExpenseEvaluation {
  id: string;
  type: string;
  description: string;
  reasoning: string;
  status: string;
  createdAt: string;
}

export interface ExpenseRecordResult {
  transactionId: string;
  approved: boolean;
  rejectionReason: string | null;
}

export interface AuditEntry {
  id: string;
  actor: string;
  actorId: string;
  action: string;
  entityType: string;
  entityId: string;
  createdAt: string;
  correlationId: string;
}

export interface SummaryResponse {
  summary: string;
}

export interface SeedResponse {
  organizationId: string;
  profile: string;
  organizationName: string;
  transactionCount: number;
  budgetCount: number;
  decisionCount: number;
  message: string;
}

export interface ProfileOption {
  value: string;
  label: string;
  tagline: string;
  description: string;
}

export const PROFILES: ProfileOption[] = [
  {
    value: 'growth-saas',
    label: 'NovaCRM',
    tagline: 'Growth SaaS · $23K MRR · Healthy',
    description: 'B2B SaaS with 12% MoM growth. 14+ months runway. Agent recommends strategic investments.',
  },
  {
    value: 'cash-crunch',
    label: 'ByteStack',
    tagline: 'Cash Crunch · $5.4K MRR · 3mo Runway',
    description: 'Dev tools startup with declining revenue. Agent flags urgent cost cuts needed.',
  },
  {
    value: 'hypergrowth',
    label: 'LaunchPad AI',
    tagline: 'Hypergrowth · $52K MRR · 6mo Runway',
    description: 'AI platform growing 22% MoM. High burn rate. Agent warns about Series B timing.',
  },
  {
    value: 'pre-seed',
    label: 'FreshStack',
    tagline: 'Pre-Seed · $1.1K MRR · Solo Founder',
    description: 'Bootstrapped solo founder. First paying customers. Agent guides every dollar.',
  },
];
