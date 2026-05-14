using UnityEngine;
using System.Collections;

public class TrafficManager : MonoBehaviour
{
    public GameObject[] carPrefabs;
    
    [Header("Configurações do Spawner")]
    public float spawnInterval = 3.5f;
    public Vector3 startSpawnPosition = new Vector3(150f, 0.2f, 0f); // Início da rua da Rodovia
    public Quaternion spawnRotation = Quaternion.Euler(0, 90, 0); // Olhando para o Leste (X Positivo)

    void Start()
    {
        // DESATIVADO: A pedido do usuário para foco total no caminhão amarelo
        foreach (var old in FindObjectsOfType<EliteCarAI>()) Destroy(old.gameObject);
        foreach (var old in FindObjectsOfType<SimpleCarAI>()) Destroy(old.gameObject);
        
        Debug.Log("TrafficManager: Tráfego desativado para limpeza do cenário.");
    }

    IEnumerator TrafficRoutine()
    {
        yield break; // Não faz nada
    }

    void SpawnRandomVehicle()
    {
        GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        // Roleta de Pontos de Spawn (Sincronizado com o Grid de Estradas)
        int lane = Random.Range(0, 4);
        if (lane == 0) { // Rodovia Norte (Faixa Direita)
            spawnPos = new Vector3(6f, 0.2f, -300f);
            spawnRot = Quaternion.Euler(0, 0, 0);
        } else if (lane == 1) { // Rodovia Sul (Faixa Esquerda)
            spawnPos = new Vector3(-6f, 0.2f, 300f);
            spawnRot = Quaternion.Euler(0, 180, 0);
        } else if (lane == 2) { // Eixo Z Negativo (Rua Lateral)
            spawnPos = new Vector3(300f, 0.2f, 0f);
            spawnRot = Quaternion.Euler(0, -90, 0);
        } else { // Eixo Z Positivo (Rua Lateral)
            spawnPos = new Vector3(-300f, 0.2f, 0f);
            spawnRot = Quaternion.Euler(0, 90, 0);
        }

        GameObject car = Instantiate(prefab, spawnPos, spawnRot);
        car.AddComponent<EliteCarAI>();
        if (car.GetComponent<Collider>() == null) car.AddComponent<BoxCollider>();
    }
}
