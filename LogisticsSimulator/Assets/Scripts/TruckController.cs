using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Globalization;

public class TruckController : MonoBehaviour
{
    private string apiUrl = GameConfig.TRIPS_URL;
    
    // Controle
    public bool isMoving = false;
    private Vector3 targetPosition;
    public float movementSpeed = 2.5f;
    
    // Dados Locais
    private int currentTripId = -1;
    private float tripDistance = 0f;

    // Sistema de Eventos Hostis (Imprevistos e Narrativa)
    private string finalIncidentsLog = "";
    private float extraPunitiveCosts = 0f;
    private float eventTimer = 0f;

    // Variaveis Fisicas & Estéticas da Fase 2
    private Vector3 docaInicial;

    void Start()
    {
        docaInicial = transform.position;
        // Fumaça removida a pedido do CEO (Desativa qualquer sistema de partículas residual)
        foreach (var ps in GetComponentsInChildren<ParticleSystem>()) ps.gameObject.SetActive(false);

        Debug.Log("Caminhão Operacional! Checando pátio do Backend...");
        StartCoroutine(PollTripsRoutine());
    }

    void Update()
    {
        if (isMoving)
        {
            // Movimentação retilínea suave pelo asfalto
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            transform.LookAt(targetPosition);

            // Roda o Gerador de Destino/Imprevistos a cada 1.5s
            eventTimer += Time.deltaTime;
            if (eventTimer > 1.5f) { eventTimer = 0f; RollEventDice(); }

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                Debug.Log($"CHEGAMOS! Finalizando contrato {currentTripId}. Mandando os logs pro Chefe...");
                StartCoroutine(FinishTripAPI(currentTripId, tripDistance, extraPunitiveCosts, finalIncidentsLog));
            }
        }
        else
        {
            // Estacionamento reverso suave para a Doca Base
            if (Vector3.Distance(transform.position, docaInicial) > 0.5f) {
                transform.position = Vector3.MoveTowards(transform.position, docaInicial, (movementSpeed + 1f) * Time.deltaTime); 
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0,0,0), Time.deltaTime * 3f);
            }
        }
    }

    void RollEventDice()
    {
        int chanceTotal = 100;
        if (DynamicWeather.IsRaining) chanceTotal = 60; // Temporal Diminui os "números seguros" = Mais Acidentes!

        int dice = Random.Range(1, chanceTotal);
        
        if (dice <= 3) // Quebra Geral do Motor
        {
            Debug.LogWarning("🚨 PANE SEVERA!");
            extraPunitiveCosts += 1500.0f;
            AppendLog("Motor superaqueceu na chuva. 1.500 de Guincho acionado.");
            StartCoroutine(RoadblockDelay(3.5f));
        }
        else if (dice > 3 && dice <= 7) // Pneu e Lataria
        {
            if (DialogueSystem.GlobalHasPremiumTires) {
                // EVITOU GRAÇAS AO UPGRADE DO BANCO O_O
                AppendLog("Pneu Michelin aguentou buraco gigante fatiando a pista! Sem Custos.");
            } else {
                Debug.LogWarning("🚧 Buraco severo detectado!");
                extraPunitiveCosts += 400.0f; 
                AppendLog("Pneu estourou na carga pesada. Prejuízo de R$ 400 da lataria.");
                StartCoroutine(RoadblockDelay(1.5f)); 
            }
        }
        else if (dice > 7 && dice <= 11) // Desvio GPS
        {
            if (DialogueSystem.GlobalHasAdvancedGPS) {
                AppendLog("GPS Avançado recalculou Rodoanel Fechado em microssegundos. Desvio perfeito.");
            } else {
                Debug.LogWarning("🌧️ Estrada Fechada!");
                extraPunitiveCosts += 280.0f;
                AppendLog("Engarrafamento não previsto. Desvio manual gastou gasolina extra de R$ 280.");
                StartCoroutine(RoadblockDelay(2.5f)); 
            }
        }
    }

    void AppendLog(string message)
    {
        if (finalIncidentsLog != "") finalIncidentsLog += " | ";
        finalIncidentsLog += message;
    }

    IEnumerator RoadblockDelay(float seconds)
    {
        isMoving = false;
        yield return new WaitForSeconds(seconds);
        isMoving = true;
    }

    IEnumerator PollTripsRoutine()
    {
        while (true)
        {
            if (!isMoving) 
            {
                yield return StartCoroutine(GetTripsFromAPI());
            }
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator GetTripsFromAPI()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Trip[] trips = JsonHelper.GetJsonArray<Trip>(jsonResponse);

                foreach (var t in trips)
                {
                    if (t.status == "Negotiating")
                    {
                        StartTripLogistics(t);
                        break;
                    }
                }
            }
        }
    }

    void StartTripLogistics(Trip trip)
    {
        currentTripId = trip.id;
        finalIncidentsLog = "";
        extraPunitiveCosts = 0f;
        eventTimer = 0f;
        
        // Direciona o caminhão para o horizonte sul (Z negativo) acompanhando a rodovia asfaltada!
        targetPosition = new Vector3(docaInicial.x, docaInicial.y, docaInicial.z - 80f);
        
        tripDistance = Vector3.Distance(transform.position, targetPosition);
        
        // Contenção especial por Contratante
        if(trip.contractorNPC == "Medicamentos") {
            movementSpeed = 4.5f; // Caminhão Urgência Médica
        } else if(trip.contractorNPC == "Peças") {
            movementSpeed = 1.8f; // Carga Pesada (Lento)
        } else if(trip.contractorNPC == "Petróleo") {
            movementSpeed = 1.3f; // Explosivo Rastejante! Extremamente perigoso.
        } else if(trip.contractorNPC == "Contêiner") {
            movementSpeed = 1.9f; // Marítimo Super Pesado
        } else {
            movementSpeed = 2.5f; // Alimento e Padrão
        }
        isMoving = true;
    }

    IEnumerator FinishTripAPI(int id, float baseDistance, float extraCost, string incidentLogsText)
    {
        float simFuelCost = (baseDistance * 1.5f) + extraCost;

        string updateUrl = $"{apiUrl}/{id}/finish";
        
        FinishPayload payload = new FinishPayload 
        {
            finalFuelCost = simFuelCost,
            incidentLogs = incidentLogsText == "" ? "Nenhuma ocorrência. Viagem perfeita." : incidentLogsText
        };
        
        string jsonPayload = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(updateUrl, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            Debug.Log($"Incidente Computado e Pago via PUT. NetProfit diminuido no BD.");
            DialogueSystem ui = FindFirstObjectByType<DialogueSystem>();
            if (ui != null) {
                ui.TriggerHUDRefresh();
                ui.ShowToast($"💰 Frete Pago: Caixa Atualizado!");
            }
        } else {
            Debug.LogError($"Erro ao finalizar: {request.error}");
        }
    }

    public void ForceResetToGarage()
    {
        isMoving = false;
        transform.position = docaInicial;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        Debug.Log("Logística Resetada: Caminhão retornou à garagem para novo turno.");
    }
}

// ------ CLASSES AUXILIARES DE JSON ------
[System.Serializable]
public class FinishPayload
{
    public float finalFuelCost;
    public string incidentLogs;
}

[System.Serializable]
public class Trip
{
    public int id;
    public string origin;
    public string destination;
    public string status;
    public float netProfit;
    public string contractorNPC;
}

public static class JsonHelper
{
    public static T[] GetJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
