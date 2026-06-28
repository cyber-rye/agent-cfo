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