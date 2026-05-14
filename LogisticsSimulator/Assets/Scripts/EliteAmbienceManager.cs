using UnityEngine;
using System.Collections.Generic;

public class EliteAmbienceManager : MonoBehaviour
{
    private GameObject roadPrefab;
    private List<GameObject> buildingPrefabs = new List<GameObject>();
    private List<GameObject> naturePrefabs = new List<GameObject>();
    
    public float blockSize = 40f;

    void Start()
    {
        // Limpeza Total para o Reset de Estética
        foreach (var old in GameObject.FindObjectsOfType<GameObject>()) {
            if (old.name.Contains("(Clone)") || old.name.Contains("Elite_Ground_Plane")) {
                Destroy(old);
            }
        }
        LoadAssets();
    }

    void LoadAssets()
    {
        GameObject[] all = Resources.LoadAll<GameObject>("CityAssets");
        if (all == null || all.Length == 0) return;

        foreach (var go in all)
        {
            if (go.name.Contains("Road Tile")) roadPrefab = go;
            else if (go.name.Contains("Building")) buildingPrefabs.Add(go);
            else if (go.name.Contains("Nature")) naturePrefabs.Add(go);
        }

        GenerateMinimalistWorld();
        ApplyEliteAtmosphere();
    }

    void GenerateMinimalistWorld()
    {
        // 1. Chão de Base (Grama/Terra Natural)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Elite_Ground_Plane";
        floor.transform.position = new Vector3(0, -0.05f, 0);
        floor.transform.localScale = new Vector3(500, 1, 500);
        floor.GetComponent<Renderer>().material.color = new Color(0.1f, 0.2f, 0.1f); // Verde Escuro Natural

        // 2. A Rodovia Amarela (Eixo Central)
        for (int z = -100; z <= 20; z++)
        {
            Vector3 pos = new Vector3(0, 0.05f, z * blockSize);
            if (roadPrefab != null) {
                GameObject r = Instantiate(roadPrefab, pos, Quaternion.identity);
                r.transform.localScale = new Vector3(blockSize/10f, 1, blockSize/10f);
            }
        }

        // 3. CINTURÃO VERDE (Floresta Densa nas Laterais e Fundo)
        for (int x = -10; x <= 10; x++)
        {
            for (int z = -40; z <= 10; z++)
            {
                // Deixa o centro (estrada) livre
                if (Mathf.Abs(x) < 2) continue;

                Vector3 pos = new Vector3(x * blockSize + Random.Range(-10, 10), 0, z * blockSize + Random.Range(-10, 10));
                if (naturePrefabs.Count > 0) {
                    GameObject tree = Instantiate(naturePrefabs[Random.Range(0, naturePrefabs.Count)], pos, Quaternion.identity);
                    tree.transform.localScale = Vector3.one * Random.Range(1.5f, 3.5f); // Árvores variadas e grandes
                }
            }
        }
    }

    void ApplyEliteAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.002f;
        RenderSettings.fogColor = new Color(0.1f, 0.15f, 0.2f); // Azul Profundo Industrial
        
        // Ajusta a luz direcional se houver
        Light sun = FindObjectOfType<Light>();
        if (sun != null) {
            sun.intensity = 0.8f;
            sun.color = new Color(0.9f, 0.95f, 1f);
        }
    }
}
