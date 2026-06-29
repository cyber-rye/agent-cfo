import { useState } from 'react';
import { Shield, ChevronDown, ChevronUp, XCircle, CheckCircle } from 'lucide-react';

const RULES = [
  { allowed: false, text: 'Cannot exceed hard budget limits' },
  { allowed: false, text: 'Cannot approve expenses > 20% of total budget' },
  { allowed: false, text: 'Cannot modify or delete the audit trail' },
  { allowed: false, text: 'All decisions require policy validation before execution' },
  { allowed: true, text: 'Can analyze transactions and detect anomalies' },
  { allowed: true, text: 'Can generate forecasts and financial reports' },
  { allowed: true, text: 'Can evaluate and approve/deny expense requests' },
];

export function GovernancePanel() {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="bg-gray-800 rounded-xl border border-gray-700">
      <button
        onClick={() => setExpanded(prev => !prev)}
        className="w-full px-4 py-3 flex items-center justify-between text-left group"
      >
        <div className="flex items-center gap-2">
          <Shield size={14} className="text-violet-400" />
          <h2 className="text-sm font-semibold text-white">Governance</h2>
        </div>
        <span className="text-gray-500 group-hover:text-gray-300 transition-colors">
          {expanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
        </span>
      </button>

      {expanded && (
        <div className="px-4 pb-3 space-y-1.5 border-t border-gray-700 pt-3">
          <p className="text-xs text-gray-400 mb-2">Agent authority boundaries:</p>
          {RULES.map((rule, i) => (
            <div key={i} className="flex items-start gap-2">
              {rule.allowed ? (
                <CheckCircle size={12} className="text-emerald-400 mt-0.5 shrink-0" />
              ) : (
                <XCircle size={12} className="text-red-400 mt-0.5 shrink-0" />
              )}
              <span className="text-xs text-gray-300">{rule.text}</span>
            </div>
          ))}
          <p className="text-xs text-gray-500 mt-2 pt-2 border-t border-gray-700">
            Enforced by MediatR policy pipeline + NemoClaw sandbox network policies.
          </p>
        </div>
      )}
    </div>
  );
}
