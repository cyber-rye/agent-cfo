import { useState } from 'react';
import { ChevronDown, ChevronUp, AlertTriangle, CheckCircle, XCircle, FileText, BarChart3 } from 'lucide-react';
import type { AgentDecision } from '../api/types';
import { TypewriterText } from './TypewriterText';

const typeConfig: Record<string, { color: string; icon: typeof CheckCircle; bg: string }> = {
  ExpenseApproved: { color: 'text-emerald-400', icon: CheckCircle, bg: 'bg-emerald-400/10' },
  ExpenseDenied: { color: 'text-red-400', icon: XCircle, bg: 'bg-red-400/10' },
  AnomalyDetected: { color: 'text-amber-400', icon: AlertTriangle, bg: 'bg-amber-400/10' },
  ReportGenerated: { color: 'text-blue-400', icon: FileText, bg: 'bg-blue-400/10' },
  ForecastUpdated: { color: 'text-purple-400', icon: BarChart3, bg: 'bg-purple-400/10' },
  AlertRaised: { color: 'text-amber-400', icon: AlertTriangle, bg: 'bg-amber-400/10' },
  BudgetAdjusted: { color: 'text-cyan-400', icon: BarChart3, bg: 'bg-cyan-400/10' },
};

function DecisionItem({ decision }: { decision: AgentDecision }) {
  const [expanded, setExpanded] = useState(false);
  const [typingDone, setTypingDone] = useState(false);
  const config = typeConfig[decision.type] || { color: 'text-gray-400', icon: FileText, bg: 'bg-gray-400/10' };
  const Icon = config.icon;

  return (
    <div className="border border-gray-700 rounded-lg p-4 hover:border-gray-600 transition-colors">
      <button
        onClick={() => setExpanded(!expanded)}
        className="w-full flex items-start gap-3 text-left"
      >
        <div className={`p-2 rounded-lg ${config.bg} shrink-0 mt-0.5`}>
          <Icon size={16} className={config.color} />
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span className={`text-xs font-medium px-2 py-0.5 rounded ${config.bg} ${config.color}`}>
              {decision.type}
            </span>
            <span className="text-gray-500 text-xs">
              {new Date(decision.createdAt).toLocaleString()}
            </span>
          </div>
          <p className="text-white text-sm mt-1 font-medium">{decision.description}</p>
        </div>
        <div className="text-gray-500 shrink-0 mt-1">
          {expanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
        </div>
      </button>
      {expanded && (
        <div className="mt-3 ml-11 text-sm text-gray-300 leading-relaxed bg-gray-900 rounded-lg p-3 border border-gray-700">
          {typingDone ? (
            <span className="whitespace-pre-wrap">{decision.reasoning}</span>
          ) : (
            <TypewriterText text={decision.reasoning} speed={15} onComplete={() => setTypingDone(true)} />
          )}
        </div>
      )}
    </div>
  );
}

export function AgentFeed({ decisions }: { decisions: AgentDecision[] }) {
  return (
    <div className="bg-gray-800 rounded-xl p-6 border border-gray-700">
      <h2 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
        <span className="w-2 h-2 bg-emerald-400 rounded-full animate-pulse" />
        Agent Activity
      </h2>
      <div className="space-y-3 max-h-[500px] overflow-y-auto pr-1">
        {decisions.length === 0 ? (
          <p className="text-gray-500 text-sm">No decisions yet. Run an analysis to see agent reasoning.</p>
        ) : (
          decisions.map(d => <DecisionItem key={d.id} decision={d} />)
        )}
      </div>
    </div>
  );
}
