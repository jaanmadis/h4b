using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLiftoffController : MonoBehaviour
{
    [SerializeField] GameObject capsule;
    [SerializeField] GameObject sky;
    [SerializeField] Light skyLight;
    [SerializeField] Transform mainCamera;

    private const float CAPSULE_ROTATION_TIME = 80f;
    private const float CAPSULE_ROTATION_X = -20f;
    private const float CAPSULE_ROTATION_Y = -30f;
    private const float CAPSULE_ROTATION_Z = -60f;
    private const float INITIAL_SHAKE_X_VALUE = 0.2f;
    private const float INITIAL_SHAKE_Y_VALUE = 0.3f;
    private const float MIN_PITCH = -120f;
    private const float MAX_PITCH = -60f;
    private const float MIN_YAW = -30f;
    private const float MAX_YAW = 30f;
    private const float MOUSE_SENSITIVITY = 1f;
    private const float SHAKE_X_INTERVAL = 0.05f;
    private const float SHAKE_Y_INTERVAL = 0.07f;
    private const float TIME_TO_SPACE = 60f;
    private const float TIME_TO_TRANSITION = 5f;

    private float timeElapsed = 0f;
    private float shakeXTime = 0f;
    private float shakeXSign = 1f;
    private float shakeYTime = 0f;
    private float shakeYSign = 1f;
    private float pitch = -90f;
    private float yaw = 0f;

    private Quaternion capsuleStartRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        capsuleStartRotation = capsule.transform.rotation;
        StartCoroutine(PlayScene());
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        float mouseX = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY;
        float mouseY = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY;

        shakeXTime += Time.deltaTime;
        if (shakeXTime > SHAKE_X_INTERVAL)
        {
            shakeXTime = 0f;
            shakeXSign *= -1f;
        }

        shakeYTime += Time.deltaTime;
        if (shakeYTime > SHAKE_Y_INTERVAL)
        {
            shakeYTime = 0f;
            shakeYSign *= -1f;
        }

        float shakeX = Mathf.Lerp(INITIAL_SHAKE_X_VALUE, 0f, timeElapsed / TIME_TO_SPACE) * shakeXSign;
        float shakeY = Mathf.Lerp(INITIAL_SHAKE_Y_VALUE, 0f, timeElapsed / TIME_TO_SPACE) * shakeYSign;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, MIN_PITCH, MAX_PITCH);

        yaw -= mouseX;
        yaw = Mathf.Clamp(yaw, MIN_YAW, MAX_YAW);

        mainCamera.localRotation =
            Quaternion.AngleAxis(yaw + shakeX, Vector3.forward) *
            Quaternion.AngleAxis(pitch + shakeY, Vector3.right);

        Quaternion capsuleEndRotation = capsuleStartRotation * Quaternion.Euler(CAPSULE_ROTATION_X, CAPSULE_ROTATION_Y, CAPSULE_ROTATION_Z);
        capsule.transform.rotation = Quaternion.Slerp(capsuleStartRotation, capsuleEndRotation, timeElapsed / CAPSULE_ROTATION_TIME);
    }

    private IEnumerator PlayScene()
    {
        yield return StartCoroutine(Liftoff());
        yield return new WaitForSeconds(TIME_TO_TRANSITION);
        SceneManager.LoadScene("Space");
    }

    private IEnumerator Liftoff()
    {
        Color color = sky.GetComponent<Renderer>().material.color;
        float alpha = color.a;
        float intensity = skyLight.intensity;
        float t = 0f;

        while (t < TIME_TO_SPACE)
        {
            t += Time.deltaTime;
            skyLight.intensity = Mathf.Lerp(intensity, 0f, t / TIME_TO_SPACE);
            color.a = Mathf.Lerp(alpha, 0f, t / TIME_TO_SPACE);
            sky.GetComponent<Renderer>().material.color = color;
            yield return null;
        }

        skyLight.intensity = 0f;
        color.a = 0f;
        sky.GetComponent<Renderer>().material.color = color;
    }
}
