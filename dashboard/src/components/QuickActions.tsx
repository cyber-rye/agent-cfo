import { useState } from 'react';
import { Search, BarChart3, FileText, Loader2 } from 'lucide-react';
import { api } from '../api/client';

interface QuickActionsProps {
  orgId: string;
  onComplete: () => void; // refresh parent data
}

const ACTIONS = [
  {
    key: 'anomalies',
    label: 'Detect Anomalies',
    icon: Search,
    color: 'text-amber-400',
    bg: 'hover:bg-amber-400/10 border-amber-500/20',
    call: (orgId: string) => api.detectAnomalies(orgId),
  },
  {
    key: 'forecast',
    label: 'Generate Forecast',
    icon: BarChart3,
    color: 'text-purple-400',
    bg: 'hover:bg-purple-400/10 border-purple-500/20',
    call: (orgId: string) => api.generateForecast(orgId),
  },
  {
    key: 'summary',
    label: 'Generate Report',
    icon: FileText,
    color: 'text-blue-400',
    bg: 'hover:bg-blue-400/10 border-blue-500/20',
    call: (orgId: string) => api.generateSummary(orgId),
  },
];

export function QuickActions({ orgId, onComplete }: QuickActionsProps) {
  const [running, setRunning] = useState<string | null>(null);

  const handleAction = async (key: string, call: (orgId: string) => Promise<unknown>) => {
    setRunning(key);
    try {
      await call(orgId);
      onComplete(); // refresh decisions + data
    } catch (err) {
      console.error(`Action ${key} failed:`, err);
    } finally {
      setRunning(null);
    }
  };

  return (
    <div className="flex flex-wrap gap-2">
      {ACTIONS.map(action => {
        const Icon = action.icon;
        const isLoading = running === action.key;
        return (
          <button
            key={action.key}
            onClick={() => handleAction(action.key, action.call)}
            disabled={running !== null}
            className={`flex items-center gap-1.5 px-3 py-1.5 bg-gray-800 border rounded-lg text-xs font-medium
                        text-gray-300 transition-colors disabled:opacity-40 disabled:cursor-not-allowed ${action.bg}`}
          >
            {isLoading ? (
              <Loader2 size={12} className="animate-spin" />
            ) : (
              <Icon size={12} className={action.color} />
            )}
            {isLoading ? 'Running...' : action.label}
          </button>
        );
      })}
    </div>
  );
}
