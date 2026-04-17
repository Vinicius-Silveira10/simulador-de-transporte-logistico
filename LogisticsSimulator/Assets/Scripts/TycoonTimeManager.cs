using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TycoonTimeManager : MonoBehaviour
{
    // O Jogo gira as horas em tempo real. Cada "Dia" financeiro são 4 segundos na vida real.
    public float secondsPerDay = 4.0f; 
    private float timer = 0f;
    private int localDayCounter = 0; // Para uso em loop de 30 dias

    void Start()
    {
        Debug.Log("Relógio Empresarial Aguardando Login Multiplayer...");
    }

    void Update()
    {
        // Trava totalmente o relógio se o Menu, Negociação ou Evento estiverem abertos na tela! (Mecânica de Turnos/Pause)
        if (DialogueSystem.GlobalPlayerId == 0 || DialogueSystem.EndGameLocked) return;
        
        // Verifica se a tela está limpa (estado 0). Se o jogador estiver focando na aba, o tempo congela.
        DialogueSystem ui = FindObjectOfType<DialogueSystem>();
        if (ui != null && ui.GetDialogState() != 0) return;

        timer += Time.deltaTime;
        if (timer >= secondsPerDay)
        {
            timer = 0f;
            StartCoroutine(AdvanceDayAPI());
        }
    }

    IEnumerator AdvanceDayAPI()
    {
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"http://localhost:5041/api/Company/{DialogueSystem.GlobalPlayerId}/tick-day", ""))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
               DayResponse resp = JsonUtility.FromJson<DayResponse>(request.downloadHandler.text);
               int novoDia = resp.currentDay;
               
               // CORREÇÃO: Religa o relógio interno no front da Unity e a Central
               DialogueSystem.GlobalCurrentDay = novoDia;

               // ATUALIZA A HUD E OS VALORES NA TELA
               FindObjectOfType<DialogueSystem>()?.TriggerHUDRefresh();

               // FIM DE TEMPORADA BATALHA! GlobalMaxDays Atingidos
               if (novoDia >= DialogueSystem.GlobalMaxDays && !DialogueSystem.EndGameLocked) {
                   DialogueSystem.EndGameLocked = true;
                   Debug.Log($"TEMPORADA ENCERRADA! {DialogueSystem.GlobalMaxDays} Dias atingidos.");
               }
               
               localDayCounter++;

               // EVENTO DE NPC (Ex: Policial parando na estrada) -> A cada 7 dias
               if (novoDia % 7 == 0 && !DialogueSystem.EndGameLocked) {
                   FindObjectOfType<DialogueSystem>()?.TriggerRandomEvent();
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
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm($"http://localhost:5041/api/Company/{DialogueSystem.GlobalPlayerId}/pay-bills", ""))
        {
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
