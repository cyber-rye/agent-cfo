import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import type { ForecastResponse } from '../api/types';

export function ForecastChart({ forecast }: { forecast: ForecastResponse }) {
  const data = forecast.projections.map(p => ({
    date: new Date(p.date).toLocaleDateString('en-US', { month: 'short', year: '2-digit' }),
    balance: Math.round(p.projectedBalance),
    revenue: Math.round(p.projectedRevenue),
    expenses: Math.round(p.projectedExpenses),
  }));

  return (
    <div className="bg-gray-800 rounded-xl p-6 border border-gray-700">
      <h2 className="text-lg font-semibold text-white mb-1">Cash Flow Forecast</h2>
      <p className="text-gray-400 text-sm mb-4">
        Runway: <span className={forecast.runwayDays < 90 ? 'text-red-400 font-medium' : 'text-emerald-400 font-medium'}>
          {forecast.runwayDays > 3650 ? 'Indefinite' : `${forecast.runwayDays} days`}
        </span>
        {' · '}Scenario: {forecast.scenario}
      </p>
      <ResponsiveContainer width="100%" height={280}>
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
          <XAxis dataKey="date" stroke="#9ca3af" fontSize={12} />
          <YAxis stroke="#9ca3af" fontSize={12} tickFormatter={(v: number) => `$${(v / 1000).toFixed(0)}k`} />
          <Tooltip
            contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '8px' }}
            labelStyle={{ color: '#f3f4f6' }}
            formatter={(value: number) => [`$${value.toLocaleString()}`, undefined]}
          />
          <Legend />
          <Area type="monotone" dataKey="balance" stroke="#8b5cf6" fill="url(#colorBalance)" name="Balance" />
          <Area type="monotone" dataKey="revenue" stroke="#10b981" fill="url(#colorRevenue)" name="Revenue" />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}