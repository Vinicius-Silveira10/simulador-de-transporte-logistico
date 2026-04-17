import React, { useState } from 'react';
import { Key } from 'lucide-react';

export default function WebRegisterBox() {
  const [name, setName] = useState('');
  const [maxDays, setMaxDays] = useState(120);
  const [generatedKey, setGeneratedKey] = useState(null);
  const [error, setError] = useState(null);

  const handleRegister = async () => {
    try {
        const res = await fetch("http://localhost:5041/api/Company/register/web", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name, maxDays: parseInt(maxDays) })
        });
        if (res.ok) {
            const data = await res.json();
            setGeneratedKey(data.accessKey);
            setError(null);
        } else {
            setError("Erro: Nome já existe ou erro no servidor");
        }
    } catch(e) {
        setError("Servidor C# Desligado.");
    }
  };

  return (
    <div className="glass-card" style={{ borderTop: "4px solid var(--accent-magenta)", padding: "1.5rem" }}>
      <h3 style={{ margin: "0 0 15px 0", display: "flex", gap: "10px", alignItems: "center" }}>
        <Key size={24} color="var(--accent-magenta)" /> 
        Sincronizador Web (Motorista-Cliente)
      </h3>
      <p style={{ fontSize: "0.9rem", color: "var(--text-secondary)", marginBottom: "15px" }}>
        Crie a Empresa no Banco do Site e envie a Token Key para o Unity.
      </p>
      
      {!generatedKey ? (
        <div style={{ display: 'flex', gap: '10px' }}>
          <input 
            type="text" 
            placeholder="Nome da Sessão/Empresa..." 
            value={name}
            onChange={e => setName(e.target.value)}
            style={{ flex: 1, padding: "10px", borderRadius: "8px", border: "1px solid var(--border-glass)", background: "rgba(0,0,0,0.3)", color: "white" }}
          />
          <select 
            value={maxDays} 
            onChange={e => setMaxDays(e.target.value)}
            style={{ padding: "10px", borderRadius: "8px", border: "1px solid var(--border-glass)", background: "rgba(0,0,0,0.5)", color: "white", cursor: "pointer" }}
          >
             <option value="30">Temporada Rápida (30 Dias)</option>
             <option value="60">Temporada Média (60 Dias)</option>
             <option value="90">Temporada Longa (90 Dias)</option>
             <option value="120">Campeonato Tycoon (120 Dias)</option>
          </select>
          <button onClick={handleRegister} style={{ background: "var(--accent-magenta)", color: "white", padding: "10px 15px", border: "none", borderRadius: "8px", fontWeight: "bold", cursor: "pointer" }}>Gerar Chave Jogo</button>
        </div>
      ) : (
        <div style={{ background: "rgba(236, 72, 153, 0.15)", padding: "15px", borderRadius: "10px", textAlign: "center", border: "1px dashed var(--accent-magenta)" }}>
           <span style={{ fontSize: "0.9rem", color: "var(--text-secondary)" }}>INJETE ESTE TOKEN NA TELA DA UNITY AGORA:</span>
           <h2 style={{ fontSize: "2.5rem", margin: "10px 0", color: "var(--accent-magenta)", letterSpacing: "5px" }}>{generatedKey}</h2>
           <button onClick={() => { setGeneratedKey(null); setName(''); }} style={{ background: "transparent", color: "white", border: "1px solid rgba(255,255,255,0.2)", padding: "5px 15px", borderRadius: "5px", cursor: "pointer", fontSize: "0.8rem" }}>Criar Nova Matriz</button>
        </div>
      )}
      {error && <p style={{ color: "var(--accent-red)", fontSize: "0.8rem", marginTop: "10px" }}>{error}</p>}
    </div>
  );
}
