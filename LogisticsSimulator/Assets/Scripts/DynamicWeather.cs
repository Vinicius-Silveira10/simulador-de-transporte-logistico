using UnityEngine;

public class DynamicWeather : MonoBehaviour
{
    public static bool IsRaining = false;
    private ParticleSystem rainSystem;
    private Material groundMat;
    private float weatherTimer = 0f;

    void Start()
    {
        IsRaining = false;
        Debug.Log("CHUVAS DESATIVADAS PERMANENTEMENTE.");
        
        // Destruição Forçada de qualquer sistema de chuva residual na cena
        GameObject oldRain = GameObject.Find("TempestadeLogistica");
        if (oldRain != null) Destroy(oldRain);

        // Se houver algum ParticleSystem de chuva rodando, desativa
        foreach (var ps in FindObjectsOfType<ParticleSystem>()) {
            if (ps.name.Contains("Chuva") || ps.name.Contains("Rain") || ps.name.Contains("Tempestade")) {
                ps.Stop();
                ps.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Desativado pelo usuário
    }

    void StartRain() {
        IsRaining = true;
        var em = rainSystem.emission; em.rateOverTime = 2500f; // Temporal BRABO
        if (groundMat != null) {
            groundMat.color = new Color(0.04f, 0.04f, 0.05f);
            groundMat.SetFloat("_Glossiness", 0.98f); // Molhado
        }
        
        // Efeito Atmosférico de Neblina por Chuva (Suave para o modo Dia)
        RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);
        RenderSettings.fogDensity = 0.01f; // Visibilidade mantida
        
        Debug.Log("TEMPESTADE INICIADA! Asfalto perigoso!");
    }

    void StopRain() {
        IsRaining = false;
        var em = rainSystem.emission; em.rateOverTime = 0f;
        if (groundMat != null) {
            groundMat.color = new Color(0.12f, 0.12f, 0.13f);
            groundMat.SetFloat("_Glossiness", 0.2f); // Seco
        }
        
        // Restaura a neblina padrão Elite
        RenderSettings.fogDensity = 0.003f;
    }
}
