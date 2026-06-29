import { useState } from 'react';
import { ChevronDown, ChevronUp, Loader2 } from 'lucide-react';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import type { ForecastResponse } from '../api/types';

const SCENARIOS = [
  { value: 'base', label: 'Base Case' },
  { value: 'hire-2-engineers', label: '+2 Engineers' },
  { value: 'hire-1-engineer', label: '+1 Engineer' },
  { value: 'expand-mrr-15', label: '+15% MRR' },
  { value: 'cut-marketing-50', label: 'Cut Marketing 50%' },
];

function getRunwayColor(days: number): string {
  if (days < 90) return 'bg-red-500';
  if (days < 180) return 'bg-amber-500';
  return 'bg-emerald-500';
}

function getRunwayTextColor(days: number): string {
  if (days < 90) return 'text-red-400';
  if (days < 180) return 'text-amber-400';
  return 'text-emerald-400';
}

function runwayPercent(days: number): number {
  return Math.min((days / (365 * 3)) * 100, 100);
}

interface ForecastChartProps {
  forecast: ForecastResponse;
  selectedScenario?: string;
  onScenarioChange?: (scenario: string) => void;
  scenarioLoading?: boolean;
}

export function ForecastChart({ forecast, selectedScenario, onScenarioChange, scenarioLoading }: ForecastChartProps) {
  const [expanded, setExpanded] = useState(false);

  const data = forecast.projections.map(p => ({
    date: new Date(p.date).toLocaleDateString('en-US', { month: 'short', year: '2-digit' }),
    balance: Math.round(p.projectedBalance),
    revenue: Math.round(p.projectedRevenue),
    expenses: Math.round(p.projectedExpenses),
  }));

  const currentScenarioLabel = SCENARIOS.find(s => s.value === forecast.scenario)?.label ?? forecast.scenario;

  return (
    <div className="bg-gray-800 rounded-xl border border-gray-700">
      {/* Compact header — always visible */}
      <div className="p-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-semibold text-white flex items-center gap-2">
            Cash Flow
            {scenarioLoading ? (
              <Loader2 size={12} className="animate-spin text-gray-400" />
            ) : (
              <span className="text-xs font-normal text-gray-500">{currentScenarioLabel}</span>
            )}
          </h2>
          <button
            onClick={() => setExpanded(prev => !prev)}
            className="text-gray-500 hover:text-gray-300 transition-colors"
          >
            {expanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
          </button>
        </div>

        {/* Scenario selector */}
        {onScenarioChange && (
          <div className="mt-2">
              <select
              value={selectedScenario ?? forecast.scenario}
              onChange={e => onScenarioChange(e.target.value)}
              disabled={scenarioLoading}
              className="w-full px-2 py-1.5 bg-gray-900 border border-gray-600 rounded text-xs text-gray-300
                         focus:outline-none focus:border-violet-500 disabled:opacity-50"
            >
              {SCENARIOS.map(s => (
                <option key={s.value} value={s.value}>{s.label}</option>
              ))}
            </select>
          </div>
        )}

        {/* Compact numbers */}
        <div className="mt-3 space-y-3">
          {/* Runway bar */}
          <div>
            <div className="flex items-baseline justify-between mb-1">
              <span className="text-xs text-gray-400">Runway</span>
              <span className={`text-sm font-bold ${getRunwayTextColor(forecast.runwayDays)}`}>
                {forecast.runwayDays > 3650 ? 'Indefinite' : `${forecast.runwayDays} days`}
              </span>
            </div>
            <div className="w-full bg-gray-700 rounded-full h-2">
              <div
                className={`h-2 rounded-full transition-all ${getRunwayColor(forecast.runwayDays)}`}
                style={{ width: `${runwayPercent(forecast.runwayDays)}%` }}
              />
            </div>
            {forecast.runwayEndDate && forecast.runwayDays <= 3650 && (
              <p className="text-xs text-gray-500 mt-1">
                Ends {new Date(forecast.runwayEndDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' })}
              </p>
            )}
          </div>

          {/* Key numbers grid */}
          <div className="grid grid-cols-3 gap-2">
            <div>
              <p className="text-xs text-gray-500">Balance</p>
              <p className="text-sm font-semibold text-white">
                ${(forecast.cashBalance / 1000).toFixed(1)}K
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Revenue</p>
              <p className="text-sm font-semibold text-emerald-400">
                ${(forecast.monthlyRevenue / 1000).toFixed(1)}K/mo
              </p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Burn</p>
              <p className="text-sm font-semibold text-red-400">
                ${(forecast.monthlyBurnRate / 1000).toFixed(1)}K/mo
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Expandable chart */}
      {expanded && (
        <div className="px-4 pb-4 border-t border-gray-700 pt-4">
          <ResponsiveContainer width="100%" height={200}>
            <AreaChart data={data}>
              <defs>
                <linearGradient id="colorBalance" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#8b5cf6" stopOpacity={0} />
                </linearGradient>
                <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#10b981" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#10b981" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
              <XAxis dataKey="date" stroke="#9ca3af" fontSize={11} />
              <YAxis stroke="#9ca3af" fontSize={11} tickFormatter={(v: number) => `$${(v / 1000).toFixed(0)}k`} />
              <Tooltip
                contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '8px', fontSize: '12px' }}
                labelStyle={{ color: '#f3f4f6' }}
                formatter={(value: number) => [`$${value.toLocaleString()}`, undefined]}
              />
              <Legend wrapperStyle={{ fontSize: '12px' }} />
              <Area type="monotone" dataKey="balance" stroke="#8b5cf6" fill="url(#colorBalance)" name="Balance" />
              <Area type="monotone" dataKey="revenue" stroke="#10b981" fill="url(#colorRevenue)" name="Revenue" />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}
