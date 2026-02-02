using UnityEngine;

public class SkyController : MonoBehaviour
{
    [SerializeField] private Transform sun;
    [SerializeField] private Transform sunLight;

    private const float EXPOSURE_SMOOTH = 5f;
    private const float MAX_EXPOSURE = 0.6f;
    private const float MIN_EXPOSURE = 0.1f;
    private const float ROTATION_SPEED = 1f;

    private float currentExposure = MAX_EXPOSURE;
    private float angle = 0f;

    void Update()
    {
        // rotate sun
        transform.Rotate(Vector3.up * ROTATION_SPEED * -1 * Time.deltaTime, Space.World);

        // rotate skybox
        angle += ROTATION_SPEED * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", angle);

        float targetExposure = MAX_EXPOSURE;

        //If sun in not occluded by asteroid, then adjust exposure based on angle to sun
        Vector3 toSunDir = -sun.transform.forward;
        Vector3 rayOrigin = Camera.main.transform.position + toSunDir * 0.5f;
        if (!Physics.Raycast(rayOrigin, toSunDir, Mathf.Infinity))
        {
            Vector3 sunDir = -sunLight.transform.forward;

            // dot =  1 looking at sun
            // dot =  0 perpendicular
            // dot = -1 sun behind
            float dot = Vector3.Dot(Camera.main.transform.forward, sunDir);

            float t = Mathf.InverseLerp(-0.5f, 0.8f, dot);
            targetExposure = Mathf.Lerp(MAX_EXPOSURE, MIN_EXPOSURE, t);
        }

        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * EXPOSURE_SMOOTH);
        RenderSettings.skybox.SetFloat("_Exposure", currentExposure);
    }
}