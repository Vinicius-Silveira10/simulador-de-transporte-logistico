import { useEffect, useState } from 'react'
import KpiMetrics from './components/KpiMetrics'
import Leaderboard from './components/Leaderboard'
import BrazilMap from './components/BrazilMap'
import ContractsBoard from './components/ContractsBoard'
import RadioChatter from './components/RadioChatter'
import WebRegisterBox from './components/WebRegisterBox'
import TycoonHeader from './components/TycoonHeader'
import FinanceChart from './components/FinanceChart'

function App() {
  const [trips, setTrips] = useState([]);
  const [players, setPlayers] = useState([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const resTrips = await fetch('http://localhost:5041/api/Trips');
        if(resTrips.ok) setTrips(await resTrips.json());

        const resPlayers = await fetch('http://localhost:5041/api/Players');
        if(resPlayers.ok) setPlayers(await resPlayers.json());
      } catch(e) {
        console.error("Erro ao conectar com a sua API. O backend está rodando?", e);
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 3000); // Polling a cada 3 segundos
    return () => clearInterval(interval);
  }, []);

  const metrics = trips.reduce((acc, curr) => {
    if(curr.status === "Finished") {
      acc.revenue += curr.revenue;
      acc.costs += curr.kmCosts + curr.taxesAmount;
      acc.profit += curr.netProfit;
    }
    return acc;
  }, { revenue: 0, costs: 0, profit: 0 });

  return (
    <>
      <header>
        <h1>Painel Logístico Interativo</h1>
        <p>Centro de Controle Financeiro em Tempo Real — Analytics do Simulador Unity</p>
      </header>
      
      <main>
        <TycoonHeader players={players} />
        <KpiMetrics metrics={metrics} />
         
        <div className="dashboard-content">
          <div className="left-panel" style={{display: 'flex', flexDirection: 'column', gap: '1.5rem'}}>
             <FinanceChart trips={trips} />
             <Leaderboard players={players} trips={trips} />
          </div>
          
          <div className="right-panel" style={{display: 'flex', flexDirection: 'column', gap: '1.5rem'}}>
             <WebRegisterBox />
             <RadioChatter trips={trips} />
             <ContractsBoard />
             <BrazilMap trips={trips} />
          </div>
        </div>
      </main>
    </>
  )
}

export default App
