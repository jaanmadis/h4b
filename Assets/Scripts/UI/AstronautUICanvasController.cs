using System.Collections;
using UnityEngine;

public class AstronautUICanvasController : MonoBehaviour
{
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private CanvasGroup buttonPanelCanvasGroup;
    [SerializeField] private GameObject objectivesPanel;

    private bool objectivesAvailable = false;

    private const float FADE_IN_DELAY = 3f;
    private const float FADE_IN_DURATION = 3f;

    public float pickupRange = 3f;
    public Transform holdPoint;
    private Rigidbody heldObject;

    private void Start()
    {
        HideObjectives();
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (objectivesAvailable && Input.GetKeyDown(KeyCode.Tab))
        {
            ShowObjectives();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            HideObjectives();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickup();
            else
                Drop();
        }
    }

    private IEnumerator FadeIn()
    {
        buttonPanelCanvasGroup.alpha = 0f;
        yield return new WaitForSeconds(FADE_IN_DELAY);

        objectivesAvailable = true;

        float t = 0f;
        while (t < FADE_IN_DURATION)
        {
            t += Time.deltaTime;
            buttonPanelCanvasGroup.alpha = t / FADE_IN_DURATION;
            yield return null;
        }
        buttonPanelCanvasGroup.alpha = 1f;
    }

    private void HideObjectives()
    {
        buttonPanel.SetActive(true);
        objectivesPanel.SetActive(false);
    }
    private void ShowObjectives()
    {
        buttonPanel.SetActive(false);
        objectivesPanel.SetActive(true);
    }

    void TryPickup()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb == null) return;

            heldObject = rb;

            rb.transform.position = new Vector3(rb.transform.position.x, rb.transform.position.y + 1f, rb.transform.position.z);

            //rb.isKinematic = true;
            //rb.transform.SetParent(holdPoint);
            //rb.transform.localPosition = Vector3.zero;
            //rb.transform.localRotation = Quaternion.identity;
        }
    }

    void Drop()
    {
        heldObject.transform.SetParent(null);
        heldObject.isKinematic = false;
        heldObject = null;
    }
}
