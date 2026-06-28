import { useState, useEffect, useCallback } from 'react';
import { AlertTriangle, CheckCircle, XCircle, FileText, BarChart3, X } from 'lucide-react';

export interface ToastMessage {
  id: string;
  type: string;
  title: string;
  description: string;
}

const typeConfig: Record<string, { color: string; icon: typeof CheckCircle; border: string }> = {
  ExpenseApproved: { color: 'text-emerald-400', icon: CheckCircle, border: 'border-emerald-500/30' },
  ExpenseDenied: { color: 'text-red-400', icon: XCircle, border: 'border-red-500/30' },
  AnomalyDetected: { color: 'text-amber-400', icon: AlertTriangle, border: 'border-amber-500/30' },
  ReportGenerated: { color: 'text-blue-400', icon: FileText, border: 'border-blue-500/30' },
  ForecastUpdated: { color: 'text-purple-400', icon: BarChart3, border: 'border-purple-500/30' },
  AlertRaised: { color: 'text-amber-400', icon: AlertTriangle, border: 'border-amber-500/30' },
};

function Toast({ toast, onDismiss }: { toast: ToastMessage; onDismiss: (id: string) => void }) {
  const config = typeConfig[toast.type] || { color: 'text-gray-400', icon: FileText, border: 'border-gray-500/30' };
  const Icon = config.icon;

  useEffect(() => {
    const timer = setTimeout(() => onDismiss(toast.id), 6000);
    return () => clearTimeout(timer);
  }, [toast.id, onDismiss]);

  return (
    <div
      className={`flex items-start gap-3 p-4 bg-gray-800 rounded-lg border ${config.border} shadow-lg animate-slide-in`}
      style={{ animation: 'slideIn 0.3s ease-out' }}
    >
      <Icon size={18} className={`${config.color} shrink-0 mt-0.5`} />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-white">{toast.title}</p>
        <p className="text-xs text-gray-400 mt-0.5 truncate">{toast.description}</p>
      </div>
      <button onClick={() => onDismiss(toast.id)} className="text-gray-500 hover:text-gray-300 shrink-0">
        <X size={14} />
      </button>
    </div>
  );
}

export function ToastContainer({ toasts, onDismiss }: { toasts: ToastMessage[]; onDismiss: (id: string) => void }) {
  if (toasts.length === 0) return null;

  return (
    <div className="fixed top-20 right-4 z-50 flex flex-col gap-2 w-80">
      {toasts.map(t => (
        <Toast key={t.id} toast={t} onDismiss={onDismiss} />
      ))}
    </div>
  );
}

export function useToasts() {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const addToast = useCallback((type: string, title: string, description: string) => {
    const id = `${Date.now()}-${Math.random().toString(36).slice(2)}`;
    setToasts(prev => [...prev, { id, type, title, description }]);
  }, []);

  const dismissToast = useCallback((id: string) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  }, []);

  return { toasts, addToast, dismissToast };
}
