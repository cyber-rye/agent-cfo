import { useState, useEffect } from 'react';
import { Shield, User, Bot, Globe, Server } from 'lucide-react';
import { api } from '../api/client';
import type { AuditEntry } from '../api/types';

const actorConfig: Record<string, { icon: typeof Bot; color: string }> = {
  Agent: { icon: Bot, color: 'text-violet-400' },
  Human: { icon: User, color: 'text-blue-400' },
  Webhook: { icon: Globe, color: 'text-emerald-400' },
  System: { icon: Server, color: 'text-gray-400' },
};

function timeAgo(dateStr: string): string {
  const seconds = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
  if (seconds < 60) return 'just now';
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
  return `${Math.floor(seconds / 86400)}d ago`;
}

export function AuditTrail({ orgId }: { orgId: string }) {
  const [entries, setEntries] = useState<AuditEntry[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      try {
        const data = await api.getAuditTrail(orgId, 10);
        if (!cancelled) setEntries(data);
      } catch {
        // silently fail — audit trail is supplementary
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [orgId]);

  return (
    <div className="bg-gray-800 rounded-xl border border-gray-700">
      <div className="px-4 py-3 flex items-center gap-2">
        <Shield size={14} className="text-gray-400" />
        <h2 className="text-sm font-semibold text-white">Audit Trail</h2>
        {entries.length > 0 && (
          <span className="text-xs text-gray-500 ml-auto">{entries.length}</span>
        )}
      </div>

      <div className="px-4 pb-3 space-y-1">
        {loading ? (
          <p className="text-xs text-gray-500 py-2">Loading...</p>
        ) : entries.length === 0 ? (
          <p className="text-xs text-gray-500 py-2">No events yet.</p>
        ) : (
          entries.map(entry => {
            const config = actorConfig[entry.actor] || actorConfig.System;
            const Icon = config.icon;
            return (
              <div key={entry.id} className="flex items-start gap-2 py-1.5 group">
                <div className="mt-0.5 shrink-0">
                  <Icon size={12} className={config.color} />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-xs text-gray-300 truncate">
                    <span className="text-gray-400">{entry.action}</span>
                    {entry.entityType && (
                      <span className="text-gray-500"> · {entry.entityType}</span>
                    )}
                  </p>
                </div>
                <span className="text-[10px] text-gray-600 shrink-0 group-hover:text-gray-400 transition-colors">
                  {timeAgo(entry.createdAt)}
                </span>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}
