using UnityEngine;

public class EliteCameraAdjuster : MonoBehaviour
{
    void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Visão Frontal Elite: Foco na Fábrica e na Natureza
            mainCam.transform.position = new Vector3(0, 15, -45);
            mainCam.transform.rotation = Quaternion.Euler(15, 0, 0); 
            Debug.Log("Câmera Elite: Visão Frontal Ativada.");
        }
    }
}
