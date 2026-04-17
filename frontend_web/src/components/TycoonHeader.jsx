import React from 'react';
import { Building2, CalendarDays, Wallet, Truck } from 'lucide-react';

export default function TycoonHeader({ players }) {
  const company = players && players.length > 0 ? players[0] : null;
  if (!company) return null;

  return (
    <div className="glass-card" style={{ marginBottom: "2rem", borderLeft: "4px solid var(--accent-cyan)", padding: "2rem" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "20px" }}>
        
        <div style={{ display: 'flex', alignItems: 'center', gap: '15px' }}>
           <div style={{ background: 'rgba(6, 182, 212, 0.15)', padding: '15px', borderRadius: '14px', color: 'var(--accent-cyan)' }}>
              <Building2 size={36} />
           </div>
           <div>
               <h2 style={{ margin: "0 0 5px 0", color: 'var(--text-primary)', fontSize: "1.6rem", fontWeight: 800 }}>👑 LÍDER GLOBAL: {company.name}</h2>
               <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                   <span style={{ fontSize: "0.95rem", color: 'var(--text-secondary)' }}>Tycoon Season 1</span>
                   {company.hasBankLoan && <span style={{ background: 'rgba(239, 68, 68, 0.2)', padding: '2px 8px', borderRadius: '4px', color: '#fca5a5', fontSize: '0.8rem', fontWeight: 'bold' }}>🏦 Divida Bancária</span>}
                   {company.hasPremiumTires && <span style={{ background: 'rgba(16, 185, 129, 0.2)', padding: '2px 8px', borderRadius: '4px', color: '#6ee7b7', fontSize: '0.8rem', fontWeight: 'bold' }}>🛣️ Pneu Michelin</span>}
                   {company.hasAdvancedGPS && <span style={{ background: 'rgba(139, 92, 246, 0.2)', padding: '2px 8px', borderRadius: '4px', color: '#c4b5fd', fontSize: '0.8rem', fontWeight: 'bold' }}>🛰️ Satélite Geo</span>}
               </div>
           </div>
        </div>

        <div style={{ display: "flex", gap: "2.5rem", alignItems: "center" }}>
           <div style={{ textAlign: "right" }}>
              <p style={{ margin: "0 0 5px 0", fontSize: "0.85rem", color: "var(--text-secondary)", textTransform: "uppercase", fontWeight: 600, display: 'flex', alignItems: 'center', gap: '8px', justifyContent: 'flex-end' }}>
                 Relógio Econômico <CalendarDays size={16} />
              </p>
              <h3 style={{ margin: 0, color: "var(--text-primary)", fontSize: "1.6rem", fontWeight: 700 }}>
                Mês {(Math.floor(company.currentDay / 30) + 1)} <span style={{color: "var(--accent-cyan)"}}>· Dia {company.currentDay}</span>
              </h3>
           </div>
           
           <div style={{ textAlign: "right", paddingLeft: "2.5rem", borderLeft: "1px solid var(--border-glass)" }}>
              <p style={{ margin: "0 0 5px 0", fontSize: "0.85rem", color: "var(--text-secondary)", textTransform: "uppercase", fontWeight: 600, display: 'flex', alignItems: 'center', gap: '8px', justifyContent: 'flex-end' }}>
                 Caixa da Empresa (Lucro Global) <Wallet size={16} />
              </p>
              <h3 style={{ margin: 0, color: company.netWorth >= 0 ? "var(--accent-green)" : "var(--accent-red)", fontSize: "1.8rem", textShadow: company.netWorth >= 0 ? "0 0 15px rgba(16,185,129,0.3)" : "none" }}>
                R$ {company.netWorth.toFixed(2).replace('.',',')}
              </h3>
           </div>

           <div style={{ textAlign: "right", background: "linear-gradient(135deg, rgba(255,255,255,0.05), rgba(0,0,0,0.2))", padding: "15px 25px", borderRadius: "12px", border: "1px solid var(--border-glass)", boxShadow: "inset 0 2px 4px rgba(0,0,0,0.2)" }}>
              <p style={{ margin: "0 0 5px 0", fontSize: "0.85rem", color: "var(--text-secondary)", textTransform: "uppercase", fontWeight: 600, display: 'flex', alignItems: 'center', gap: '8px', justifyContent: 'flex-end' }}>
                 Ativos em Frota Livre <Truck size={16} />
              </p>
              <h3 style={{ margin: 0, color: company.fleetSize >= 7 ? "var(--accent-red)" : "var(--accent-gold)", fontSize: "1.6rem" }}>
                {company.fleetSize} <span style={{fontSize: "1rem", color: "var(--text-secondary)"}}>/ 7 Ativos</span>
              </h3>
           </div>
        </div>

      </div>
    </div>
  );
}
