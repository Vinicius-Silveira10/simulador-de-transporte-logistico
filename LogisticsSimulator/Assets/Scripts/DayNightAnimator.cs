using UnityEngine;

public class DayNightAnimator : MonoBehaviour
{
    [Header("Configuração de Atmosfera")]
    private Light sunLight;
    
    // Paleta de Cores Cinematográficas
    private Color dawnColor = new Color(1f, 0.5f, 0.3f); // Alvorecer Laranja
    private Color dayColor = new Color(1f, 1f, 0.95f);  // Dia Branco Quente
    private Color duskColor = new Color(0.6f, 0.3f, 0.7f); // Crepúsculo Roxo/Rosa
    private Color nightColor = new Color(0.05f, 0.05f, 0.2f); // Noite Azul Profunda

    void Start()
    {
        sunLight = GetComponent<Light>();
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
    }

    void Update()
    {
        // FORÇADO: Sempre Meio-Dia para visibilidade Elite
        float hour = 12f; 
        float sunRotation = (hour * 15f) - 90f; 
        transform.rotation = Quaternion.Euler(sunRotation, -30f, 0f);

        UpdateAtmosphere(hour);
    }

    void UpdateAtmosphere(float hour)
    {
        Color targetSky;
        float intensity;
        float fogDensity;

        // MODO ELITE DIA PERMANENTE
        targetSky = dayColor;
        intensity = 1.2f;
        fogDensity = 0.003f;

        // Aplica as mudanças no RenderSettings (Atmosfera Global)
        Camera.main.backgroundColor = targetSky;
        RenderSettings.fogColor = targetSky;
        RenderSettings.fogDensity = fogDensity;
        
        if (sunLight != null)
        {
            sunLight.intensity = intensity;
            sunLight.color = targetSky;
            // Desativa sombras pesadas à noite para performance e estética
            sunLight.shadows = (hour > 19f || hour < 5f) ? LightShadows.None : LightShadows.Soft;
        }
    }
}
