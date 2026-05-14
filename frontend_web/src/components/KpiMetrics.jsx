import React, { useEffect } from 'react';
import { DollarSign, Activity, AlertTriangle } from 'lucide-react';
import { motion as Motion, useMotionValue, useTransform, animate } from 'framer-motion';

const AnimatedNumber = ({ value, colorClass }) => {
  const count = useMotionValue(0);
  const rounded = useTransform(count, (latest) => 
    new Intl.NumberFormat('pt-BR', { 
      minimumFractionDigits: 2, 
      maximumFractionDigits: 2 
    }).format(latest)
  );

  useEffect(() => {
    const controls = animate(count, value, { duration: 1.5, ease: "easeOut" });
    return () => controls.stop();
  }, [value, count]);

  return (
    <span className={`kpi-value ${colorClass}`}>
      <Motion.span>{rounded}</Motion.span>
    </span>
  );
};

export default function KpiMetrics({ metrics }) {
  return (
    <div className="kpi-grid">
      <div className="glass-card kpi-item">
        <span className="kpi-label">
            <DollarSign size={16} color="var(--accent-emerald)"/> FATURAMENTO BRUTO
        </span>
        <div style={{ display: 'flex', alignItems: 'baseline', gap: '8px' }}>
          <span className="kpi-value" style={{color: 'var(--accent-emerald)'}}>R$ </span>
          <AnimatedNumber value={metrics.revenue} colorClass="" />
        </div>
        <p style={{margin: '12px 0 0 0', fontSize: '1rem', color: 'var(--text-primary)', lineHeight: '1.5'}}>
          <b>Entrada de Capital:</b> Soma de todos os fretes contratados e finalizados com sucesso no simulador.
        </p>
      </div>
      
      <div className="glass-card kpi-item">
        <span className="kpi-label">
            <AlertTriangle size={16} color="var(--accent-rose)"/> CUSTOS OPERACIONAIS
        </span>
        <div style={{ display: 'flex', alignItems: 'baseline', gap: '8px' }}>
          <span className="kpi-value" style={{color: 'var(--accent-rose)'}}>- R$ </span>
          <AnimatedNumber value={metrics.costs} colorClass="" />
        </div>
        <p style={{margin: '12px 0 0 0', fontSize: '1rem', color: 'var(--text-primary)', lineHeight: '1.5'}}>
          <b>Drenagem de Recursos:</b> Inclui o custo fixo por KM, taxas de pedágio automático e serviços de guincho em acidentes.
        </p>
      </div>
      
      <div className="glass-card kpi-item">
        <span className="kpi-label" style={{color: 'var(--accent-cyan)'}}>
            <Activity size={16} color="var(--accent-cyan)"/> MARGEM LÍQUIDA (NET)
        </span>
        <div style={{ display: 'flex', alignItems: 'baseline', gap: '8px' }}>
          <span className="kpi-value" style={{color: 'var(--accent-cyan)'}}>R$ </span>
          <AnimatedNumber value={metrics.profit} colorClass="" />
        </div>
        <p style={{margin: '12px 0 0 0', fontSize: '1rem', color: 'var(--text-primary)', lineHeight: '1.5'}}>
          <b>O que sobra no bolso:</b> É o lucro real após abater todas as despesas. Este valor determina seu poder de compra para expandir a frota.
        </p>
      </div>

      <div style={{ gridColumn: '1 / -1', padding: '15px 25px', background: 'rgba(255,255,255,0.03)', borderRadius: '12px', border: '1px solid var(--border-glass)' }}>
        <p style={{ margin: 0, fontSize: '0.95rem', color: 'var(--text-secondary)', textAlign: 'center', lineHeight: '1.6' }}>
          💡 <b>Cálculo de Performance:</b> A sua <b>Margem Líquida</b> é o resultado direto de <code>(Soma de Fretes) - (Desgaste + Pedágios + Multas)</code>. Mantenha os custos abaixo de 20% para uma operação saudável.
        </p>
      </div>
    </div>
  );
}
