using UnityEngine;
using UnityEditor;

public class SceneAutomator : Editor
{
    [MenuItem("Logistics/Auto-Setup Scene")]
    public static void SetupScene()
    {
        Debug.Log("Iniciando Montagem da Cidade Tycoon (High-Quality Models)...");

        // 0. Limpeza Geral (Agora com o método moderno do Unity 6)
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = allObjects.Length - 1; i >= 0; i--) {
            GameObject obj = allObjects[i];
            if (obj != null) {
                string n = obj.name;
                if (n.Contains("Patio") || n.Contains("Garage") || n.Contains("Camera") || 
                    n.Contains("Directional") || n.Contains("Caminhao") || n.Contains("Truck") || 
                    n.Contains("Trailer") || n.Contains("Vehicle") || n.Contains("Building") || 
                    n.Contains("Road") || n.Contains("Estrada") || n.Contains("Armazem") || 
                    n.Contains("Oficina") || n.Contains("Rock") || n.Contains("Controlador") || 
                    n.Contains("Base") || n.Contains("Rodovia")) {
                    DestroyImmediate(obj);
                }
            }
        }

        // 1. O Sol Tycoon
        GameObject lightObj = new GameObject("Directional Light");
        Light dirLight = lightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.intensity = 1.3f;
        dirLight.color = new Color(1f, 0.95f, 0.9f); 
        dirLight.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(35, -45, 0);
        lightObj.AddComponent<DayNightAnimator>();

