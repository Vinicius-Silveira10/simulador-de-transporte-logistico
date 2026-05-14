import { useEffect, useState } from 'react'
import { motion as Motion } from 'framer-motion'
import { LayoutDashboard, TrendingUp, Truck, Users, Radio, Map, Settings, Shield } from 'lucide-react'
import KpiMetrics from './components/KpiMetrics'
import Leaderboard from './components/Leaderboard'
import BrazilMap from './components/BrazilMap'
import ContractsBoard from './components/ContractsBoard'
import RadioChatter from './components/RadioChatter'
import WebRegisterBox from './components/WebRegisterBox'
import TycoonHeader from './components/TycoonHeader'
import FinanceChart from './components/FinanceChart'

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 }
  }
};

const itemVariants = {
  hidden: { y: 20, opacity: 0 },
  visible: {
    y: 0,
    opacity: 1,
    transition: { type: 'spring', stiffness: 300, damping: 24 }
  }
};

function App() {
  const [trips, setTrips] = useState([]);
  const [players, setPlayers] = useState([]);
  const [activeTab, setActiveTab] = useState('dashboard');

  useEffect(() => {
    const fetchData = async () => {
      try {
        const resTrips = await fetch('http://localhost:5041/api/Trips');
        if(resTrips.ok) setTrips(await resTrips.json());

        const resPlayers = await fetch('http://localhost:5041/api/Players');
        if(resPlayers.ok) setPlayers(await resPlayers.json());
      } catch(e) {
        console.error("Erro ao conectar com a sua API.", e);
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 3000); 
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
      {/* SIDEBAR ELITE */}
      <aside className="sidebar">
        <div className="sidebar-logo">
          <Shield size={28} color="var(--accent-cyan)" />
          SGE ELITE
        </div>
        
        <nav className="nav-group">
          <a href="#" className={`nav-item ${activeTab === 'dashboard' ? 'active' : ''}`} onClick={() => setActiveTab('dashboard')}>
            <LayoutDashboard size={20} /> Visão Geral
          </a>
          <a href="#" className={`nav-item ${activeTab === 'finance' ? 'active' : ''}`} onClick={() => setActiveTab('finance')}>
            <TrendingUp size={20} /> Financeiro
          </a>
          <a href="#" className={`nav-item ${activeTab === 'fleet' ? 'active' : ''}`} onClick={() => setActiveTab('fleet')}>
            <Truck size={20} /> Frota & Contratos
          </a>
          <a href="#" className={`nav-item ${activeTab === 'map' ? 'active' : ''}`} onClick={() => setActiveTab('map')}>
            <Map size={20} /> Monitoramento
          </a>
        </nav>

        <div className="nav-group" style={{marginTop: 'auto'}}>
          <div className="nav-item"> <Radio size={20} /> Rádio Frequência </div>
          <div className="nav-item"> <Settings size={20} /> Configurações </div>
        </div>
      </aside>

      {/* CONTEÚDO PRINCIPAL */}
      <main className="main-content">
        <Motion.div 
          initial="hidden"
          animate="visible"
          variants={containerVariants}
          key={activeTab} // Força animação ao trocar de tab
        >
          <header>
            <Motion.h1 variants={itemVariants}>
              {activeTab === 'dashboard' && "Painel Logístico Interativo"}
              {activeTab === 'finance' && "Análise Financeira Corporativa"}
              {activeTab === 'fleet' && "Gestão de Frota & Contratos"}
              {activeTab === 'map' && "Monitoramento Global de Ativos"}
            </Motion.h1>
            <Motion.p variants={itemVariants}>Centro de Controle Estratégico — Analytics em Tempo Real</Motion.p>
          </header>
          
          <div style={{display: 'flex', flexDirection: 'column', gap: '2rem'}}>
            
            {activeTab === 'dashboard' && (
              <>
                {/* SINCRONIZADOR CENTRALIZADO NO TOPO */}
                <Motion.div variants={itemVariants}>
                  <WebRegisterBox />
                </Motion.div>

                <Motion.div variants={itemVariants}>
                  <TycoonHeader players={players} />
                </Motion.div>

                <Motion.div variants={itemVariants}>
                  <KpiMetrics metrics={metrics} />
                </Motion.div>

                <div className="dashboard-content">
                  <div className="left-panel" style={{display: 'flex', flexDirection: 'column', gap: '2rem'}}>
                     <Motion.div variants={itemVariants}>
                       <FinanceChart trips={trips} />
                     </Motion.div>
                  </div>
                  <div className="right-panel" style={{display: 'flex', flexDirection: 'column', gap: '2rem'}}>
                     <Motion.div variants={itemVariants}>
                       <Leaderboard players={players} trips={trips} />
                     </Motion.div>
                  </div>
                </div>
              </>
            )}

            {activeTab === 'finance' && (
              <>
                <Motion.div variants={itemVariants}>
                  <KpiMetrics metrics={metrics} />
                </Motion.div>
                <Motion.div variants={itemVariants}>
                  <FinanceChart trips={trips} />
                </Motion.div>
                <Motion.div variants={itemVariants}>
                  <Leaderboard players={players} trips={trips} />
                </Motion.div>
              </>
            )}

            {activeTab === 'fleet' && (
              <>
                <Motion.div variants={itemVariants}>
                  <ContractsBoard />
                </Motion.div>
                <div className="dashboard-content">
                   <div className="left-panel">
                      <Motion.div variants={itemVariants}>
                        <RadioChatter trips={trips} />
                      </Motion.div>
                   </div>
                   <div className="right-panel">
                      {/* Removido daqui para não duplicar */}
                   </div>
                </div>
              </>
            )}

            {activeTab === 'map' && (
              <Motion.div variants={itemVariants} style={{height: '70vh'}}>
                <BrazilMap trips={trips} />
              </Motion.div>
            )}

          </div>
        </Motion.div>
      </main>
    </>
  )
}

export default App
