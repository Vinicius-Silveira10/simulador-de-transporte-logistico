using UnityEngine;

public class SimpleCarAI : MonoBehaviour
{
    public float baseSpeed = 12f;
    private float currentSpeed;
    public float stopDistance = 6f; // Distância segura
    public float sensorHeight = 0.8f; // Altura do sensor laser no parachoque

    void Start()
    {
        // Variabilidade de velocidade para parecer orgânico
        currentSpeed = baseSpeed + Random.Range(-2f, +3f);
    }

    void Update()
    {
        bool objectAhead = false;
        
        // Posição do sensor: Centro do caminhão/carro
        Vector3 sensorPos = transform.position + (Vector3.up * sensorHeight);

        // Dispara um raio para a frente
        RaycastHit hit;
        if (Physics.Raycast(sensorPos, transform.forward, out hit, stopDistance))
        {
            // Bateu em algo (Carro da frente, Pedra, Cone)
            objectAhead = true;
            Debug.DrawLine(sensorPos, hit.point, Color.red);
        }
        else 
        {
            Debug.DrawLine(sensorPos, sensorPos + (transform.forward * stopDistance), Color.green);
        }

        if (!objectAhead)
        {
            // Anda para a frente
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        }

        // Sistema de Limpeza: Se o carro dirigir para o abismo (muito longe do mapa), ele se destrói
        if (Vector3.Distance(transform.position, new Vector3(200, 0, 0)) > 300f) {
            Destroy(gameObject);
        }
    }
}
