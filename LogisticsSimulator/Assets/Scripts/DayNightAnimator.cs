using UnityEngine;

public class DayNightAnimator : MonoBehaviour
{
    private float rotationSpeed = 3f; // Graus que o sol passa por segundo
    
    // Opcional: Variar a cor do céu da câmera com o passar das horas
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Gira a iluminação central dando efeito de passagem de semanas (Sombras Dinâmicas!)
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

        // Abaixo da linha do Equador (- rotationX entre 0 e 180 é cima)
        float angleX = transform.eulerAngles.x;

        if (cam != null)
        {
            if (angleX < 180f)
            {
                // De dia (Azul Claro Vivo)
                cam.backgroundColor = Color.Lerp(cam.backgroundColor, new Color(0.4f, 0.6f, 0.8f), Time.deltaTime);
                GetComponent<Light>().intensity = Mathf.Lerp(GetComponent<Light>().intensity, 1.2f, Time.deltaTime);
            }
            else
            {
                // De noite (Ceu Estrelado escuro)
                cam.backgroundColor = Color.Lerp(cam.backgroundColor, new Color(0.05f, 0.05f, 0.15f), Time.deltaTime);
                GetComponent<Light>().intensity = Mathf.Lerp(GetComponent<Light>().intensity, 0.1f, Time.deltaTime);
            }
        }
    }
}