        // 2. A Câmera e Gerenciadores
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.4f, 0.55f, 0.7f);
        camObj.transform.position = new Vector3(0, 16, -24);
        camObj.transform.rotation = Quaternion.Euler(30, 0, 0);
        camObj.AddComponent<TycoonTimeManager>();
        camObj.AddComponent<DialogueSystem>();

        // ==========================================
        // CARREGAMENTO DE PREFABS DA ASSET STORE
        // ==========================================
        GameObject prefabFactory = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Buildings/Building_Factory.prefab");
        GameObject prefabAutoService = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Buildings/Building_Auto Service.prefab");
        GameObject prefabTruck = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Vehicles/Vehicle with Separated Wheels/Vehicle_Truck_color03_separate.prefab");
        GameObject prefabRoad = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LowpolyStreetPack/Prefabs/Roads/Streets/Road_Streight.prefab");
        GameObject prefabRockBig = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Natures/Natures_Rock_Big.prefab");
        GameObject prefabCone = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LowpolyStreetPack/Prefabs/StreetProps/RoadBlocks/RoadCone_A.prefab");

        // Carros para a Automação de IA
        GameObject prefabAmbulance = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Vehicles/Vehicle with Static Wheels/Vehicle_Ambulance.prefab");
        GameObject prefabCar1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Vehicles/Vehicle with Static Wheels/Vehicle_Car_color01.prefab");
        GameObject prefabBus = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Vehicles/Vehicle with Static Wheels/Vehicle_Bus_color01.prefab");

        if (prefabFactory == null || prefabTruck == null) {
            Debug.LogError("ERRO: Prefabs Low Poly não encontrados nos caminhos esperados. Certifique-se de que a importação terminou!");
            return;
        }

        // ==========================================
        // PALCO 1: BASE LOGÍSTICA PRINCIPAL (z: 0)
        // ==========================================
        GameObject baseLogistica = new GameObject("Base_Logistica_Z0");

        // O Asfalto do Pátio (Avenida levando até a fábrica)
        if (prefabRoad != null) {
            for (int r = -4; r <= 1; r++) {
                InstanciarModelo(prefabRoad, baseLogistica.transform, new Vector3(0, 0, r * 10f), Quaternion.Euler(-90, 0, 0));
            }
        }

        // Galpão Profissional Instalado
        GameObject hq = InstanciarModelo(prefabFactory, baseLogistica.transform, new Vector3(0, 0, 20), Quaternion.Euler(0, 180, 0));
        hq.name = "Armazem_HQ";

        // Caminhão Profissional na Doca
        GameObject truckTeam = InstanciarModelo(prefabTruck, baseLogistica.transform, new Vector3(0, 0, 5), Quaternion.Euler(0, 180, 0));
        truckTeam.name = "CaminhaoLogistico";
        
        // Exaustor no novo caminhão
        GameObject exhaust = new GameObject("ExhaustPipe");
        exhaust.transform.SetParent(truckTeam.transform);
        exhaust.transform.localPosition = new Vector3(-1f, 3f, 0f); // Teto do modelo do caminhão
        exhaust.transform.localRotation = Quaternion.Euler(-90, 0, 0); 
        ParticleSystem ps = exhaust.AddComponent<ParticleSystem>();
        var main = ps.main; main.startColor = new Color(0.2f, 0.2f, 0.2f, 0.8f); main.startSize = 1.5f; main.startLifetime = 1.5f; main.startSpeed = 4f;
        var em = ps.emission; em.rateOverTime = 12f;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 8f; shape.radius = 0.2f;

        truckTeam.AddComponent<TruckController>();

        // ==========================================
        // PALCO 2: RODOVIA DO DESLIZAMENTO (x: 200)
        // ==========================================
        GameObject estradaMundo = new GameObject("RodoviaMundo_X200");
        estradaMundo.transform.position = new Vector3(200, 0, 0);

        if (prefabRoad != null) {
            for (int r = -5; r <= 5; r++) {
                InstanciarModelo(prefabRoad, estradaMundo.transform, new Vector3((r * 10f) + 200f, 0, 0), Quaternion.Euler(-90, 90, 0));
            }
        }

        // O Obstáculo Perigoso (Deslizamento de Pedras Low Poly Realistas)
        if (prefabRockBig != null) {
            GameObject deslizamento = new GameObject("BarricadaPedras");
            deslizamento.transform.SetParent(estradaMundo.transform);
            deslizamento.transform.localPosition = new Vector3(8f, 0, 0f); 
            
            for(int p = 0; p < 6; p++) {
                Vector3 rockPos = deslizamento.transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                GameObject rock = InstanciarModelo(prefabRockBig, deslizamento.transform, rockPos, Random.rotation);
                rock.transform.localScale = new Vector3(Random.Range(2f, 3.5f), Random.Range(1.5f, 3f), Random.Range(2f, 3.5f));
                rock.AddComponent<BoxCollider>(); // Provê corpo físico pra IA frear
            }
        }
        if (prefabCone != null) {
            GameObject c1 = InstanciarModelo(prefabCone, estradaMundo.transform, new Vector3(203f, 0, 4f), Quaternion.identity);
            GameObject c2 = InstanciarModelo(prefabCone, estradaMundo.transform, new Vector3(203f, 0, -4f), Quaternion.identity);
            c1.AddComponent<BoxCollider>();
            c2.AddComponent<BoxCollider>();
        }

        // Atrelando Inteligência Artificial no Palco 2!
        if (prefabAmbulance != null) {
            TrafficManager tm = estradaMundo.AddComponent<TrafficManager>();
            tm.carPrefabs = new GameObject[] { prefabAmbulance, prefabCar1, prefabBus };
        }

        // ==========================================
        // AMBIENTAÇÃO E CLIMA (LUZ DO SOL)
        // ==========================================
        GameObject sol = GameObject.Find("Directional Light");
        if (sol != null && sol.GetComponent<DayNightAnimator>() == null) {
            sol.AddComponent<DayNightAnimator>();
        }

        // ==========================================
        // PALCO 3: OFICINA DO JOGADOR
        // ==========================================
        GameObject oficinaMundo = new GameObject("OficinaMundo_Xminus200");
        oficinaMundo.transform.position = new Vector3(-200, 0, 0);

        // A Oficina Tycoon Premium
        if (prefabAutoService != null) {
            GameObject oficina = InstanciarModelo(prefabAutoService, oficinaMundo.transform, new Vector3(-200, 0, 0), Quaternion.Euler(0, 180, 0));
            oficina.name = "Auto_Service_HQ";
            oficina.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Dá mais presença à oficina
        }

        Debug.Log("SISTEMA DE ASSETS ATIVADO! Cidade montada com sucesso usando Prefabs de Alta Fidelidade Tycoon.");
    }

    private static GameObject InstanciarModelo(GameObject prefab, Transform pai, Vector3 posicaoMundo, Quaternion rotacao) {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.transform.position = posicaoMundo;
        obj.transform.rotation = rotacao;
        if (pai != null) obj.transform.SetParent(pai);
        return obj;
    }
}
