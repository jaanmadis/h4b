using UnityEngine;

public class SunController : MonoBehaviour
{
    [SerializeField] private Light sunLight;
    private readonly float DISTANCE_FROM_CAMERA = 1000f;

    void LateUpdate()
    {
        transform.forward = -sunLight.transform.forward;
        transform.position = Camera.main.transform.position + transform.forward * DISTANCE_FROM_CAMERA;
    }
}
