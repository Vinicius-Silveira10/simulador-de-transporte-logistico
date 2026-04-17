export default function ContractsBoard() {
  return (
    <div className="glass-card" style={{marginTop: '1.5rem'}}>
      <h2 style={{marginTop: 0, color: 'var(--text-primary)'}}>👥 Portfólio de Contratantes VIPs da Transportadora</h2>
      <div className="contracts-grid">
         <div className="contract-npc food-npc">
            <h3>🍎 Roberto (Alimentos)</h3>
            <p className="npc-desc">Nosso fiel cliente de grãos. Risco Baixo. Suas caixas fornecem estabilidade mas as negociações são travadas e o lucro não possui elásticidade. Frete Fixo Básico: R$ 800.</p>
         </div>
         <div className="contract-npc parts-npc">
            <h3>⚙️ Tanaka (AutoPeças)</h3>
            <p className="npc-desc">Engenharia Pesada. Os fretes mecânicos são hiperdensos e vão sobrecarregar a estrutura elástica dos caminhões reduzindo drasticamente a velocidade do trânsito na Simulação. Risco Moderado.</p>
         </div>
         <div className="contract-npc med-npc">
            <h3>💊 Dra. Silvia (Medicamentos)</h3>
            <p className="npc-desc">Vacinas de alta perecibilidade. Os fretes da Silva são disparados na capacidade máxima mecânica exigindo pressa do simulador e aumentando chance de acidentes e bloqueios bruscos na pista. Apenas motoristas qualificados.</p>
         </div>
      </div>
    </div>
  );
}
