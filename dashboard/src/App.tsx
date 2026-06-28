import { useState, useEffect, useCallback } from 'react';
import { RefreshCw, Zap, DollarSign } from 'lucide-react';
import { api } from './api/client';
import type { DashboardSummary, ForecastResponse, RevenueMetrics, BudgetResponse, AgentDecision } from './api/types';
import { MetricCard } from './components/MetricCard';
import { AgentFeed } from './components/AgentFeed';
import { ForecastChart } from './components/ForecastChart';
import { BudgetStatus } from './components/BudgetStatus';

const ORG_KEY = 'agentcfo_org_id';

export default function App() {
  const [orgId, setOrgId] = useState<string | null>(localStorage.getItem(ORG_KEY));
  const [loading, setLoading] = useState(false);
  const [seeding, setSeeding] = useState(false);
  const [analyzing, setAnalyzing] = useState(false);
  const [dashboard, setDashboard] = useState<DashboardSummary | null>(null);
  const [forecast, setForecast] = useState<ForecastResponse | null>(null);
  const [revenue, setRevenue] = useState<RevenueMetrics | null>(null);
  const [budgets, setBudgets] = useState<BudgetResponse[]>([]);
  const [decisions, setDecisions] = useState<AgentDecision[]>([]);

  const loadData = useCallback(async (id: string) => {
    setLoading(true);
    try {
      const [dash, fc, rev, bud, dec] = await Promise.all([
        api.getDashboard(id),
        api.getForecast(id),
        api.getRevenueMetrics(id),
        api.getBudgets(id),
        api.getDecisions(id),
      ]);
      setDashboard(dash);
      setForecast(fc);
      setRevenue(rev);
      setBudgets(bud);
      setDecisions(dec);
    } catch (err) {
      console.error('Failed to load data:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (orgId) loadData(orgId);
  }, [orgId, loadData]);

  const handleSeed = async () => {
    setSeeding(true);
    try {
      const result = await api.seedDemo();
      localStorage.setItem(ORG_KEY, result.organizationId);
      setOrgId(result.organizationId);
    } catch (err) {
      console.error('Seed failed:', err);
    } finally {
      setSeeding(false);
    }
  };

  const handleAnalyze = async () => {
    if (!orgId) return;
    setAnalyzing(true);
    try {
      await api.runFullAnalysis(orgId);
      await loadData(orgId);
    } catch (err) {
      console.error('Analysis failed:', err);
    } finally {
      setAnalyzing(false);
    }
  };

  const formatMoney = (amount: number, currency = 'USD') =>
    new Intl.NumberFormat('en-US', { style: 'currency', currency, maximumFractionDigits: 0 }).format(amount);

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Header */}
      <header className="border-b border-gray-800 bg-gray-900/80 backdrop-blur-sm sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 bg-violet-600 rounded-lg flex items-center justify-center">
              <DollarSign size={18} />
            </div>
            <h1 className="text-xl font-bold tracking-tight">AgentCFO</h1>
            <span className="text-xs text-gray-500 border border-gray-700 rounded px-2 py-0.5">Autonomous Financial Agent</span>
          </div>
          <div className="flex items-center gap-3">
            {orgId && (
              <button
                onClick={handleAnalyze}
                disabled={analyzing}
                className="flex items-center gap-2 px-4 py-2 bg-violet-600 hover:bg-violet-700 disabled:opacity-50 rounded-lg text-sm font-medium transition-colors"
              >
                <Zap size={14} className={analyzing ? 'animate-spin' : ''} />
                {analyzing ? 'Analyzing...' : 'Run Analysis'}
              </button>
            )}
            <button
              onClick={() => orgId && loadData(orgId)}
              disabled={loading || !orgId}
              className="p-2 text-gray-400 hover:text-white disabled:opacity-50 transition-colors"
              title="Refresh"
            >
              <RefreshCw size={18} className={loading ? 'animate-spin' : ''} />
            </button>
            <button
              onClick={handleSeed}
              disabled={seeding}
              className="px-4 py-2 bg-gray-800 hover:bg-gray-700 disabled:opacity-50 rounded-lg text-sm font-medium border border-gray-700 transition-colors"
            >
              {seeding ? 'Seeding...' : 'Seed Demo Data'}
            </button>
          </div>
        </div>
      </header>

      {/* Content */}
      <main className="max-w-7xl mx-auto px-6 py-8">
        {!orgId ? (
          <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6">
            <div className="w-16 h-16 bg-violet-600/20 rounded-2xl flex items-center justify-center">
              <DollarSign size={32} className="text-violet-400" />
            </div>
            <h2 className="text-2xl font-bold">Welcome to AgentCFO</h2>
            <p className="text-gray-400 max-w-md text-center">
              An autonomous financial agent for startups. Seed demo data to get started.
            </p>
            <button
              onClick={handleSeed}
              disabled={seeding}
              className="px-6 py-3 bg-violet-600 hover:bg-violet-700 disabled:opacity-50 rounded-lg text-sm font-medium transition-colors"
            >
              {seeding ? 'Seeding...' : 'Seed Demo Data'}
            </button>
          </div>
        ) : (
          <div className="space-y-6">
            {/* Metric Cards */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <MetricCard
                label="Monthly Recurring Revenue"
                value={revenue ? formatMoney(revenue.monthlyRecurringRevenue) : '—'}
                trend={revenue && revenue.growthRatePercent > 0 ? 'up' : revenue && revenue.growthRatePercent < 0 ? 'down' : 'neutral'}
                trendValue={revenue ? `${revenue.growthRatePercent > 0 ? '+' : ''}${revenue.growthRatePercent.toFixed(1)}% MoM` : undefined}
              />
              <MetricCard
                label="Monthly Burn Rate"
                value={forecast ? formatMoney(forecast.monthlyBurnRate) : '—'}
                subtext={forecast ? `Revenue: ${formatMoney(forecast.monthlyRevenue)}` : undefined}
                trend="down"
                trendValue={forecast ? `Net: ${formatMoney(forecast.monthlyRevenue - forecast.monthlyBurnRate)}` : undefined}
              />
              <MetricCard
                label="Runway"
                value={forecast ? (forecast.runwayDays > 3650 ? '∞' : `${forecast.runwayDays} days`) : '—'}
                subtext={forecast?.runwayEndDate ? `Ends ${new Date(forecast.runwayEndDate).toLocaleDateString()}` : undefined}
                trend={forecast && forecast.runwayDays > 180 ? 'up' : 'down'}
                trendValue={forecast?.confidence}
              />
              <MetricCard
                label="Net Income (MTD)"
                value={dashboard ? formatMoney(dashboard.netIncome) : '—'}
                subtext={dashboard ? `${dashboard.transactionCount} transactions` : undefined}
                trend={dashboard && dashboard.netIncome >= 0 ? 'up' : 'down'}
                trendValue={dashboard ? `Revenue: ${formatMoney(dashboard.totalRevenue)}` : undefined}
              />
            </div>

            {/* Charts and Feed */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <div className="lg:col-span-2 space-y-6">
                {forecast && <ForecastChart forecast={forecast} />}
                {budgets.length > 0 && <BudgetStatus budgets={budgets} />}
              </div>
              <div>
                <AgentFeed decisions={decisions} />
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
