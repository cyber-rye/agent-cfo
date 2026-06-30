import { useState } from 'react';
import { DollarSign, CheckCircle, XCircle, Loader2, Save } from 'lucide-react';
import { api } from '../api/client';
import type { ExpenseEvaluation, ExpenseRecordResult } from '../api/types';
import { TypewriterText } from './TypewriterText';

const CATEGORIES = [
  { value: 0, label: 'Subscription' },
  { value: 1, label: 'One-Time' },
  { value: 2, label: 'Infrastructure' },
  { value: 3, label: 'Marketing' },
  { value: 4, label: 'Tools' },
  { value: 5, label: 'Contractors' },
  { value: 6, label: 'Office' },
  { value: 7, label: 'Legal' },
  { value: 8, label: 'Other' },
];

type EvalState =
  | { step: 'form' }
  | { step: 'evaluating' }
  | { step: 'result'; evaluation: ExpenseEvaluation }
  | { step: 'recording'; evaluation: ExpenseEvaluation }
  | { step: 'recorded'; evaluation: ExpenseEvaluation; recordResult: ExpenseRecordResult };

export function ExpenseEvaluator({ orgId, onComplete }: { orgId: string; onComplete?: () => void }) {
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState(4); // default: Tools
  const [state, setState] = useState<EvalState>({ step: 'form' });
  const [typingDone, setTypingDone] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleEvaluate = async () => {
    const parsedAmount = parseFloat(amount);
    if (!parsedAmount || parsedAmount <= 0 || !description.trim()) return;

    setState({ step: 'evaluating' });
    setError(null);
    setTypingDone(false);

    try {
      const evaluation = await api.evaluateExpense(orgId, parsedAmount, description.trim());
      setState({ step: 'result', evaluation });
      onComplete?.();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Evaluation failed');
      setState({ step: 'form' });
    }
  };

  const handleRecord = async () => {
    if (state.step !== 'result') return;
    const parsedAmount = parseFloat(amount);

    setState({ step: 'recording', evaluation: state.evaluation });

    try {
      const recordResult = await api.recordExpense(orgId, parsedAmount, description.trim(), category);
      setState({ step: 'recorded', evaluation: state.evaluation, recordResult });
      onComplete?.();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Recording failed');
      setState({ step: 'result', evaluation: state.evaluation });
    }
  };

  const handleReset = () => {
    setAmount('');
    setDescription('');
    setCategory(4);
    setState({ step: 'form' });
    setTypingDone(false);
    setError(null);
  };

  const isApproved = state.step !== 'form' && state.step !== 'evaluating'
    ? state.evaluation.type === 'ExpenseApproved'
    : false;
  const isRecording = state.step === 'recording';
  const showRecordAction = isApproved && (state.step === 'result' || isRecording) && typingDone;

  return (
    <div className="border border-gray-700 rounded-lg bg-gray-800/50">
      {/* Header */}
      <div className="px-4 py-3 border-b border-gray-700 flex items-center gap-2">
        <DollarSign size={16} className="text-violet-400" />
        <span className="text-sm font-semibold text-white">Ask the Agent</span>
        {state.step !== 'form' && state.step !== 'evaluating' && (
          <button
            onClick={handleReset}
            className="ml-auto text-xs text-gray-400 hover:text-white transition-colors"
          >
            New request
          </button>
        )}
      </div>

      <div className="p-4 space-y-3">
        {/* Form inputs */}
        <div className="flex gap-2">
          <div className="relative w-28 shrink-0">
            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm">$</span>
            <input
              type="number"
              value={amount}
              onChange={e => setAmount(e.target.value)}
              placeholder="0"
              min="0"
              step="100"
              disabled={state.step !== 'form'}
              className="w-full pl-7 pr-3 py-2 bg-gray-900 border border-gray-600 rounded-lg text-white text-sm
                         placeholder:text-gray-600 focus:outline-none focus:border-violet-500 disabled:opacity-50"
            />
          </div>
          <input
            type="text"
            value={description}
            onChange={e => setDescription(e.target.value)}
            placeholder="What do you want to buy?"
            disabled={state.step !== 'form'}
            onKeyDown={e => e.key === 'Enter' && handleEvaluate()}
            className="flex-1 px-3 py-2 bg-gray-900 border border-gray-600 rounded-lg text-white text-sm
                       placeholder:text-gray-600 focus:outline-none focus:border-violet-500 disabled:opacity-50"
          />
          <button
            onClick={handleEvaluate}
            disabled={state.step !== 'form' || !amount || !description.trim()}
            className="px-4 py-2 bg-violet-600 hover:bg-violet-700 disabled:opacity-40 disabled:cursor-not-allowed
                       rounded-lg text-sm font-medium text-white transition-colors flex items-center gap-1.5 shrink-0"
          >
            {state.step === 'evaluating' ? (
              <Loader2 size={14} className="animate-spin" />
            ) : (
              '⚡'
            )}
            {state.step === 'evaluating' ? 'Thinking...' : 'Evaluate'}
          </button>
        </div>

        {error && (
          <p className="text-xs text-red-400 bg-red-400/10 rounded px-3 py-2">{error}</p>
        )}

        {/* Agent response */}
        {state.step !== 'form' && state.step !== 'evaluating' && (
          <div className={`rounded-lg p-3 border ${
            isApproved
              ? 'bg-emerald-400/5 border-emerald-500/20'
              : 'bg-red-400/5 border-red-500/20'
          }`}>
            {/* Decision header */}
            <div className="flex items-center gap-2 mb-2">
              {isApproved ? (
                <CheckCircle size={16} className="text-emerald-400" />
              ) : (
                <XCircle size={16} className="text-red-400" />
              )}
              <span className={`text-sm font-semibold ${isApproved ? 'text-emerald-400' : 'text-red-400'}`}>
                {state.evaluation.type === 'ExpenseApproved' ? 'Approved' : 'Denied'}
              </span>
              <span className="text-xs text-gray-500">{state.evaluation.description}</span>
            </div>

            {/* Reasoning with typewriter */}
            <div className="text-sm text-gray-300 leading-relaxed">
              {(state.step === 'result' || state.step === 'recorded') && !typingDone ? (
                <TypewriterText
                  text={state.evaluation.reasoning}
                  speed={15}
                  onComplete={() => setTypingDone(true)}
                />
              ) : (
                <span className="whitespace-pre-wrap">{state.evaluation.reasoning}</span>
              )}
            </div>

            {/* Record expense action (only on approval) */}
            {showRecordAction && (
              <div className="mt-3 pt-3 border-t border-gray-700 flex items-center gap-2">
                <select
                  value={category}
                  onChange={e => setCategory(Number(e.target.value))}
                  disabled={isRecording}
                  className="px-2 py-1.5 bg-gray-900 border border-gray-600 rounded text-xs text-gray-300
                             focus:outline-none focus:border-violet-500 disabled:opacity-50"
                >
                  {CATEGORIES.map(c => (
                    <option key={c.value} value={c.value}>{c.label}</option>
                  ))}
                </select>
                <button
                  onClick={handleRecord}
                  disabled={isRecording}
                  className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700
                             disabled:opacity-50 rounded text-xs font-medium text-white transition-colors"
                >
                  {isRecording ? (
                    <Loader2 size={12} className="animate-spin" />
                  ) : (
                    <Save size={12} />
                  )}
                  {isRecording ? 'Recording...' : 'Record Expense'}
                </button>
              </div>
            )}

            {/* Recorded success */}
            {state.step === 'recorded' && (
              <div className="mt-3 pt-3 border-t border-gray-700">
                <p className="text-xs text-emerald-400 flex items-center gap-1.5">
                  <CheckCircle size={12} />
                  Expense recorded — transaction {state.recordResult.transactionId.slice(0, 8)}...
                  budget updated.
                </p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
