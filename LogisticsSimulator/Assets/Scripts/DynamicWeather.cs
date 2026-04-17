using UnityEngine;

public class DynamicWeather : MonoBehaviour
{
    public static bool IsRaining = false;
    private ParticleSystem rainSystem;
    private Material groundMat;
    private float weatherTimer = 0f;

    void Start()
    {
        Debug.Log("Satélite Meteorológico Ativado!");
        
        GameObject rainObj = new GameObject("TempestadeLogistica");
        rainObj.transform.position = new Vector3(0, 15, 10);
        rainObj.transform.rotation = Quaternion.Euler(90, 0, 0); 
        
        rainSystem = rainObj.AddComponent<ParticleSystem>();
        var main = rainSystem.main;
        main.startColor = new Color(0.6f, 0.7f, 0.9f, 0.8f);
        main.startSize = 0.08f;
        main.startSpeed = 40f;
        main.startLifetime = 1.0f;
        main.maxParticles = 8000;

        var em = rainSystem.emission;
        em.rateOverTime = 0f;

        var shape = rainSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(60, 60, 1);

        var vel = rainSystem.velocityOverLifetime;
        vel.enabled = true;
        vel.yMultiplier = -30f;
        
        var ren = rainSystem.GetComponent<ParticleSystemRenderer>();
        // Tentativa de puxar um shader de particula se existir, senao default
        Material ptMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        if(ptMat != null) ren.material = ptMat;
        ren.lengthScale = 15f;
        ren.renderMode = ParticleSystemRenderMode.Stretch; 
        
        GameObject patio = GameObject.Find("PatioLogistico");
        if(patio != null) groundMat = patio.GetComponent<Renderer>().material;
    }

    void Update()
    {
        weatherTimer += Time.deltaTime;
        if (weatherTimer > 8f) { // Gira roleta a cada 8s
            weatherTimer = 0f;
            int chance = Random.Range(0, 100);
            if (chance < 25 && !IsRaining) StartRain();
            else if (chance >= 25 && chance < 60 && IsRaining) StopRain();
        }
    }

    void StartRain() {
        IsRaining = true;
        var em = rainSystem.emission; em.rateOverTime = 2500f; // Temporal BRABO
        if (groundMat != null) {
            groundMat.color = new Color(0.04f, 0.04f, 0.05f);
            groundMat.SetFloat("_Glossiness", 0.98f); // Molhado
        }
        Debug.Log("TEMPESTADE INICIADA! Asfalto perigoso!");
    }

    void StopRain() {
        IsRaining = false;
        var em = rainSystem.emission; em.rateOverTime = 0f;
        if (groundMat != null) {
            groundMat.color = new Color(0.12f, 0.12f, 0.13f);
            groundMat.SetFloat("_Glossiness", 0.2f); // Seco
        }
    }
}
