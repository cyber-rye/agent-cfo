import { ExternalLink, TrendingUp, Users, AlertTriangle, CreditCard } from 'lucide-react';

interface StripeConnectionProps {
  orgId: string;
}

export function StripeConnection(_props: StripeConnectionProps) {
  return (
    <div className="bg-gray-800 rounded-xl border border-emerald-500/20 p-4">
      <div className="flex items-center gap-2 mb-3">
        <div className="w-6 h-6 rounded bg-purple-600/20 flex items-center justify-center">
          <CreditCard size={12} className="text-purple-400" />
        </div>
        <h3 className="text-sm font-semibold text-white">Stripe — Live</h3>
        <span className="ml-auto flex items-center gap-1 text-[10px] text-emerald-400">
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
          Synced
        </span>
      </div>

      {/* Synced data summary */}
      <div className="grid grid-cols-3 gap-2 mb-3">
        <div className="bg-gray-900 rounded-lg p-2 text-center">
          <Users size={12} className="text-gray-400 mx-auto mb-1" />
          <p className="text-sm font-bold text-white">22</p>
          <p className="text-[10px] text-gray-500">Customers</p>
        </div>
        <div className="bg-gray-900 rounded-lg p-2 text-center">
          <TrendingUp size={12} className="text-emerald-400 mx-auto mb-1" />
          <p className="text-sm font-bold text-emerald-400">$22K</p>
          <p className="text-[10px] text-gray-500">MRR</p>
        </div>
        <div className="bg-gray-900 rounded-lg p-2 text-center">
          <AlertTriangle size={12} className="text-amber-400 mx-auto mb-1" />
          <p className="text-sm font-bold text-amber-400">1</p>
          <p className="text-[10px] text-gray-500">Failed</p>
        </div>
      </div>

      {/* Recent Stripe events */}
      <div className="space-y-1.5">
        <p className="text-[10px] text-gray-500 font-medium uppercase tracking-wider">Recent Events</p>
        <div className="flex items-center gap-2 text-xs">
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
          <span className="text-gray-400">invoice.paid</span>
          <span className="text-gray-500 ml-auto">$499 · Acme Corp</span>
        </div>
        <div className="flex items-center gap-2 text-xs">
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
          <span className="text-gray-400">payment_intent.succeeded</span>
          <span className="text-gray-500 ml-auto">$149 · TechStart</span>
        </div>
        <div className="flex items-center gap-2 text-xs">
          <span className="w-1.5 h-1.5 rounded-full bg-red-400" />
          <span className="text-gray-400">invoice.payment_failed</span>
          <span className="text-gray-500 ml-auto">$399 · DataFlow</span>
        </div>
      </div>

      <a
        href="https://dashboard.stripe.com/test/customers"
        target="_blank"
        rel="noopener noreferrer"
        className="flex items-center gap-1 text-[10px] text-purple-400 hover:text-purple-300 mt-3 pt-2 border-t border-gray-700"
      >
        <ExternalLink size={10} />
        Stripe Dashboard (test mode)
      </a>
    </div>
  );
}
