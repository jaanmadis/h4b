using UnityEngine;

public class PlayerMouseLookController : MonoBehaviour
{
    [SerializeField] Transform mainCamera;

    private const float MOUSE_SENSITIVITY = 2f;
    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY;
        float mouseY = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY;

        // Rotate to point head/camera up or down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        mainCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate to body left or right
        transform.Rotate(Vector3.up * mouseX);
    }
}
