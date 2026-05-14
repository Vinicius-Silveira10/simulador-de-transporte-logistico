using UnityEngine;

public class EliteCarAI : MonoBehaviour
{
    public float speed = 15f;
    public float stopDistance = 8f;
    private float originalSpeed;
    
    void Start()
    {
        // Define velocidade por tipo de veículo se possível, senão randômico
        if (name.Contains("Ambulance") || name.Contains("Police")) {
            speed = 25f;
        } else if (name.Contains("Bus")) {
            speed = 10f;
        } else {
            speed = 15f + Random.Range(-3f, 3f);
        }
        originalSpeed = speed;

        // Auto-Escala Elite: Garante que os modelos importados não fiquem gigantes
        transform.localScale = Vector3.one * 1.5f;
    }

    void Update()
    {
        bool obstacle = false;
        Vector3 sensorPos = transform.position + Vector3.up * 1f;

        RaycastHit hit;
        if (Physics.Raycast(sensorPos, transform.forward, out hit, stopDistance)) {
            obstacle = true;
        }

        if (!obstacle) {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        // Destruição por distância (Limpeza de VRAM)
        if (transform.position.magnitude > 400f) {
            Destroy(gameObject);
        }
    }
}
