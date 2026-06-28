import { useState, useEffect, useCallback } from 'react';
import { RefreshCw, Zap, DollarSign, WifiOff } from 'lucide-react';
import { api } from './api/client';
import type { DashboardSummary, ForecastResponse, RevenueMetrics, BudgetResponse, AgentDecision } from './api/types';
import { MetricCard } from './components/MetricCard';
import { AgentFeed } from './components/AgentFeed';
import { ForecastChart } from './components/ForecastChart';
import { BudgetStatus } from './components/BudgetStatus';
import { ToastContainer, useToasts } from './components/Toast';

const ORG_KEY = 'agentcfo_org_id';

type AgentStatus = 'idle' | 'analyzing' | 'alert';

export default function App() {
  const [orgId, setOrgId] = useState<string | null>(() => {
    const stored = localStorage.getItem(ORG_KEY);
    return stored && stored !== 'null' ? stored : null;
  });
  const [loading, setLoading] = useState(false);
  const [seeding, setSeeding] = useState(false);
  const [analyzing, setAnalyzing] = useState(false);
  const [agentStatus, setAgentStatus] = useState<AgentStatus>('idle');
  const [apiError, setApiError] = useState(false);
  const [dashboard, setDashboard] = useState<DashboardSummary | null>(null);
  const [forecast, setForecast] = useState<ForecastResponse | null>(null);
  const [revenue, setRevenue] = useState<RevenueMetrics | null>(null);
  const [budgets, setBudgets] = useState<BudgetResponse[]>([]);
  const [decisions, setDecisions] = useState<AgentDecision[]>([]);
  const { toasts, addToast, dismissToast } = useToasts();

  const loadData = useCallback(async (id: string) => {
    setLoading(true);
    setApiError(false);
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

      // Check if any anomaly alerts exist
      const hasAlerts = dec.some(d => d.type === 'AnomalyDetected' || d.type === 'AlertRaised');
      if (hasAlerts) setAgentStatus('alert');
    } catch (err) {
      console.error('Failed to load data:', err);
      setApiError(true);
      // If 404, org doesn't exist — clear localStorage
      if (err instanceof Error && err.message.includes('404')) {
        localStorage.removeItem(ORG_KEY);
        setOrgId(null);
      }
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (orgId) loadData(orgId);
  }, [orgId, loadData]);

  const handleSeed = async () => {
    setSeeding(true);
    setApiError(false);
    try {
      const result = await api.seedDemo();
      localStorage.setItem(ORG_KEY, result.organizationId);
      setOrgId(result.organizationId);
      addToast('ReportGenerated', 'Demo Data Loaded', '90 transactions, 5 budgets, 4 agent decisions');
    } catch (err) {
      console.error('Seed failed:', err);
      setApiError(true);
    } finally {
      setSeeding(false);
    }
  };

  const handleAnalyze = async () => {
    if (!orgId) return;
    setAnalyzing(true);
    setAgentStatus('analyzing');
    try {
      await api.runFullAnalysis(orgId);
      await loadData(orgId);

      // Show toasts for new decisions
      const updatedDecisions = await api.getDecisions(orgId, 5);
      const newOnes = updatedDecisions.filter(d => !decisions.some(existing => existing.id === d.id));
      for (const d of newOnes.slice(0, 3)) {
        addToast(d.type, d.description, d.reasoning.slice(0, 100) + '...');
      }

      const hasAlerts = updatedDecisions.some(d => d.type === 'AnomalyDetected' || d.type === 'AlertRaised');
      setAgentStatus(hasAlerts ? 'alert' : 'idle');
    } catch (err) {
      console.error('Analysis failed:', err);
    } finally {
      setAnalyzing(false);
      if (agentStatus === 'analyzing') setAgentStatus('idle');
    }
  };

  const formatMoney = (amount: number, currency = 'USD') =>
    new Intl.NumberFormat('en-US', { style: 'currency', currency, maximumFractionDigits: 0 }).format(amount);

  const statusColors = {
    idle: 'bg-gray-500',
    analyzing: 'bg-emerald-500 animate-pulse',
    alert: 'bg-amber-500 animate-pulse',
  };
  const statusLabels = {
    idle: 'Agent: Idle',
    analyzing: 'Agent: Analyzing...',
    alert: 'Agent: Alert',
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Toast notifications */}
      <ToastContainer toasts={toasts} onDismiss={dismissToast} />

      {/* Error banner */}
      {apiError && (
        <div className="bg-red-900/80 border-b border-red-700 px-6 py-2 text-center text-sm text-red-200 flex items-center justify-center gap-2">
          <WifiOff size={14} />
          Unable to connect to AgentCFO API — is the server running?
          <button onClick={() => setApiError(false)} className="text-red-400 hover:text-red-200 underline ml-2">Dismiss</button>
        </div>
      )}

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
            {/* Agent status indicator */}
            {orgId && (
              <div className="flex items-center gap-2 text-xs text-gray-400">
                <span className={`w-2 h-2 rounded-full ${statusColors[agentStatus]}`} />
                <span className="hidden sm:inline">{statusLabels[agentStatus]}</span>
              </div>
            )}
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

        {/* Founder persona banner */}
        {orgId && dashboard && (
          <div className="border-t border-gray-800/50 bg-gray-900/60">
            <div className="max-w-7xl mx-auto px-6 py-2 flex items-center gap-4 text-xs text-gray-500">
              <span className="text-gray-400 font-medium">Acme SaaS</span>
              <span>·</span>
              <span>Founded 2025</span>
              <span>·</span>
              <span>Solo Founder</span>
              <span>·</span>
              <span>3 Employees</span>
              <span>·</span>
              <span>{revenue ? formatMoney(revenue.monthlyRecurringRevenue) + ' MRR' : '—'}</span>
            </div>
          </div>
        )}
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
              An autonomous financial agent for startups. It watches your Stripe revenue,
              enforces your budget, forecasts your runway, and makes decisions with reasoning.
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
