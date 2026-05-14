import React, { useState, useEffect } from 'react';

const TypewriterText = ({ text, delay = 20 }) => {
  const [currentText, setCurrentText] = useState('');
  const [currentIndex, setCurrentIndex] = useState(0);

  useEffect(() => {
    if (currentIndex < text.length) {
      const timeout = setTimeout(() => {
        setCurrentText(prevText => prevText + text[currentIndex]);
        setCurrentIndex(prevIndex => prevIndex + 1);
      }, delay);
  
      return () => clearTimeout(timeout);
    }
  }, [currentIndex, delay, text]);

  return <span className={currentIndex < text.length ? 'typewriter-cursor' : ''}>{currentText}</span>;
};

export default function RadioChatter({ trips }) {
  // Pega a ultima viagem que foi listada que teve historico de acidentes severos
  const recentTrip = trips.filter(t => t.status === "Finished" && t.incidentLogs).sort((a,b) => b.id - a.id)[0];
  
  if(!recentTrip || recentTrip.incidentLogs === "Nenhuma ocorrência. Viagem perfeita.") {
    return (
      <div className="glass-card chatter-box" style={{height: '220px', display: 'flex', flexDirection: 'column'}}>
        <h2 style={{marginTop: 0, color: 'var(--text-primary)'}}>📻 Central de Rádio da Frota</h2>
        <div style={{flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--accent-green)'}}>
          <p><TypewriterText text="Tudo tranquilo nas rodovias nacionais. Câmbio." /></p>
        </div>
      </div>
    );
  }

  const msgs = recentTrip.incidentLogs.split(' | ');

  return (
    <div className="glass-card chatter-box" style={{height: 'fit-content'}}>
        <h2 className="pulse-red" style={{marginTop: 0, color: 'var(--accent-red)'}}>📻 Alerta no Rádio (Viagem #{recentTrip.id})</h2>
        <div className="chat-history">
           {msgs.map((msg, i) => {
              const isAdmin = msg.includes("Cláudia") || msg.includes("Mecânico");
              return (
                 <div key={i} className={`chat-bubble ${isAdmin ? 'admin' : 'driver'}`}>
                    <TypewriterText text={msg} delay={15} />
                 </div>
              );
           })}
        </div>
    </div>
  );
}
