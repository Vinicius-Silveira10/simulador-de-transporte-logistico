import React from 'react';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { TrendingUp, TrendingDown, DollarSign } from 'lucide-react';

export default function FinanceChart({ trips }) {
  if(!trips || trips.length === 0) {
      return (
        <div className="glass-card" style={{ height: '400px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', color: 'var(--text-secondary)' }}>
            <DollarSign size={48} opacity={0.3} style={{ marginBottom: '1rem' }} />
            <p>Aguardando a primeira entrega da Matriz para traçar o fluxo financeiro...</p>
        </div>
      );
  }

  let cumulativeProfit = 0;
  let cumulativeCosts = 0;
  const chartData = trips.filter(t => t.status === "Finished").map((t, index) => {
      cumulativeProfit += t.netProfit;
      cumulativeCosts += (t.kmCosts + t.taxesAmount);
      return {
          nome: `Frete ${index + 1}`,
          lucroLiq: parseFloat(cumulativeProfit.toFixed(2)),
          custoOpe: parseFloat(cumulativeCosts.toFixed(2)),
          incidente: t.incidentLogs !== "Nenhuma ocorrência. Viagem perfeita." ? true : false
      };
  });

  // Calculate quick metrics for the chart header
  const currentProfit = cumulativeProfit;
  const lastProfit = chartData.length > 1 ? chartData[chartData.length - 2].lucroLiq : 0;
  const growth = currentProfit - lastProfit;

  return (
    <div className="glass-card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
            <div>
                <h3 style={{ margin: 0, color: 'var(--text-primary)', display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <TrendingUp color={"var(--accent-cyan)"} /> Gráfico de Capital da Empresa
                </h3>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', margin: '5px 0 0 0' }}>Evolução acumulativa de Receita contra Despesas da Frota.</p>
            </div>
            
            <div style={{ textAlign: 'right' }}>
                <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Momento Financeiro</span>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', color: growth >= 0 ? 'var(--accent-green)' : 'var(--accent-red)', fontWeight: 'bold' }}>
                    {growth >= 0 ? <TrendingUp size={16} /> : <TrendingDown size={16} />}
                    <span>{growth >= 0 ? '+' : ''} R$ {growth.toFixed(2).replace('.',',')} no último frete</span>
                </div>
            </div>
        </div>
        
        <div style={{ width: '100%', height: 320 }}>
            <ResponsiveContainer>
                <AreaChart data={chartData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <defs>
                        <linearGradient id="colorLucro" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="var(--accent-green)" stopOpacity={0.6}/>
                            <stop offset="95%" stopColor="var(--accent-green)" stopOpacity={0}/>
                        </linearGradient>
                        <linearGradient id="colorCusto" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="var(--accent-red)" stopOpacity={0.6}/>
                            <stop offset="95%" stopColor="var(--accent-red)" stopOpacity={0}/>
                        </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" vertical={false} />
                    <XAxis dataKey="nome" stroke="var(--text-secondary)" fontSize={11} tickLine={false} axisLine={false} />
                    <YAxis stroke="var(--text-secondary)" fontSize={11} tickFormatter={(val) => `R$${val}`} tickLine={false} axisLine={false} />
                    
                    <Tooltip 
                        contentStyle={{ backgroundColor: 'rgba(15, 23, 42, 0.95)', border: '1px solid rgba(6, 182, 212, 0.3)', borderRadius: '12px', backdropFilter: 'blur(8px)', boxShadow: '0 10px 25px rgba(0,0,0,0.5)' }}
                        itemStyle={{ color: 'var(--text-primary)', fontWeight: 'bold' }}
                        labelStyle={{ color: 'var(--accent-cyan)', marginBottom: '5px' }}
                    />
                    
                    <Legend iconType="circle" wrapperStyle={{ paddingTop: '20px' }} />
                    <Area type="monotone" name="Lucro Acumulado (Net Worth)" dataKey="lucroLiq" stroke="var(--accent-green)" strokeWidth={3} fillOpacity={1} fill="url(#colorLucro)" />
                    <Area type="monotone" name="Despesas Totais (Operação)" dataKey="custoOpe" stroke="var(--accent-red)" strokeWidth={3} fillOpacity={1} fill="url(#colorCusto)" />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    </div>
  );
}
