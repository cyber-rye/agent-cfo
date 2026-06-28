import { TrendingUp, TrendingDown, Minus } from 'lucide-react';

interface MetricCardProps {
  label: string;
  value: string;
  subtext?: string;
  trend?: 'up' | 'down' | 'neutral';
  trendValue?: string;
}

export function MetricCard({ label, value, subtext, trend, trendValue }: MetricCardProps) {
  const trendColor = trend === 'up' ? 'text-emerald-400' : trend === 'down' ? 'text-red-400' : 'text-gray-400';
  const TrendIcon = trend === 'up' ? TrendingUp : trend === 'down' ? TrendingDown : Minus;

  return (
    <div className="bg-gray-800 rounded-xl p-6 border border-gray-700">
      <p className="text-gray-400 text-sm font-medium mb-1">{label}</p>
      <p className="text-3xl font-bold text-white tracking-tight">{value}</p>
      <div className="flex items-center gap-2 mt-2">
        {trend && (
          <span className={`flex items-center gap-1 text-sm ${trendColor}`}>
            <TrendIcon size={14} />
            {trendValue}
          </span>
        )}
        {subtext && <span className="text-gray-500 text-sm">{subtext}</span>}
      </div>
    </div>
  );
}