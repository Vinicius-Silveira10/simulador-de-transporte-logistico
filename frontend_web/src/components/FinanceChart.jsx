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

  const finishedTrips = trips.filter(t => t.status === "Finished");
  
  const chartData = finishedTrips.reduce((acc, t, index) => {
      const prevProfit = acc.length > 0 ? acc[acc.length - 1].lucroLiq : 0;
      const prevCosts = acc.length > 0 ? acc[acc.length - 1].custoOpe : 0;
      
      acc.push({
          nome: `Frete ${index + 1}`,
          lucroLiq: parseFloat((prevProfit + t.netProfit).toFixed(2)),
          custoOpe: parseFloat((prevCosts + t.kmCosts + t.taxesAmount).toFixed(2)),
          incidente: t.incidentLogs !== "Nenhuma ocorrência. Viagem perfeita."
      });
      return acc;
  }, []);

  const cumulativeProfit = chartData.length > 0 ? chartData[chartData.length - 1].lucroLiq : 0;

  // Calculate quick metrics for the chart header
  const currentProfit = cumulativeProfit;
  const lastProfit = chartData.length > 1 ? chartData[chartData.length - 2].lucroLiq : 0;
  const growth = currentProfit - lastProfit;

  return (
    <div className="glass-card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
            <div>
                <h3 style={{ margin: 0, color: 'var(--text-primary)', display: 'flex', alignItems: 'center', gap: '10px', fontSize: '1.2rem' }}>
                    <TrendingUp color={"var(--accent-cyan)"} size={22} /> FLUXO DE CAPITAL CORPORATIVO
                </h3>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', margin: '5px 0 0 0' }}>Análise de desempenho acumulado: Receita Líquida vs. Custos Operacionais.</p>
            </div>
            
            <div style={{ textAlign: 'right' }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '1px' }}>Variação Último Frete</span>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px', color: growth >= 0 ? 'var(--accent-emerald)' : 'var(--accent-rose)', fontWeight: '800', fontSize: '1.1rem' }}>
                    {growth >= 0 ? <TrendingUp size={20} /> : <TrendingDown size={20} />}
                    <span>{growth >= 0 ? '+' : ''} R$ {growth.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}</span>
                </div>
            </div>
        </div>
        
        <div style={{ width: '100%', height: 350 }}>
            <ResponsiveContainer>
                <AreaChart data={chartData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <defs>
                        <linearGradient id="colorLucro" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="var(--accent-emerald)" stopOpacity={0.4}/>
                            <stop offset="95%" stopColor="var(--accent-emerald)" stopOpacity={0}/>
                        </linearGradient>
                        <linearGradient id="colorCusto" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="var(--accent-rose)" stopOpacity={0.4}/>
                            <stop offset="95%" stopColor="var(--accent-rose)" stopOpacity={0}/>
                        </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="4 4" stroke="rgba(255,255,255,0.03)" vertical={false} />
                    <XAxis dataKey="nome" stroke="var(--text-secondary)" fontSize={11} tickLine={false} axisLine={false} tick={{dy: 10}} />
                    <YAxis stroke="var(--text-secondary)" fontSize={11} tickFormatter={(val) => `R$${val/1000}k`} tickLine={false} axisLine={false} tick={{dx: -5}} />
                    
                    <Tooltip 
                        contentStyle={{ backgroundColor: 'rgba(2, 6, 23, 0.9)', border: '1px solid var(--border-glass)', borderRadius: '16px', backdropFilter: 'blur(12px)', boxShadow: '0 20px 40px rgba(0,0,0,0.6)' }}
                        itemStyle={{ padding: '4px 0' }}
                        cursor={{ stroke: 'var(--accent-cyan)', strokeWidth: 1 }}
                    />
                    
                    <Legend iconType="rect" wrapperStyle={{ paddingTop: '30px', fontSize: '12px', fontWeight: '500' }} />
                    <Area type="monotone" name="Patrimônio Líquido" dataKey="lucroLiq" stroke="var(--accent-emerald)" strokeWidth={4} fillOpacity={1} fill="url(#colorLucro)" animationDuration={2000} />
                    <Area type="monotone" name="Custos de Operação" dataKey="custoOpe" stroke="var(--accent-rose)" strokeWidth={4} fillOpacity={1} fill="url(#colorCusto)" animationDuration={2500} />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    </div>
  );
}
