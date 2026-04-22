using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    // === SISTEMA MULTIPLAYER BATTLE ===
    public static int GlobalPlayerId = 0;
    public static string GlobalPlayerName = "";
    public static int GlobalCurrentDay = 1;
    public static int GlobalMaxDays = 120; // Variavel Governamental Dinâmica
    public static bool EndGameLocked = false;
    
    // === EXPANSÃO UPGRADES ===
    public static bool GlobalHasPremiumTires = false;
    public static bool GlobalHasAdvancedGPS = false;

    // === SISTEMA DE HUD E EVENTOS ===
    public static float GlobalNetWorth = 0f;
    public static float GlobalLoanDebt = 0f;
    public static int GlobalFleet = 1;
    
    private string eventTitle = "";
    private string eventMessage = "";
    private float eventCost = 0f;
    private Texture2D loadedEventImage = null; // Cache da Arte 2D Gerada

    // === AVATARES NPC ===
    private Texture2D npcJulia = null;
    private Texture2D npcRoberto = null;
    private Texture2D npcTanaka = null;
    private Texture2D npcSilvia = null;
    private Texture2D npcCarlos = null;
    private Texture2D npcMarcal = null;

    private int dialogState = -1; 
    public int GetDialogState() { return dialogState; } // Expõe a variável para o Motor de Tempo pausar!
    private string loginInputKey = ""; // AGORA É KEY E NÃO NAME

    private string currentNPC = "";
    private float finalRevenue = 0f;
    private bool isSubmitting = false;

    // Cache de Referências (Performance & Gestão de Turno)
    private TruckController cachedTruck;
    private TycoonTimeManager cachedTime;

    private int companyFleet = 1;
    private float demandMultiplier = 1.0f;
    private Rect windowRect;
    private string dialogTitle = "PABX Corporativo";
    private string dialogText = "Carregando...";

    // --- SISTEMA TOAST E FLUXO DIÁRIO ---
    public struct Toast { public string text; public float time; }
    private System.Collections.Generic.List<Toast> activeToasts = new System.Collections.Generic.List<Toast>();
    private float startOfDayNetWorth = 0f; // Variavel Sombra que clona o seu Caixa para calculo do Lucro Diario
    private int contractsAcceptedToday = 0; // Nova Regra de Frota: Contador de Despachos

    public void ShowToast(string message) {
        activeToasts.Add(new Toast { text = message, time = 4.0f });
    }

    void Update() {
        for (int i = activeToasts.Count - 1; i >= 0; i--) {
            Toast t = activeToasts[i];
            t.time -= Time.deltaTime;
            if (t.time <= 0) activeToasts.RemoveAt(i);
            else activeToasts[i] = t;
        }
    }

    // State Vars do Banco
    private bool stateHasLoan = false;

    void Start() {
        if (GlobalPlayerId == 0) dialogState = -1;
        
        cachedTruck = FindFirstObjectByType<TruckController>();
        cachedTime = FindFirstObjectByType<TycoonTimeManager>();

        // Carrega Avatares Silenciosamente para VRAM
        npcJulia = Resources.Load<Texture2D>("NPC/npc_julia");
        npcRoberto = Resources.Load<Texture2D>("NPC/npc_roberto");
        npcTanaka = Resources.Load<Texture2D>("NPC/npc_tanaka");
        npcSilvia = Resources.Load<Texture2D>("NPC/npc_silvia");
        npcCarlos = Resources.Load<Texture2D>("NPC/npc_carlos");
        npcMarcal = Resources.Load<Texture2D>("NPC/npc_marcal");

        // Puxa e inicia o Satélite do Clima (Ideia 1) silenciosamente pra economizar clique do Estagiário
        if (FindObjectOfType<DynamicWeather>() == null) {
            new GameObject("Controlador_Climatico").AddComponent<DynamicWeather>();
        }
    }

    void OnGUI()
    {
        windowRect = new Rect(20, Screen.height - 240, Screen.width - 40, 220); // Janela maior
        GUI.color = Color.white;
        GUIStyle textStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, richText = true };

        // >>> DESENHA A HUD FINANCEIRA (SEMPRE VISÍVEL APÓS LOGIN) <<<
        if (GlobalPlayerId != 0 && dialogState != -1) {
            GUI.backgroundColor = new Color(0.1f, 0.4f, 0.2f, 0.95f);
            GUI.Box(new Rect(0, 0, Screen.width, 40), "");
            
            // Relógio em Turnos Baseado em Horas
            int h = Mathf.FloorToInt(TycoonTimeManager.CurrentHour);
            int m = 0; // Turnos exatos não perdem tempo com minutos
            string digitalClock = $"{h:D2}:{m:D2}";
            
            GUI.Label(new Rect(20, 10, Screen.width, 30), $"🗓️ Dia: <b>{GlobalCurrentDay}/{GlobalMaxDays}</b>   |   ⏰ Hora: <b>{digitalClock}</b>   |   💰 Caixa Lìquido: <b>R$ {GlobalNetWorth:F2}</b>   |   🚚 Caminhões: <b>{GlobalFleet}</b>   |   🏦 Dívida Banco: <b>R$ {GlobalLoanDebt:F2}</b>", textStyle);
        }

        // >>> JANELA DE EVENTO INVASIVO DO NPC (Cobre a tela inteira) Y = 10 (Eventos Aleatorios), Y = 11 (Fechamento Relatorio Diario) <<<
        if (dialogState == 10 || dialogState == 11) {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.98f); // Dossiê Sombrio e Elegante
            Rect evtRect = new Rect(Screen.width/2 - 350, Screen.height/2 - 250, 700, 520); // GIGANTE!
            GUI.Window(99, evtRect, dialogState == 10 ? DrawEventScreen : DrawDailyReportScreen, "⚠️ " + eventTitle);
            return; // Bloqueia outros cliques do jogador
        }

        if (dialogState == -1) // LOGIN
        {
            GUI.backgroundColor = new Color(0.1f, 0.3f, 0.5f, 0.9f);
            GUI.Window(0, new Rect(20, Screen.height - 180, Screen.width - 40, 160), DrawLoginScreen, "🏆 TYCOON E-SPORTS: Registrar no Torneio");
            return;
        }

        if (EndGameLocked) // FIM
        {
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
            GUI.Window(0, new Rect(20, Screen.height - 180, Screen.width - 40, 160), (id) => {
                GUI.Label(new Rect(20, 30, windowRect.width - 40, 60), $"<b>FIM DA TEMPORADA (Meta de {GlobalMaxDays} Dias Atingida)!</b> \nSua jornada competitiva encerrou. Vá ao painel React para ver como você ficou no Ranking Mundial!", textStyle);
            }, "🏆 TEMPORADA FINALIZADA");
            return;
        }

        // >>> BOTÃO FLUTUANTE DE REABRIR O PABX <<<
        if (dialogState == 0) {
            // HUD Informativa de Status da Frota
            bool isTruckWorking = cachedTruck != null && cachedTruck.isMoving;

            if (isTruckWorking) {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(Screen.width - 250, Screen.height - 110, 240, 30), "🚚 Frota Ativa em Trânsito...");
                GUI.color = Color.white;
            }

            // Controle Tycoon de Turnos Livres (Sempre Visíveis!)
            GUI.backgroundColor = new Color(0.6f, 0.4f, 0.1f, 0.9f);
            if (GUI.Button(new Rect(Screen.width - 270, Screen.height - 70, 120, 50), "⏩ Pular 1 Hora")) {
                cachedTime?.AdvanceHours(1);
            }

            GUI.backgroundColor = new Color(0.3f, 0.2f, 0.5f, 0.9f);
            if (GUI.Button(new Rect(Screen.width - 140, Screen.height - 70, 120, 50), "🌙 Fechar Caixa")) {
                // Nova Regra de Gestão: O Dia encerra AGORA, independente da posição do caminhão.
                if (cachedTruck != null) cachedTruck.ForceResetToGarage();
                cachedTime?.EndDayEarly();
            }

            GUI.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);
            if (GUI.Button(new Rect(20, Screen.height - 70, 250, 50), "📞 Chamar a Júlia (PABX)")) {
                StartCoroutine(FetchCompanyStateForJulia());
            }
            return; // Se a tela principal estiver limpa pra visualizar o 3D, não desenha janelas falsas!
        }

        if (dialogState == 5) return; 
        
        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        
        // SE FOR O MENU CENTRAL DE NEGOCIAÇÃO, A JANELA CRESCE PRO CENTRO E ENGULHE O 3D!
        if (dialogState == 2 && currentNPC != "Vendedor") {
            windowRect = new Rect(20, Screen.height/2 - 200, Screen.width - 40, 400); 
        }

        windowRect = GUI.Window(0, windowRect, DrawDialogWindow, dialogTitle);

        // >>> RENDERIZA TOASTS <<<
        int toastY = Screen.height - 100;
        for (int i = activeToasts.Count - 1; i >= 0; i--) {
            Toast t = activeToasts[i];
            float alpha = t.time > 1f ? 1f : t.time;
            GUI.color = new Color(1f, 0.8f, 0.4f, alpha); // Laranja Dourado Tycoon
            GUIStyle toastStyle = new GUIStyle(GUI.skin.box) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            GUI.Box(new Rect(Screen.width - 340, toastY, 320, 40), t.text, toastStyle);
            toastY -= 45; // Empilha
        }
        GUI.color = Color.white;
    }

    void DrawLoginScreen(int id) {
        GUIStyle tStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
        GUI.Label(new Rect(20, 30, windowRect.width - 40, 40), "Terminal Mestre-Escravo: Injete o Token Gerado no Painel Web:", tStyle);
        loginInputKey = GUI.TextField(new Rect(20, 70, 300, 30), loginInputKey, 10).ToUpper(); // Token ex: X-090
        
        if (!isSubmitting && GUI.Button(new Rect(340, 70, 200, 30), "Sincronizar Unity!")) {
            isSubmitting = true;
            StartCoroutine(LoginPlayerAPI());
        }
    }

    IEnumerator LoginPlayerAPI() {
        LoginPayload payload = new LoginPayload { Key = loginInputKey };
        UnityWebRequest req = new UnityWebRequest(GameConfig.LOGIN_URL, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success) {
            LoginResponse res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
            GlobalPlayerId = res.id;
            GlobalPlayerName = res.name;
            GlobalCurrentDay = res.currentDay;
            GlobalMaxDays = res.maxDays;
            isSubmitting = false;
            
            if (GlobalCurrentDay >= GlobalMaxDays) EndGameLocked = true;
            else {
                // A UI fará a calibração de Relógio com o Shadow Account
                startOfDayNetWorth = GlobalNetWorth; 
                dialogState = 0; 
                StartCoroutine(FetchCompanyStateForJulia()); 
            }
        } else {
            Debug.LogError("Servidor offline");
            isSubmitting = false;
        }
    }

    void DrawEventScreen(int id) {
        GUIStyle tStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, richText = true };
        
        // Renderiza o Visual Novel se a Imagem For Encontrada no HD
        if (loadedEventImage != null) {
            GUI.DrawTexture(new Rect(20, 30, 660, 340), loadedEventImage, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(20, 380, 660, 80), eventMessage, tStyle);
        } else {
            GUI.Label(new Rect(20, 30, 660, 200), eventMessage + "\n\n<i>[Sys Warning: Imagem Ausente em Assets/Resources/Events/...]</i>", tStyle);
        }
        
        string btnText = eventCost > 0 ? $"Autorizar Débito de R$ {eventCost:F2}" : "Arquivar Relatório (Isento)";
        if (!isSubmitting && GUI.Button(new Rect(250, 460, 200, 40), btnText)) {
            isSubmitting = true;
            dialogState = 0; // Libera o Jogo
            if (eventCost > 0) StartCoroutine(ChargeEventAPI(eventCost));
            else { isSubmitting = false; TriggerHUDRefresh(); }
        }
    }

    void DrawDailyReportScreen(int id) {
        GUIStyle tStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, richText = true };
        
        if (loadedEventImage != null) GUI.DrawTexture(new Rect(20, 30, 200, 200), loadedEventImage, ScaleMode.ScaleToFit);
        
        GUI.Label(new Rect(240, 30, 420, 200), eventMessage, tStyle);
        GUI.Label(new Rect(20, 240, 660, 40), "<i>O relógio das 00h00 está travado. Os motoristas aguardam a liberação de seu acesso Mestre C-Level.</i>", tStyle);
        
        if (!isSubmitting && GUI.Button(new Rect(250, 400, 200, 60), "[Assinar] Avançar Dia")) {
            startOfDayNetWorth = GlobalNetWorth; // Zero a balança novamente pro dia que começa!
            dialogState = 0; // Libera o Jogo! Começa Tudo De Novo!
        }
    }

    void DrawDialogWindow(int windowID)
    {
        GUIStyle textStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, richText = true };

        if (dialogState == 1) // MENU MASTER (Agora com Upgrades e Banco)
        {
            dialogTitle = $"☎️ Central: {GlobalPlayerName} | Dia Limite: {GlobalCurrentDay}/120";
            GUI.Label(new Rect(20, 30, windowRect.width - 40, 40), "Selecione o ramal para despachos Tycoon (Ramais Novos Adicionados!):", textStyle);

            GUI.backgroundColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);
            if (GUI.Button(new Rect(20, 80, 200, 40), "💼 Falar com Júlia")) {
                dialogState = 2;
                int month = (GlobalCurrentDay / 30) + 1;
                dialogText = $"<b>[Júlia (Gerente)]</b>: Olá Chefe! Mês {month}. Nossa frota é de {companyFleet} cavalo(s).\nMultiplicador R$ na Praça: <b>x{demandMultiplier:F2}</b>.\nQual VIP conectou?";
            }
            
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.1f, 0.9f);
            if (GUI.Button(new Rect(230, 80, 230, 40), "💰 Juca (Caminhão R$12K)")) {
                currentNPC = "Vendedor";
                dialogText = "<b>[Juca Vendas]</b>: Caminhão zero no pátio, R$ 12.000 à vista. Leva?";
                dialogState = 2;
            }

            GUI.backgroundColor = new Color(0.1f, 0.7f, 0.3f, 0.9f);
            if (GUI.Button(new Rect(470, 80, 180, 40), "🏦 Banco (Sr. Carvalho)")) {
                dialogState = 6;
                dialogText = stateHasLoan ? "<b>[Sr. Carvalho]</b>: Prezado, você JÁ PEGOU SEU EMPRÉSTIMO. Pague suas parcelas do dia 30!" : "<b>[Sr. Carvalho (Banco)]</b>: Olá! Liberei um Crédito de R$ 30.000 (Trinta Mil) na sua conta hoje. \nAVISO: Descontaremos R$ 5.000 de Parcelas Direto do Seu Caixa Todo Dia 30! Assinar?";
            }

            GUI.backgroundColor = new Color(0.6f, 0.2f, 0.8f, 0.9f);
            if (GUI.Button(new Rect(660, 80, 180, 40), "🔧 Oficina Mecânica")) {
                dialogState = 7;
                dialogText = "<b>[Mecânico]</b>: Quer blindar seus caminhões contra imprevistos na rodovia e desvios de GPS pra não perder dinheiro na viagem?";
            }

            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f); 
        }
        else if (dialogState == 2 && currentNPC != "Vendedor") // JÚLIA E CONTATOS V.N.
        {
            dialogTitle = "Central de Operações Tycoon";
            
            // FOTO VIP DA JULIA NO HEADER 
            if (npcJulia != null) GUI.DrawTexture(new Rect(20, 30, 80, 80), npcJulia, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(110, 30, windowRect.width - 150, 80), dialogText, textStyle);

            GUI.Label(new Rect(20, 115, windowRect.width - 40, 30), $"<b>Catálogo de Clientes Disponíveis:</b> (Frota Disponível: {GlobalFleet - contractsAcceptedToday}/{GlobalFleet})", textStyle);

            if (contractsAcceptedToday >= GlobalFleet) {
                GUI.backgroundColor = Color.red;
                GUI.Box(new Rect(20, 145, windowRect.width - 40, 235), "");
                GUI.Label(new Rect(40, 200, windowRect.width - 80, 100), "<size=24><b>⚠️ FROTA 100% OCUPADA</b></size>\n\nVocê já despachou todos os caminhões disponíveis para hoje. \n<b>Dica:</b> Encerre o dia no botão '🌙 Fechar Caixa' para renovar sua frota amanhã!", new GUIStyle(textStyle) { alignment = TextAnchor.MiddleCenter });
                GUI.backgroundColor = Color.white;
            } else {
                // MURAL 1: ROBERTO ALIMENTOS
            GUI.Box(new Rect(20, 145, 170, 235), "");
            if (npcRoberto != null) GUI.DrawTexture(new Rect(30, 155, 150, 125), npcRoberto, ScaleMode.ScaleToFit);
            else GUI.Label(new Rect(30, 155, 150, 125), "[Sem Avatar]", textStyle);
            GUI.Label(new Rect(30, 290, 150, 40), "📦 Roberto (Alimentos)", textStyle);
            if (GUI.Button(new Rect(30, 330, 150, 40), $"R$ {800 * demandMultiplier:F0}")) { currentNPC = "Alimentos"; finalRevenue = 800f * demandMultiplier; StartDialogTree(); }

            // MURAL 2: ENG. TANAKA PEÇAS
            GUI.Box(new Rect(200, 145, 170, 235), "");
            if (npcTanaka != null) GUI.DrawTexture(new Rect(210, 155, 150, 125), npcTanaka, ScaleMode.ScaleToFit);
            else GUI.Label(new Rect(210, 155, 150, 125), "[Sem Avatar]", textStyle);
            GUI.Label(new Rect(210, 290, 150, 40), "⚙️ Tanaka (Peças)", textStyle);
            if (GUI.Button(new Rect(210, 330, 150, 40), $"R$ {1500 * demandMultiplier:F0}")) { currentNPC = "Peças"; finalRevenue = 1500f * demandMultiplier; StartDialogTree(); }

            // MURAL 3: DRA SILVIA LUXURY
            GUI.Box(new Rect(380, 145, 170, 235), "");
            if (npcSilvia != null) GUI.DrawTexture(new Rect(390, 155, 150, 125), npcSilvia, ScaleMode.ScaleToFit);
            else GUI.Label(new Rect(390, 155, 150, 125), "[Sem Avatar]", textStyle);
            GUI.Label(new Rect(390, 290, 150, 40), "💊 Dra. Silvia (Vacinas)", textStyle);
            if (GUI.Button(new Rect(390, 330, 150, 40), $"R$ {3500 * demandMultiplier:F0}")) { currentNPC = "Medicamentos"; finalRevenue = 3500f * demandMultiplier; StartDialogTree(); }

            // MURAL 4: CARLOS PETROLEO
            GUI.Box(new Rect(560, 145, 170, 235), "");
            if (npcCarlos != null) GUI.DrawTexture(new Rect(570, 155, 150, 125), npcCarlos, ScaleMode.ScaleToFit);
            else GUI.Label(new Rect(570, 155, 150, 125), "[Sem Avatar]", textStyle);
            GUI.Label(new Rect(570, 290, 150, 40), "🛢️ Carlos (Petróleo)", textStyle);
            if (GUI.Button(new Rect(570, 330, 150, 40), $"R$ {5000 * demandMultiplier:F0}")) { currentNPC = "Petróleo"; finalRevenue = 5000f * demandMultiplier; StartDialogTree(); }

            // MURAL 5: MARÇAL CONTÊINER
            GUI.Box(new Rect(740, 145, 170, 235), "");
            if (npcMarcal != null) GUI.DrawTexture(new Rect(750, 155, 150, 125), npcMarcal, ScaleMode.ScaleToFit);
            else GUI.Label(new Rect(750, 155, 150, 125), "[Sem Avatar]", textStyle);
            GUI.Label(new Rect(750, 290, 150, 40), "🚢 Marçal (Portuário)", textStyle);
            if (GUI.Button(new Rect(750, 330, 150, 40), $"R$ {2800 * demandMultiplier:F0}")) { currentNPC = "Contêiner"; finalRevenue = 2800f * demandMultiplier; StartDialogTree(); }
            }

            // BOTAO FINAL DE DESLIGAR ISOLADO NA DIREITA
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
            if (GUI.Button(new Rect(windowRect.width - 150, 30, 120, 40), "[Desligar]")) { ResetDialog(); }
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f); // Volta o fundo pra padrão
        }
        else if (dialogState == 2 && currentNPC == "Vendedor") // JUCA
        {
            dialogTitle = "Mecânica Pesada do Juca Automóveis";
            GUI.Label(new Rect(20, 30, windowRect.width - 40, 60), dialogText, textStyle);
            if (!isSubmitting && GUI.Button(new Rect(20, 100, 300, 40), "[Comprar Truck] Fechado.")) { isSubmitting = true; dialogState = 5; StartCoroutine(BuyTruckAPI()); }
            if (GUI.Button(new Rect(340, 100, 300, 40), "[Recusar] Achei salgado.")) { ResetDialog(); }
        }
        else if (dialogState >= 3 && dialogState <= 5) // CONTRATOS DO CLIENTE
        {
            dialogTitle = $"Ligação: {currentNPC} | Proposta Inicial: R$ {finalRevenue:F2}";
            GUI.Label(new Rect(20, 30, windowRect.width - 40, 60), dialogText, textStyle);
            if (dialogState == 3) {
                if (GUI.Button(new Rect(20, 100, 300, 40), "[Proteção Total] Enviarei agora.")) { dialogState = 4; if(currentNPC == "Peças") finalRevenue -= 150; UpdateRound2Text(); }
                if (GUI.Button(new Rect(340, 100, 300, 40), "[Taxar Risco Adicional] Adiciona 10%.")) { finalRevenue *= 1.10f; dialogState = 4; UpdateRound2Text(); }
            } else if (dialogState == 4) {
               if (!isSubmitting && GUI.Button(new Rect(20, 100, 400, 40), $"📝 Assinar Papelada por R$ {finalRevenue:F2}")) { 
                   isSubmitting = true; dialogState = 5; StartCoroutine(SignContractAPI()); 
               }
            }
        }
        else if (dialogState == 6) // BANCO TYCOON
        {
            dialogTitle = "Agência Bancária Nacional (Empréstimos)";
            GUI.Label(new Rect(20, 30, windowRect.width - 40, 60), dialogText, textStyle);
            if (!stateHasLoan && !isSubmitting && GUI.Button(new Rect(20, 100, 350, 40), "[Me dê os R$ 30.000 agora] Suportarei as parcelas!")) { 
                isSubmitting = true; dialogState = 5; StartCoroutine(TakeLoanAPI()); 
            }
            if (GUI.Button(new Rect(380, 100, 200, 40), "[Desligar Telefone]")) { ResetDialog(); }
        }
        else if (dialogState == 7) // OFICINA MECANICA (UPGRADES)
        {
            dialogTitle = "A Casa das Peças Vips - Bloqueador de Acidentes";
            GUI.Label(new Rect(20, 30, windowRect.width - 40, 60), dialogText, textStyle);
            
            if (!GlobalHasPremiumTires && !isSubmitting) {
                if (GUI.Button(new Rect(20, 100, 310, 40), "🛣️ Pneu Michelin (R$ 4.000)\nZera os buracos severos!")) { 
                    isSubmitting = true; dialogState = 5; StartCoroutine(BuyUpgradeAPI("tires")); 
                }
            } else if (GlobalHasPremiumTires) {
                GUI.Label(new Rect(20, 110, 310, 40), "🛣️ Você Comprou os Pneus!", textStyle);
            }

            if (!GlobalHasAdvancedGPS && !isSubmitting) {
                if (GUI.Button(new Rect(340, 100, 310, 40), "🛰️ Satélite Geo Militar (R$ 2.000)\nNunca mais vai se perder em desvios!")) { 
                    isSubmitting = true; dialogState = 5; StartCoroutine(BuyUpgradeAPI("gps")); 
                }
            } else if (GlobalHasAdvancedGPS) {
                GUI.Label(new Rect(340, 110, 310, 40), "🛰️ Satélite GPS Já Adquirido!", textStyle);
            }

            if (GUI.Button(new Rect(680, 100, 150, 40), "[Desligar]")) { ResetDialog(); }
        }
    }

    void StartDialogTree() {
        dialogState = 3;
        if (currentNPC == "Alimentos") dialogText = "<b>[Roberto]</b>: Rodovia esburacada. Você garante proteção total da carga?";
        else if (currentNPC == "Peças") dialogText = "<b>[Tanaka]</b>: Blocos de motor pesados. Vai aguentar os socos sem rachar?";
        else if (currentNPC == "Medicamentos") dialogText = "<b>[Dra. Silvia]</b>: Se as vacinas estragarem no baú quente eu te processo!";
        else if (currentNPC == "Petróleo") dialogText = "<b>[Carlos]</b>: Combustível classe A inflamável. Tem seguro contra explosões na pista?";
        else if (currentNPC == "Contêiner") dialogText = "<b>[Marçal]</b>: O navio chinês apita no porto amanhã. Suporta essa tonelagem extra?";
    }

    void UpdateRound2Text() {
        if (currentNPC == "Alimentos") dialogText = "<b>[Roberto]</b>: Tudo certo! Vou avisar os peões.";
        if (currentNPC == "Peças") dialogText = "<b>[Tanaka]</b>: Despache logo.";
        if (currentNPC == "Medicamentos") dialogText = "<b>[Dra. Silvia]</b>: Contrato assinado. Vida deles na sua rede.";
        if (currentNPC == "Petróleo") dialogText = "<b>[Carlos]</b>: Carga blindada! Caminhão tanque tá pesado, ande devagar e com Deus!";
        if (currentNPC == "Contêiner") dialogText = "<b>[Marçal]</b>: Papelada do Porto assinada. Libera no pátio!";
    }

    void ResetDialog() {
       dialogState = 0; // O Jogo Volta pro Modo Visualização
       currentNPC = "";
       // A UI Flutuante voltará a aparecer e o tempo do TycoonManager volta a correr em background
    }

    IEnumerator FetchCompanyStateForJulia() {
        using (UnityWebRequest request = UnityWebRequest.Get($"{GameConfig.COMPANY_BASE_URL}/{GlobalPlayerId}/state")) {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) {
                CompanyState state = JsonUtility.FromJson<CompanyState>(request.downloadHandler.text);
                companyFleet = state.fleetSize;
                
                // Mapeia novas Flags e o Caixa Real da DB do Estágio
                GlobalNetWorth = state.netWorth;
                GlobalLoanDebt = state.loanDebt;
                GlobalFleet = state.fleetSize;
                GlobalCurrentDay = state.currentDay; // REDUNDÂNCIA VITAL
                GlobalMaxDays = state.maxDays;

                GlobalHasPremiumTires = state.hasPremiumTires;
                GlobalHasAdvancedGPS = state.hasAdvancedGPS;
                stateHasLoan = state.hasBankLoan;

                int month = (GlobalCurrentDay / 30) + 1;
                demandMultiplier = 1.0f;
                if (month % 2 == 0) demandMultiplier *= 1.25f; 
                if (companyFleet >= 3) demandMultiplier *= 0.85f; 
                
                // BUGFIX: Só reseta o estado se estivermos no login ou menu, preservando sub-menus de decisão!
                if (dialogState <= 1) dialogState = 1;
            } else if (dialogState <= 0) dialogState = 1;
        }
    }

    // ----------------------------------------------------
    // -------------- CHAMADAS DE API E EVENTOS -----------
    // ----------------------------------------------------
    
    public void TriggerHUDRefresh() {
        if (dialogState != -1 && !EndGameLocked && !isSubmitting) StartCoroutine(FetchCompanyStateForJulia());
    }

    public void TriggerDailyReport(int passedDay) {
        if (dialogState == -1 || EndGameLocked || dialogState == 10) return;
        
        contractsAcceptedToday = 0; // Reset diário da regra de frota!

        // Magia Matemática de Tycoon
        float dailyProfit = GlobalNetWorth - startOfDayNetWorth;
        
        eventTitle = $"Fechamento Contábil DRE - Ciclo de 24h";
        
        string colorCode = dailyProfit >= 0 ? "<color=#00FF00>" : "<color=#FF0000>";
        string sign = dailyProfit >= 0 ? "+" : "";

        eventMessage = $"<b>[Júlia (RH Central)]</b>\nChefe, as frotas desligaram os motores. Bateu 24h!\n\nEste é o resumo das faturas e punições de todos os seus despachos de hoje (Dia {passedDay}): \n\n<size=22><b>Balanço Diário: {colorCode}{sign} R$ {dailyProfit:F2}</color></b></size>";
        
        loadedEventImage = npcJulia;
        dialogState = 11;
    }

    public void TriggerMonthlyReceipt(float cost) {
        if (dialogState == -1 || EndGameLocked || dialogState == 10) return; // Se já houver popup, descarta
        
        eventCost = 0f; // 0f Porque o Backend (PayBills) acabou de realizar a cobrança automaticamente!
        eventTitle = "Fechamento Mensal de Caixa!";
        eventMessage = $"<b>[Júlia Requisita Assinatura Tycoon]</b>\nO mês virou. Subtraí silenciosamente de sua conta do banco Tycoon o pagamento de todos os salários, manutenção da frota e eventuais juros do Agiota!\n\n<b>Despesas Livres do Mês: - R$ {cost:F2} </b>";
        loadedEventImage = npcJulia; // Aproveita e carrega a imagem do zap
        dialogState = 10;
        TriggerHUDRefresh();
    }

    public void TriggerRandomEvent() {
        if (dialogState == -1 || EndGameLocked || dialogState == 10) return;
        
        int s = Random.Range(1, 5); // Pode ser de 1 até 4
        switch(s) {
            case 1:
                eventCost = Random.Range(3000f, 6000f);
                eventTitle = "Relatório Dossiê: Acidente Envolvendo Terceiros (PERDA TOTAL)";
                eventMessage = $"<b>[Destruição na Pista - Flagrante]</b>\nUma colisão frontal severa ceifou a carga. O caminhão esmagou um veículo civil. Despesas hospitalares e reboque expedidas ao nosso CNPJ!\n\n<b>Prejuízo Absorvido: R$ {eventCost:F2}</b>";
                loadedEventImage = Resources.Load<Texture2D>("Events/evt_crash");
                break;
            case 2:
                eventCost = Random.Range(1500f, 3000f);
                eventTitle = "Relatório Dossiê: Desastre Natural em Rota";
                eventMessage = $"<b>[Deslizamento Interrompe Pista]</b>\nUm deslizamento soterrou de surpresa a passagem daquele canyon. Guindastes e retroescavadeiras cobram caro para liberar as 5 toneladas do asfalto!\n\n<b>Prejuízo Absorvido: R$ {eventCost:F2}</b>";
                loadedEventImage = Resources.Load<Texture2D>("Events/evt_landslide");
                break;
            case 3:
                eventCost = Random.Range(800f, 1500f);
                eventTitle = "Relatório Dossiê: Estouro de Pneu Calibrado a Fogo";
                eventMessage = $"<b>[Desgaste Térmico]</b>\nAlta velocidade no asfalto infernal furou o pneu a ponto de queimar. Bombeiros chamados para evitar fogo na carreta inteira.\n\n<b>Prejuízo Absorvido: R$ {eventCost:F2}</b>";
                loadedEventImage = Resources.Load<Texture2D>("Events/evt_flattire");
                break;
            case 4:
                eventCost = 0f;
                eventTitle = "Relatório Dossiê: Prevenção Tecnológica VIP";
                eventMessage = $"<b>[Oficina Tycoon Ativa]</b>\nSeu motorista foi brilhante e abriu o motor a tempo. O caminhão foi recuperado! Prevenção garantiu as entregas.\n\n<b>Efeito: Gastos Absolvidos. Sem Prejuízos Locais!</b>";
                loadedEventImage = Resources.Load<Texture2D>("Events/evt_workshop");
                break;
        }
        dialogState = 10;
    }

    IEnumerator ChargeEventAPI(float tax) {
        TripCreationPayload payload = new TripCreationPayload {
            playerId = GlobalPlayerId, origin = "Balanca Rodoviaria", destination = "Penalidade", revenue = 0,
            taxesAmount = 0, kmCosts = 0, netProfit = -tax, status = "Fined", contractorNPC = "Governo"
        };
        UnityWebRequest req = new UnityWebRequest(GameConfig.TRIPS_URL, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        yield return new WaitForSeconds(0.5f);
        isSubmitting = false; 
        TriggerHUDRefresh();
    }

    IEnumerator SignContractAPI() {
        string targetCoord = $"{Mathf.FloorToInt(Random.Range(2, 20))},{Mathf.FloorToInt(Random.Range(2, 20))}";
        TripCreationPayload payload = new TripCreationPayload {
            playerId = GlobalPlayerId, origin = "0,0", destination = targetCoord, revenue = finalRevenue,
            taxesAmount = finalRevenue * 0.12f, kmCosts = 0f, netProfit = finalRevenue - (finalRevenue * 0.12f), status = "Negotiating", contractorNPC = currentNPC
        };
        UnityWebRequest req = new UnityWebRequest(GameConfig.TRIPS_URL, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        yield return new WaitForSeconds(1.5f);
        if (req.result == UnityWebRequest.Result.Success) contractsAcceptedToday++; // Sucesso! Um caminhão a menos.
        isSubmitting = false; 
        ResetDialog();
        TriggerHUDRefresh(); // Agora ele puxa o saldo logo na largada do contrato!
        FindFirstObjectByType<TycoonTimeManager>()?.AdvanceHours(2);
    }

    IEnumerator BuyTruckAPI() {
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"{GameConfig.COMPANY_BASE_URL}/{GlobalPlayerId}/buy-truck", "")) {
            yield return request.SendWebRequest();
            yield return new WaitForSeconds(1.5f);
            isSubmitting = false; ResetDialog();
        }
    }

    IEnumerator TakeLoanAPI() {
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"{GameConfig.COMPANY_BASE_URL}/{GlobalPlayerId}/take-loan", "")) {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) Debug.Log("Empréstimo R$30.000 Caiu na Conta!");
            yield return new WaitForSeconds(1.0f);
            isSubmitting = false; ResetDialog();
        }
    }

    IEnumerator BuyUpgradeAPI(string typeStr) {
        UpgradePayload payload = new UpgradePayload { Type = typeStr };
        UnityWebRequest req = new UnityWebRequest($"{GameConfig.COMPANY_BASE_URL}/{GlobalPlayerId}/buy-upgrade", "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success) Debug.Log($"Upgrade Comercial Adquirido: {typeStr}");
        yield return new WaitForSeconds(1.0f);
        isSubmitting = false; ResetDialog();
    }
}

public class CompanyState { public int currentDay; public int maxDays; public int fleetSize; public float netWorth; public bool hasBankLoan; public bool hasPremiumTires; public bool hasAdvancedGPS; public float loanDebt; }
[System.Serializable] public class TripCreationPayload { public int playerId; public string origin; public string destination; public float revenue; public float taxesAmount; public float kmCosts; public float netProfit; public string status; public string contractorNPC; }
public class LoginPayload { public string Key; }
public class LoginResponse { public int id; public string name; public int currentDay; public int maxDays; }
public class UpgradePayload { public string Type; }
