using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TycoonTimeManager : MonoBehaviour
{
    // Sistema Tycoon em Turnos! O Tempo não passa mais sozinho.
    private int localDayCounter = 0; // Para uso em loop de 30 dias
    
    // O Turno começa às 02:00 (Pedido pelo CEO)
    public static float CurrentHour = 2f; 
    
    // A HUd lê isso para desenhar o Relógio
    public static float CurrentTimeRatio { get { return CurrentHour / 24f; } }

    void Start()
    {
        Debug.Log("Tycoon de Turnos Aguardando Ações Táticas...");
    }

    public void AdvanceHours(int hoursToAdd)
    {
        if (DialogueSystem.GlobalPlayerId == 0 || DialogueSystem.EndGameLocked) return;
        
        CurrentHour += hoursToAdd;
        Debug.Log($"Tempo Avançado! Hora atual: {CurrentHour}:00");

        if (CurrentHour >= 24f)
        {
            EndDayEarly();
        }
    }

    public void EndDayEarly()
    {
        if (DialogueSystem.GlobalPlayerId == 0 || DialogueSystem.EndGameLocked) return;
        
        CurrentHour = 2f; // Reseta O Relógio de Turnos pra 2 da Manhã automaticamente!! Impede o Loop Infinito de spams.
        StartCoroutine(AdvanceDayAPI());
    }

    IEnumerator AdvanceDayAPI()
    {
        // Bugfix: Unity Web Request falha no envio de POST sem payload explícito
        string url = $"{GameConfig.COMPANY_BASE_URL}/{DialogueSystem.GlobalPlayerId}/tick-day";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
               DayResponse resp = JsonUtility.FromJson<DayResponse>(request.downloadHandler.text);
               int novoDia = resp.currentDay;
               
               // CORREÇÃO: Religa o relógio interno no front da Unity e a Central
               int diaAnterior = DialogueSystem.GlobalCurrentDay;
               DialogueSystem.GlobalCurrentDay = novoDia;

               // ATUALIZA A HUD E OS VALORES NA TELA E DISPARA O EVENTO DE "FECHAMENTO DE CAIXA"
               DialogueSystem ui = FindFirstObjectByType<DialogueSystem>();
               if (ui != null) {
                   ui.TriggerHUDRefresh();
                   // A Júlia agora para a tela renderizando o Lucro do Dia, por isso o Toast sumiu daqui!
                   ui.TriggerDailyReport(diaAnterior);
               }

               // FIM DE TEMPORADA BATALHA! GlobalMaxDays Atingidos
               if (novoDia >= DialogueSystem.GlobalMaxDays && !DialogueSystem.EndGameLocked) {
                   DialogueSystem.EndGameLocked = true;
                   Debug.Log($"TEMPORADA ENCERRADA! {DialogueSystem.GlobalMaxDays} Dias atingidos.");
               }
               
               localDayCounter++;

               // EVENTO DE NPC (Ex: Policial parando na estrada) -> A cada 7 dias
               if (novoDia % 7 == 0 && !DialogueSystem.EndGameLocked) {
                   FindFirstObjectByType<DialogueSystem>()?.TriggerRandomEvent();
               }
               if (localDayCounter >= 30 && !DialogueSystem.EndGameLocked) {
                   localDayCounter = 0;
                   StartCoroutine(PayMonthlyBillsAPI());
               }
            }
        }
    }

    IEnumerator PayMonthlyBillsAPI()
    {
        string url = $"{GameConfig.COMPANY_BASE_URL}/{DialogueSystem.GlobalPlayerId}/pay-bills";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success) {
               PayBillsResponse bills = JsonUtility.FromJson<PayBillsResponse>(request.downloadHandler.text);
               FindObjectOfType<DialogueSystem>()?.TriggerMonthlyReceipt(bills.paidAmount);
            }
        }
    }
}

public class DayResponse { public int currentDay; }
public class PayBillsResponse { public float paidAmount; public float newBalance; public float loanDebt; }
