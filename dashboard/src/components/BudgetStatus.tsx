import type { BudgetResponse } from '../api/types';

const categoryNames: Record<string, string> = {
  Infrastructure: 'Infrastructure',
  Marketing: 'Marketing',
  Tools: 'Tools',
  Contractors: 'Contractors',
  Office: 'Office',
  Other: 'Other',
  Legal: 'Legal',
  Subscription: 'Subscriptions',
};

function getBarColor(percent: number): string {
  if (percent >= 90) return 'bg-red-500';
  if (percent >= 75) return 'bg-amber-500';
  return 'bg-emerald-500';
}

export function BudgetStatus({ budgets }: { budgets: BudgetResponse[] }) {
  return (
    <div className="bg-gray-800 rounded-xl p-6 border border-gray-700">
      <h2 className="text-lg font-semibold text-white mb-4">Budget Status</h2>
      <div className="space-y-4">
        {budgets.map(b => (
          <div key={b.id}>
            <div className="flex justify-between items-center mb-1">
              <span className="text-sm text-gray-300">{categoryNames[b.category] || b.category}</span>
              <span className="text-sm text-gray-400">
                ${b.currentSpend.toLocaleString()} / ${b.monthlyLimit.toLocaleString()}
              </span>
            </div>
            <div className="w-full bg-gray-700 rounded-full h-2.5">
              <div
                className={`h-2.5 rounded-full transition-all ${getBarColor(b.percentUsed)}`}
                style={{ width: `${Math.min(b.percentUsed, 100)}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}