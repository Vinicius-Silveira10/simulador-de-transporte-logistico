# Plano de Ação: Encerramento de Dia Instantâneo

Este plano visa garantir que o jogador tenha total controle sobre o fluxo do tempo, permitindo o encerramento do dia independentemente do estado da simulação física do caminhão.

## Propostas de Mudança

### 🚚 Logística de "Força Bruta" (Reset de Cena)
- **Mudança**: Ao clicar em "Fechar Caixa", o jogo irá forçar o retorno do caminhão físico para a doca instantaneamente.
- **Justificativa**: Em um jogo de gestão, o ciclo administrativo (dia/turno) tem precedência sobre a animação de tráfego. Se o CEO decidiu encerrar o expediente, a logística de campo é "concluída administrativamente" para que o novo dia comece limpo.

### 🔘 HUD Transparente e Funcional
- **Mudança**: Os botões de controle de tempo não serão mais removidos da tela.
- **Comportamento**: Eles estarão sempre disponíveis no canto inferior direito. O aviso "Frota em Trânsito" será apenas informativo e posicionado de forma a não obstruir os botões.

---

## Detalhes por Componente

### [MODIFY] [TruckController.cs](file:///c:/Users/Vinicius/Projeto_Estagio_01_02/LogisticsSimulator/Assets/Scripts/TruckController.cs)
- Adicionar o método `public void ForceResetToGarage()`:
    - Define `isMoving = false`.
    - Reseta `transform.position` para `docaInicial`.
    - Reseta a rotação.

### [MODIFY] [DialogueSystem.cs](file:///c:/Users/Vinicius/Projeto_Estagio_01_02/LogisticsSimulator/Assets/Scripts/DialogueSystem.cs)
- Cachear as referências (`TruckController` e `TycoonTimeManager`) no `Start`.
- Reformular o bloco `if (dialogState == 0)` para manter os botões de tempo visíveis.
- No clique de "Fechar Caixa", chamar `truck.ForceResetToGarage()` antes de disparar o encerramento do dia.

---

## Plano de Verificação

1. Iniciar uma entrega longa.
2. Com o caminhão ainda no meio do caminho, clicar em **"🌙 Fechar Caixa"**.
3. O caminhão deve "teleportar" para a garagem e a tela de relatório diário deve abrir imediatamente.
4. O novo dia deve começar com o caminhão na garagem pronto para o próximo contrato.
