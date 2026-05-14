using UnityEngine;
using System.Collections.Generic;

public class CityLightsManager : MonoBehaviour
{
    private List<Material> emissiveMaterials = new List<Material>();
    private List<Light> streetLights = new List<Light>();
    private bool areLightsOn = false;

    void Start()
    {
        // Encontra todos os objetos que podem ser luzes de rua ou janelas
        // Nota: Em um projeto real, você usaria tags ou camadas, mas aqui vamos buscar por nome/tipo
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.gameObject.name.Contains("Street") || l.gameObject.name.Contains("Lamp"))
            {
                streetLights.Add(l);
                l.enabled = false;
            }
        }

        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer r in allRenderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.name.Contains("Window") || m.name.Contains("Emissive") || m.name.Contains("Light"))
                {
                    emissiveMaterials.Add(m);
                    m.DisableKeyword("_EMISSION");
                }
            }
        }
        
        Debug.Log($"CityLightsManager: Monitorando {streetLights.Count} luzes e {emissiveMaterials.Count} materiais emissivos.");
    }

    void Update()
    {
        float hour = TycoonTimeManager.CurrentHour;
        bool shouldBeOn = (hour >= 18f || hour < 6f);

        if (shouldBeOn != areLightsOn)
        {
            ToggleLights(shouldBeOn);
        }
    }

    void ToggleLights(bool on)
    {
        areLightsOn = on;
        foreach (Light l in streetLights)
        {
            l.enabled = on;
        }

        foreach (Material m in emissiveMaterials)
        {
            if (on) m.EnableKeyword("_EMISSION");
            else m.DisableKeyword("_EMISSION");
        }
        
        Debug.Log(on ? "🌃 Luzes da Cidade Ligadas!" : "🌅 Luzes da Cidade Desligadas!");
    }
}
