# 🚚 Simulador de Transporte Logístico Tycoon

Bem-vindo ao **Simulador Logístico Full-Stack**, um ecossistema completo de desenvolvimento estruturado como um jogo competitivo de *Tycoon*. O projeto separa as responsabilidades entre uma **Engine Gráfica Inteligente**, um **Servidor Central de Regras** e um **Painel Tático Web**.

## 🏗️ Estrutura da Arquitetura (Mestre-Escravo)

Toda a infraestrutura foi projetada modularizada sob o paradigma API-First, descentralizando a computação em três esferas principais:

1. **Unity Engine 3D (C#) -> [Cliente Interativo]**  
   Um simulador imersivo misturando *Top-Down 3D* com um sistema denso de UI *Visual Novel*. Ele opera passivamente como um cliente visual para as simulações, consumindo endpoints para apresentar escolhas táticas ao usuário através da Central de Frotas de Júlia (Assistente NPC Embutida).
2. **Back-End ASP.NET API (C#) -> [Motor Financeiro]**  
   Servidor local que processa, calcula e valida todas as variáveis do mundo: Descontos de folha de pagamento (`/pay-bills`), compras de ativos (`/buy-upgrade`), avanços climáticos, relógios sistêmicos e persistência do Caixa Líquido no Banco de Dados.
3. **React Web Dashboard (JS) -> [Console do CEO]**  
   Interface Neumórfica administrativa isolada (Vite/React) responsável por cadastrar Instâncias de Empresas, definir as regras do jogo (Ex: *Temporadas de 30 a 120 dias*) e gerar as Chaves de Token Mestre necessárias para desbloquear e injetar comandos no Game-Client (Unity).

---

## ⚙️ Principais Funcionalidades

- **Ciclo de Turnos Tycoon:** O tempo flui dinamicamente com penalidades financeiras pesadas aplicadas rotineiramente ao fechar do mês comercial.
- **Ecossistema de Contratos Avançado:** Sistema orgânico em 2D de fechamento logístico operado em uma janela gerencial, com flutuações e cobranças extras atreladas a riscos.
- **Oficina e Banqueiro:** Empréstimos pesados de R$ 30.000, pneu Michelin para aumento global da receita, e GPS satélite.
- **Geração Aleatória de Crises:** Enchentes, acidentes e roubo de carga paralisam totalmente o 3D e sequestram a interface requisitando verba imediata do caixa unificado corporativo.
- **Conexão Direta Via Token:** É impossível logar no Front 3D sem antes criar uma infraestrutura remota pela interface React. O sistema Mestre-Escravo é perfeitamente selado.

---

## 🚀 Como Iniciar Seu Próprio Ecossistema

### 1. Iniciar Banco e Servidor (O Motor)
Abra a pasta `backend_api`. Se o PostgreSQL estiver rodando em background no seu Docker, basta comandar o EF e ligar a API nas portas locais:
```bash
dotnet run
```

### 2. Iniciar a Central do Diretor (Web)
Com a API rodando, inicialize sua sala de comando React na pasta `frontend_web`.
```bash
npm install
npm run dev
```
Acesse `http://localhost:5173/`, cadastre-se, crie a Temporada com limite customizável, e um **TOKEN EXCLUSIVO** será gerado em tela.

### 3. Iniciar o Complexo Imersivo (Unity 3D)
Abra o projeto dentro da pasta mãe no **Unity Hub**, dê o **Play** na cena principal. Digite o Token gerado pelo seu React na tela de credenciais, aperte Enter e a simulação de tráfego, junto ao banco de dados logístico, ganhará vida instantaneamente.

---

> Esse projeto é a demonstração prática de habilidades unificadas entre Design de Banco de Dados, Integração de APIs Web e Lógica Matemática Orgânica em Simuladores dentro da Unity.
