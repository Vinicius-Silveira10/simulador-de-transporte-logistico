export default function ContractsBoard() {
  return (
    <div className="glass-card" style={{marginTop: '1.5rem'}}>
      <h2 style={{marginTop: 0, color: 'var(--text-primary)'}}>👥 Portfólio de Contratantes VIPs da Transportadora</h2>
      <div className="contracts-grid">
         <div className="contract-npc food-npc">
            <h3>🍎 Roberto (Alimentos)</h3>
            <p className="npc-desc">Nosso fiel cliente de grãos. Risco Baixo. Suas caixas fornecem estabilidade mas as negociações são travadas e o lucro não possui elasticidade. Frete Fixo Básico: R$ 800.</p>
         </div>
         <div className="contract-npc parts-npc">
            <h3>⚙️ Tanaka (AutoPeças)</h3>
            <p className="npc-desc">Engenharia Pesada. Os fretes mecânicos são hiperdensos e vão sobrecarregar a estrutura elástica dos caminhões reduzindo drasticamente a velocidade do trânsito na Simulação. Risco Moderado.</p>
         </div>
         <div className="contract-npc med-npc">
            <h3>💊 Dra. Silvia (Medicamentos)</h3>
            <p className="npc-desc">Vacinas de alta perecibilidade. Os fretes da Silva são disparados na capacidade máxima mecânica exigindo pressa do simulador e aumentando chance de acidentes e bloqueios bruscos na pista. Apenas motoristas qualificados.</p>
         </div>
         <div className="contract-npc oil-npc">
            <h3>⛽ Carlos (Combustíveis)</h3>
            <p className="npc-desc">Ouro Negro Inflamável. O Carlos gerencia as refinarias da região. Transportar petróleo classe A exige seguro caro e paciência na curva. Qualquer erro pode gerar explosões catastróficas. Retorno Financeiro: Altíssimo.</p>
         </div>
         <div className="contract-npc cargo-npc">
            <h3>🚢 Marçal (Logística Portuária)</h3>
            <p className="npc-desc">Contêineres de exportação. O Marçal coordena a chegada dos navios chineses no porto. São cargas de volume extremo que testam o motor do caminhão até o limite. Paga por tonelagem extra e bônus de pontualidade.</p>
         </div>
      </div>
    </div>
  );
}
