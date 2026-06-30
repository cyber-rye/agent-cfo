import { useState } from 'react';
import { Shield, ChevronDown, ChevronUp, Lock, Network, Eye } from 'lucide-react';

const RULES = [
  { allowed: false, text: 'Cannot exceed hard budget limits', icon: '🚫' },
  { allowed: false, text: 'Cannot approve expenses > 20% of total budget', icon: '🚫' },
  { allowed: false, text: 'Cannot modify or delete the audit trail', icon: '🚫' },
  { allowed: false, text: 'All decisions require policy validation before execution', icon: '🔒' },
  { allowed: true, text: 'Can analyze transactions and detect anomalies', icon: '✅' },
  { allowed: true, text: 'Can generate forecasts and financial reports', icon: '✅' },
  { allowed: true, text: 'Can evaluate and approve/deny expense requests', icon: '✅' },
];

const SANDBOX_CONSTRAINTS = [
  { label: 'Network Policy', detail: 'Outbound restricted to Stripe API only', active: true },
  { label: 'Spending Cap', detail: 'Hard limits enforced by MediatR pipeline', active: true },
  { label: 'Audit Trail', detail: 'Every action logged with correlation ID', active: true },
  { label: 'Credential Isolation', detail: 'Stripe keys scoped to sandbox', active: true },
];

export function GovernancePanel() {
  const [expanded, setExpanded] = useState(true);

  return (
    <div className="bg-gray-800 rounded-xl border border-gray-700">
      <button
        onClick={() => setExpanded(prev => !prev)}
        className="w-full px-4 py-3 flex items-center justify-between text-left group"
      >
        <div className="flex items-center gap-2">
          <Shield size={14} className="text-violet-400" />
          <h2 className="text-sm font-semibold text-white">Governance</h2>
          <span className="text-[10px] text-emerald-400 border border-emerald-500/30 rounded px-1.5 py-0.5 flex items-center gap-1">
            <Lock size={8} />
            NemoClaw Sandbox
          </span>
        </div>
        <span className="text-gray-500 group-hover:text-gray-300 transition-colors">
          {expanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
        </span>
      </button>

      {expanded && (
        <div className="px-4 pb-4 space-y-4 border-t border-gray-700 pt-3">
          {/* Agent authority boundaries */}
          <div>
            <p className="text-xs text-gray-400 mb-2 font-medium">Agent Authority Boundaries</p>
            <div className="space-y-1.5">
              {RULES.map((rule, i) => (
                <div key={i} className="flex items-start gap-2">
                  <span className="text-xs shrink-0">{rule.icon}</span>
                  <span className="text-xs text-gray-300">{rule.text}</span>
                </div>
              ))}
            </div>
          </div>

          {/* NemoClaw Sandbox Status */}
          <div className="pt-3 border-t border-gray-700">
            <p className="text-xs text-gray-400 mb-2 font-medium flex items-center gap-1.5">
              <Network size={10} className="text-green-400" />
              NemoClaw Sandbox Enforcement
            </p>
            <div className="space-y-1.5">
              {SANDBOX_CONSTRAINTS.map((constraint, i) => (
                <div key={i} className="flex items-center gap-2">
                  <span className={`w-1.5 h-1.5 rounded-full ${constraint.active ? 'bg-emerald-400' : 'bg-gray-500'}`} />
                  <span className="text-xs text-gray-300 font-medium">{constraint.label}</span>
                  <span className="text-[10px] text-gray-500 ml-auto">{constraint.detail}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Footer */}
          <div className="pt-2 border-t border-gray-700">
            <p className="text-[10px] text-gray-500 flex items-center gap-1">
              <Eye size={8} />
              Enforced by MediatR policy pipeline + NemoClaw network policies.
              Agent operates in isolated sandbox with full audit trail.
            </p>
          </div>
        </div>
      )}
    </div>
  );
}
