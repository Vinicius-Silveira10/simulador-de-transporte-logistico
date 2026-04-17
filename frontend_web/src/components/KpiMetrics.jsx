import React from 'react';
import { DollarSign, Activity, AlertTriangle } from 'lucide-react';

export default function KpiMetrics({ metrics }) {
  return (
    <div className="kpi-grid">
      <div className="glass-card kpi-item">
        <span className="kpi-label" style={{display: 'flex', alignItems: 'center', gap: '8px'}}>
            <DollarSign size={18} color="var(--accent-green)"/> Faturamento Bruto (Contratos)
        </span>
        <span className="kpi-value green">R$ {metrics.revenue.toFixed(2).replace('.',',')}</span>
      </div>
      
      <div className="glass-card kpi-item">
        <span className="kpi-label" style={{display: 'flex', alignItems: 'center', gap: '8px'}}>
            <AlertTriangle size={18} color="var(--accent-red)"/> Despesas Visíveis (GPS + Pedágio + Guincho)
        </span>
        <span className="kpi-value red">- R$ {metrics.costs.toFixed(2).replace('.',',')}</span>
      </div>
      
      <div className="glass-card kpi-item" style={{ borderColor: 'rgba(6, 182, 212, 0.4)' }}>
        <span className="kpi-label" style={{display: 'flex', alignItems: 'center', gap: '8px', color: 'var(--accent-cyan)'}}>
            <Activity size={18} color="var(--accent-cyan)"/> Margem de Lucro Exata (Net)
        </span>
        <span className="kpi-value cyan">R$ {metrics.profit.toFixed(2).replace('.',',')}</span>
      </div>
    </div>
  );
}
