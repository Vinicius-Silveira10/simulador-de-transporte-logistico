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
        
        cachedTruck = FindObjectOfType<TruckController>();
        cachedTime = FindObjectOfType<TycoonTimeManager>();

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

        // AUTO-INJEÇÃO DE AMBIENTAÇÃO ELITE (Task 1.1)
        if (FindObjectOfType<EliteAmbienceManager>() == null) {
            new GameObject("Elite_Ambience_Generator").AddComponent<EliteAmbienceManager>();
        }
    }

    void OnGUI()
    {
        // === CONFIGURAÇÃO DE DESIGN PREMIUM ===
        Color glassBlue = new Color(0.05f, 0.1f, 0.2f, 0.92f);
        Color accentCyan = new Color(0.02f, 0.7f, 0.82f, 1f);
        Color accentGold = new Color(1f, 0.84f, 0f, 1f);
        
        windowRect = new Rect(20, Screen.height - 240, Screen.width - 40, 220); 
        GUI.color = Color.white;
        
        GUIStyle textStyle = new GUIStyle(GUI.skin.label) { 
            fontSize = 15, 
            wordWrap = true, 
            richText = true,
            fontStyle = FontStyle.Normal
        };
        
        GUIStyle headerStyle = new GUIStyle(textStyle) {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };

        // >>> DESENHA A HUD FINANCEIRA (TOP BAR GLASSMORPISM) <<<
        if (GlobalPlayerId != 0 && dialogState != -1) {
            GUI.backgroundColor = new Color(0.02f, 0.05f, 0.1f, 0.95f);
            GUI.Box(new Rect(0, 0, Screen.width, 45), ""); // Fundo da Barra
            
            // Linha de acento inferior (Cyan Glow)
            GUI.color = accentCyan;
            GUI.DrawTexture(new Rect(0, 43, Screen.width, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            int h = Mathf.FloorToInt(TycoonTimeManager.CurrentHour);
            string digitalClock = $"{h:D2}:00";
            
            string hudText = $"<color=#00e5ff>🗓️ DIA {GlobalCurrentDay}</color>  |  <color=#ffd700>⏰ {digitalClock}</color>  |  💰 CAIXA: <color=#00ff88>R$ {GlobalNetWorth:N2}</color>  |  🚚 FROTA: <b>{GlobalFleet}</b>";
            GUI.Label(new Rect(25, 8, Screen.width, 30), hudText, headerStyle);
        }

        // >>> JANELAS DE EVENTO (MODAL CENTRAL) <<<
        if (dialogState == 10 || dialogState == 11) {
            GUI.backgroundColor = glassBlue;
            Rect evtRect = new Rect(Screen.width/2 - 350, Screen.height/2 - 260, 700, 520);
            
            // Sombra externa simples
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.Box(new Rect(evtRect.x + 5, evtRect.y + 5, evtRect.width, evtRect.height), "");
            GUI.color = Color.white;
            
            GUI.Window(99, evtRect, dialogState == 10 ? DrawEventScreen : DrawDailyReportScreen, "");
            return;
        }

        if (dialogState == -1) // LOGIN SCREEN
        {
            GUI.backgroundColor = new Color(0.05f, 0.15f, 0.25f, 0.98f);
            GUI.Window(0, new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 200), DrawLoginScreen, "🔐 ACESSO RESTRITO: TERMINAL LOGÍSTICO");
            return;
        }

        if (EndGameLocked) // END SCREEN
        {
            GUI.backgroundColor = new Color(0.3f, 0.05f, 0.05f, 0.95f);
            GUI.Window(0, new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 200), (id) => {
                GUI.Label(new Rect(20, 50, 560, 100), $"<size=22><b>TEMPORADA ENCERRADA!</b></size>\n\nMeta de {GlobalMaxDays} dias atingida. Verifique sua posição no Ranking Web.", textStyle);
            }, "🏆 RESULTADO FINAL");
            return;
        }

        // >>> BOTÕES DE AÇÃO RÁPIDA (CANTO INFERIOR DIREITO) <<<
        if (dialogState == 0) {
            // Background blur-like panel for buttons
            GUI.backgroundColor = new Color(0, 0, 0, 0.6f);
            GUI.Box(new Rect(Screen.width - 290, Screen.height - 130, 270, 115), "");
            
            GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 0.9f);
            if (GUI.Button(new Rect(Screen.width - 275, Screen.height - 115, 120, 45), "⏩ +1 HORA")) {
                cachedTime?.AdvanceHours(1);
            }

            GUI.backgroundColor = new Color(0.6f, 0.1f, 0.1f, 0.9f);
            if (GUI.Button(new Rect(Screen.width - 145, Screen.height - 115, 120, 45), "🌙 FECHAR")) {
                if (cachedTruck != null) cachedTruck.ForceResetToGarage();
                cachedTime?.EndDayEarly();
            }

            GUI.backgroundColor = accentCyan;
            if (GUI.Button(new Rect(Screen.width - 275, Screen.height - 60, 250, 45), "📞 ABRIR PABX CENTRAL")) {
                StartCoroutine(FetchCompanyStateForJulia());
            }
            return;
        }

        if (dialogState == 5) return; 
        
        GUI.backgroundColor = glassBlue;
        
        if (dialogState == 2 && currentNPC != "Vendedor") {
            windowRect = new Rect(Screen.width/2 - 470, Screen.height/2 - 220, 940, 440); 
        }

        windowRect = GUI.Window(0, windowRect, DrawDialogWindow, "");

        // >>> TOAST SYSTEM (NOTIFICAÇÕES MODERNAS) <<<
        int toastY = 60;
        for (int i = activeToasts.Count - 1; i >= 0; i--) {
            Toast t = activeToasts[i];
            float alpha = t.time > 1f ? 1f : t.time;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            
            GUIStyle toastStyle = new GUIStyle(GUI.skin.box) { 
                fontSize = 14, 
                fontStyle = FontStyle.Bold, 
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            
            GUI.backgroundColor = new Color(0.02f, 0.7f, 0.82f, 0.85f * alpha);
            GUI.Box(new Rect(Screen.width - 320, toastY, 300, 35), t.text, toastStyle);
            toastY += 40;
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
            startOfDayNetWorth = GlobalNetWorth; 
            dialogState = 0; 
        }
    }

    void DrawDialogWindow(int windowID)
    {
        GUIStyle textStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, richText = true };
        Color accentCyan = new Color(0.02f, 0.7f, 0.82f, 1f);

        if (dialogState == 1) // MENU MASTER
        {
            GUI.Label(new Rect(25, 20, windowRect.width - 50, 40), "<color=#00e5ff><b>CENTRAL DE OPERAÇÕES LOGÍSTICAS</b></color>", textStyle);
            float btnW = 210; float btnH = 45; float gap = 15;
            GUI.backgroundColor = new Color(0.1f, 0.3f, 0.6f, 0.9f);
            if (GUI.Button(new Rect(25, 90, btnW, btnH), "💼 JÚLIA (GESTÃO)")) { 
                currentNPC = "Julia"; 
                dialogState = 2; 
                StartCoroutine(FetchCompanyStateForJulia()); 
            }
            GUI.backgroundColor = new Color(0.7f, 0.4f, 0.1f, 0.9f);
            if (GUI.Button(new Rect(25 + (btnW + gap), 90, btnW, btnH), "💰 JUCA (FROTA)")) { 
                currentNPC = "Vendedor"; 
                dialogText = "<b>[Juca Vendas]</b>: Caminhão zero no pátio, R$ 12.000 à vista. Leva?"; 
                dialogState = 2; 
            }
            GUI.backgroundColor = new Color(0.1f, 0.5f, 0.2f, 0.9f);
            if (GUI.Button(new Rect(25 + (btnW + gap) * 2, 90, btnW, btnH), "🏦 BANCO (CRÉDITO)")) { 
                currentNPC = "Banco";
                dialogState = 6; 
            }
            GUI.backgroundColor = new Color(0.5f, 0.1f, 0.5f, 0.9f);
            if (GUI.Button(new Rect(25 + (btnW + gap) * 3, 90, btnW, btnH), "🔧 OFICINA (UPGRADES)")) { 
                currentNPC = "Oficina";
                dialogState = 7; 
            }
            GUI.backgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.8f);
            if (GUI.Button(new Rect(windowRect.width - 100, 10, 80, 30), "SAIR")) { ResetDialog(); }
        }
        else if (dialogState == 2 && currentNPC != "Vendedor") // JÚLIA
        {
            GUI.color = new Color(1,1,1,0.1f);
            GUI.DrawTexture(new Rect(0,0, windowRect.width, 100), Texture2D.whiteTexture);
            GUI.color = Color.white;
            if (npcJulia != null) GUI.DrawTexture(new Rect(25, 15, 70, 70), npcJulia, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(110, 20, windowRect.width - 150, 80), $"<size=18><b>Júlia - Gerência</b></size>\n{dialogText}", textStyle);
            if (contractsAcceptedToday >= GlobalFleet) {
                GUI.Label(new Rect(20, 150, windowRect.width - 40, 200), "<size=24><color=#ff4444><b>⚠️ TODA A FROTA ESTÁ EM TRÂNSITO</b></color></size>", new GUIStyle(textStyle) { alignment = TextAnchor.MiddleCenter });
            } else {
                float cardW = 170; float cardH = 240; float cardGap = 10;
                string[] npcs = { "Roberto", "Tanaka", "Silvia", "Carlos", "Marçal" };
                Texture2D[] pics = { npcRoberto, npcTanaka, npcSilvia, npcCarlos, npcMarcal };
                float[] revs = { 800, 1500, 3500, 5000, 2800 };
                string[] types = { "Alimentos", "Peças", "Vacinas", "Petróleo", "Contêiner" };
                for(int i=0; i<5; i++) {
                    Rect r = new Rect(25 + (cardW + cardGap) * i, 120, cardW, cardH);
                    GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
                    GUI.Box(r, "");
                    if (pics[i] != null) GUI.DrawTexture(new Rect(r.x+10, r.y+10, cardW-20, 120), pics[i], ScaleMode.ScaleToFit);
                    GUI.Label(new Rect(r.x+10, r.y+140, cardW-20, 40), $"<b>{npcs[i]}</b>", textStyle);
                    GUI.backgroundColor = accentCyan;
                    if (GUI.Button(new Rect(r.x+10, r.y+190, cardW-20, 35), $"R$ {revs[i] * demandMultiplier:F0}")) {
                        currentNPC = types[i]; finalRevenue = revs[i] * demandMultiplier; StartDialogTree();
                    }
                }
            }
            GUI.backgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.8f);
            if (GUI.Button(new Rect(windowRect.width - 100, 10, 80, 30), "VOLTAR")) { dialogState = 1; }
        }
        else if (dialogState == 2 && currentNPC == "Vendedor") // JUCA (FROTA)
        {
            if (npcCarlos != null) GUI.DrawTexture(new Rect(25, 20, 100, 100), npcCarlos, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(140, 30, windowRect.width - 180, 80), $"<size=18><b>Juca - Gestor de Frota</b></size>\n{dialogText}", textStyle);
            
            GUI.backgroundColor = accentCyan;
            if (!isSubmitting && GUI.Button(new Rect(140, 120, 350, 50), "🚚 COMPRAR CAMINHÃO (R$ 12.000)")) {
                isSubmitting = true;
                dialogState = 5; 
                StartCoroutine(BuyTruckAPI());
            }
            
            GUI.backgroundColor = Color.grey;
            if (GUI.Button(new Rect(windowRect.width - 100, 10, 80, 30), "VOLTAR")) { dialogState = 1; }
        }
        else if (dialogState == 3 || dialogState == 4) // CONTRATOS
        {
            GUI.Label(new Rect(30, 30, windowRect.width - 60, 60), $"<size=18><b>PROPOSTA: {currentNPC}</b></size>\n{dialogText}", textStyle);
            if (dialogState == 3) {
                GUI.backgroundColor = accentCyan;
                if (GUI.Button(new Rect(30, 100, 250, 45), "ACEITAR")) { dialogState = 4; UpdateRound2Text(); }
            } else if (dialogState == 4) {
                GUI.backgroundColor = new Color(0, 1, 0.5f, 0.9f);
                if (!isSubmitting && GUI.Button(new Rect(30, 100, 400, 50), $"📝 ASSINAR: R$ {finalRevenue:N2}")) { 
                    isSubmitting = true; dialogState = 5; StartCoroutine(SignContractAPI()); 
                }
            }
        }
        else if (dialogState == 6) // BANCO
        {
            GUI.Label(new Rect(25, 20, windowRect.width - 50, 80), $"<size=18><b>🏦 Banco Tycoon - Crédito</b></size>\n{dialogText}", textStyle);
            if (!stateHasLoan && !isSubmitting) {
                GUI.backgroundColor = accentCyan;
                if (GUI.Button(new Rect(25, 100, 350, 45), "TOMAR EMPRÉSTIMO (R$ 30.000)")) { isSubmitting = true; dialogState = 5; StartCoroutine(TakeLoanAPI()); }
            }
            GUI.backgroundColor = Color.grey;
            if (GUI.Button(new Rect(windowRect.width - 100, 10, 80, 30), "VOLTAR")) { ResetDialog(); }
        }
        else if (dialogState == 7) // OFICINA
        {
            GUI.Label(new Rect(25, 20, windowRect.width - 50, 80), $"<size=18><b>🔧 Oficina Especializada</b></size>\n{dialogText}", textStyle);
            if (!GlobalHasPremiumTires) {
                GUI.backgroundColor = accentCyan;
                if (GUI.Button(new Rect(25, 100, 250, 45), "PNEU PREMIUM (R$ 4.000)")) { isSubmitting = true; dialogState = 5; StartCoroutine(BuyUpgradeAPI("tires")); }
            }
            if (!GlobalHasAdvancedGPS) {
                GUI.backgroundColor = accentCyan;
                if (GUI.Button(new Rect(300, 100, 250, 45), "GPS MILITAR (R$ 2.000)")) { isSubmitting = true; dialogState = 5; StartCoroutine(BuyUpgradeAPI("gps")); }
            }
            GUI.backgroundColor = Color.grey;
            if (GUI.Button(new Rect(windowRect.width - 100, 10, 80, 30), "VOLTAR")) { ResetDialog(); }
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
        FindObjectOfType<TycoonTimeManager>()?.AdvanceHours(2);
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
