export default function Leaderboard({ players, trips }) {
  // Aggregate profit by player
  const leaderboardData = players.map(player => {
    const playerTrips = trips.filter(t => t.playerId === player.id);
    const totalProfit = playerTrips.reduce((acc, curr) => acc + curr.netProfit, 0);
    const tripsDone = playerTrips.filter(t => t.status === "Finished").length;
    return { ...player, totalProfit, tripsDone };
  }).sort((a, b) => b.totalProfit - a.totalProfit);

  return (
    <div className="glass-card">
      <h2 style={{marginTop: 0, color: 'var(--text-primary)'}}>🏆 Ranking de Desempenho dos Jogadores</h2>
      <table>
        <thead>
          <tr>
            <th>Posição</th>
            <th>Motorista</th>
            <th>Entregas Concluídas</th>
            <th>Lucro Gerado</th>
          </tr>
        </thead>
        <tbody>
          {leaderboardData.map((player, index) => (
             <tr key={player.id}>
               <td style={{fontWeight: 'bold', color: index === 0 ? 'gold' : index === 1 ? 'silver' : index === 2 ? '#cd7f32' : 'inherit'}}>{index + 1}º</td>
               <td>
                 {player.name || `Jogador #${player.id}`} 
                 {player.accessKey && (
                   <span style={{color: 'var(--accent-magenta)', fontSize: '0.8rem', marginLeft: '8px', padding: '2px 6px', background: 'rgba(236, 72, 153, 0.1)', borderRadius: '4px'}}>
                     {player.accessKey}
                   </span>
                 )}
               </td>
               <td>{player.tripsDone}</td>
               <td className={player.totalProfit >= 0 ? "profit-positive" : "profit-negative"}>
                 R$ {player.totalProfit.toFixed(2)}
               </td>
             </tr>
          ))}
          {leaderboardData.length === 0 && (
             <tr><td colSpan="4" style={{textAlign: "center", color: 'var(--text-secondary)', padding: "2rem"}}>Nenhum motorista registrado no momento.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
