using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField] Camera landerCamera;
    [SerializeField] Camera playerCamera;

    void Start()
    {
        landerCamera.tag = "MainCamera";
        playerCamera.tag = "Untagged";
    }
}
