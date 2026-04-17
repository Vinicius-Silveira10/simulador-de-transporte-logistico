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
        StartCoroutine(TrafficRoutine());
    }

    IEnumerator TrafficRoutine()
    {
        // Dá um tempinho inicial antes de congestionar as vias
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (carPrefabs != null && carPrefabs.Length > 0)
            {
                // Roleta de carros
                int randomCarIndex = Random.Range(0, carPrefabs.Length);
                GameObject selectedCarPrefab = carPrefabs[randomCarIndex];
                
                if (selectedCarPrefab != null) 
                {
                    // Usa Instantiate normal de Runtime PBR
                    GameObject activeCar = Instantiate(selectedCarPrefab, startSpawnPosition, spawnRotation);
                    
                    // Injeta a Inteligência Artificial dinamicamente!
                    activeCar.AddComponent<SimpleCarAI>();
                    
                    // Injeta um BoxCollider para o Raycast saber que o carro tem "Massa Física"
                    if (activeCar.GetComponent<Collider>() == null) {
                        activeCar.AddComponent<BoxCollider>();
                    }
                }
            }
            
            // Ritmo orgânico de engarrafamento (ex: de 2.5 a 4.5 segundos)
            yield return new WaitForSeconds(spawnInterval + Random.Range(-1f, 1f));
        }
    }
}
