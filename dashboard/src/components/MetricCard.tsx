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
    <div className="bg-gray-800 rounded-xl p-4 border border-gray-700">
      <p className="text-gray-400 text-xs font-medium mb-1">{label}</p>
      <p className="text-2xl font-bold text-white tracking-tight">{value}</p>
      <div className="flex items-center gap-2 mt-1.5">
        {trend && (
          <span className={`flex items-center gap-1 text-xs ${trendColor}`}>
            <TrendIcon size={12} />
            {trendValue}
          </span>
        )}
        {subtext && <span className="text-gray-500 text-xs">{subtext}</span>}
      </div>
    </div>
  );
}
