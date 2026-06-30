import { useState, useEffect, useRef } from 'react';
import { ChevronDown, ChevronUp, AlertTriangle, CheckCircle, XCircle, FileText, BarChart3, Loader2, Brain } from 'lucide-react';
import type { AgentDecision } from '../api/types';

const typeConfig: Record<string, { color: string; icon: typeof CheckCircle; bg: string }> = {
  ExpenseApproved: { color: 'text-emerald-400', icon: CheckCircle, bg: 'bg-emerald-400/10' },
  ExpenseDenied: { color: 'text-red-400', icon: XCircle, bg: 'bg-red-400/10' },
  AnomalyDetected: { color: 'text-amber-400', icon: AlertTriangle, bg: 'bg-amber-400/10' },
  ReportGenerated: { color: 'text-blue-400', icon: FileText, bg: 'bg-blue-400/10' },
  ForecastUpdated: { color: 'text-purple-400', icon: BarChart3, bg: 'bg-purple-400/10' },
  AlertRaised: { color: 'text-amber-400', icon: AlertTriangle, bg: 'bg-amber-400/10' },
  BudgetAdjusted: { color: 'text-cyan-400', icon: BarChart3, bg: 'bg-cyan-400/10' },
};

interface DecisionItemProps {
  decision: AgentDecision;
  expanded: boolean;
  isNew: boolean;
}

function DecisionItem({ decision, expanded, isNew }: DecisionItemProps) {
  const [userToggled, setUserToggled] = useState<boolean | null>(null);
  const config = typeConfig[decision.type] || { color: 'text-gray-400', icon: FileText, bg: 'bg-gray-400/10' };
  const Icon = config.icon;

  const isOpen = userToggled !== null ? userToggled : expanded;

  return (
    <div className={`border border-gray-700 rounded-lg p-4 hover:border-gray-600 transition-all duration-500 ${isNew ? 'opacity-0 animate-[fadeIn_0.6s_ease-out_forwards]' : ''}`}>
      <button
        onClick={() => setUserToggled(prev => prev !== null ? !prev : !expanded)}
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
          {isOpen ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
        </div>
      </button>
      {isOpen && (
        <div className="mt-3 ml-11 text-sm text-gray-300 leading-relaxed bg-gray-900 rounded-lg p-3 border border-gray-700">
          <span className="whitespace-pre-wrap">{decision.reasoning}</span>
        </div>
      )}
    </div>
  );
}

interface AnalysisLoadingProps {
  step: string | null;
}

function AnalysisLoading({ step }: AnalysisLoadingProps) {
  const [dots, setDots] = useState('');

  useEffect(() => {
    const interval = setInterval(() => {
      setDots(prev => prev.length >= 3 ? '' : prev + '.');
    }, 400);
    return () => clearInterval(interval);
  }, []);

  const stepMessages: Record<string, string> = {
    'Detecting anomalies...': 'Scanning expense patterns and budget utilization',
    'Generating forecast...': 'Projecting cash flow and runway scenarios',
    'Writing summary...': 'Compiling financial analysis and recommendations',
  };

  return (
    <div className="border border-violet-500/30 rounded-lg p-4 bg-violet-500/5 animate-pulse">
      <div className="flex items-start gap-3">
        <div className="p-2 rounded-lg bg-violet-500/10 shrink-0">
          <Brain size={16} className="text-violet-400 animate-pulse" />
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span className="text-xs font-medium px-2 py-0.5 rounded bg-violet-500/10 text-violet-400">
              Agent Thinking
            </span>
            <Loader2 size={12} className="text-violet-400 animate-spin" />
          </div>
          <p className="text-white text-sm mt-1 font-medium">
            {step || 'Analyzing financial data'}{dots}
          </p>
          {step && stepMessages[step] && (
            <p className="text-xs text-gray-400 mt-1">{stepMessages[step]}</p>
          )}
        </div>
      </div>
    </div>
  );
}

interface AgentFeedProps {
  decisions: AgentDecision[];
  analyzing?: boolean;
  analysisStep?: string | null;
  newDecisionIds?: Set<string>;
}

export function AgentFeed({ decisions, analyzing, analysisStep, newDecisionIds }: AgentFeedProps) {
  const initialLoadRef = useRef(true);
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());

  useEffect(() => {
    if (initialLoadRef.current && decisions.length > 0) {
      setExpandedIds(new Set(decisions.map(d => d.id)));
      initialLoadRef.current = false;
    }
    if (newDecisionIds && newDecisionIds.size > 0) {
      setExpandedIds(prev => {
        const next = new Set(prev);
        for (const id of newDecisionIds) next.add(id);
        return next;
      });
    }
  }, [decisions, newDecisionIds]);

  return (
    <div className="bg-gray-800 rounded-xl p-6 border border-gray-700">
      <h2 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
        <span className={`w-2 h-2 rounded-full ${analyzing ? 'bg-violet-400 animate-pulse' : 'bg-emerald-400 animate-pulse'}`} />
        Agent Activity
        {analyzing && (
          <span className="text-xs text-violet-400 font-normal ml-2">Processing...</span>
        )}
      </h2>
      <div className="space-y-3 max-h-[700px] overflow-y-auto pr-1">
        {analyzing && <AnalysisLoading step={analysisStep} />}

        {decisions.length === 0 && !analyzing ? (
          <p className="text-gray-500 text-sm">No decisions yet. Run an analysis to see agent reasoning.</p>
        ) : (
          decisions.map(d => (
            <DecisionItem
              key={d.id}
              decision={d}
              expanded={expandedIds.has(d.id)}
              isNew={newDecisionIds?.has(d.id) ?? false}
            />
          ))
        )}
      </div>
    </div>
  );
}
