using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanderCameraCanvasController : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image asteroidIndicator;
    [SerializeField] CapsuleLandingController capsuleLandingController;
    [SerializeField] RectTransform canvasRect;
    [SerializeField] TextMeshProUGUI blinkingText;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] TextMeshProUGUI distanceLabelText;
    [SerializeField] TextMeshProUGUI distanceText;
    [SerializeField] TextMeshProUGUI velocityLabelText;
    [SerializeField] TextMeshProUGUI velocityText;
    [SerializeField] TextMeshProUGUI Engine1LabelText;
    [SerializeField] TextMeshProUGUI Engine1Text;
    [SerializeField] TextMeshProUGUI Engine2LabelText;
    [SerializeField] TextMeshProUGUI Engine2Text;
    [SerializeField] TextMeshProUGUI Engine3LabelText;
    [SerializeField] TextMeshProUGUI Engine3Text;
    [SerializeField] TextMeshProUGUI Engine4LabelText;
    [SerializeField] TextMeshProUGUI Engine4Text;

    private RectTransform AsteroidIndicatorRect => asteroidIndicator.rectTransform;

    private Coroutine blinker;

    private const float ASTEROID_TARGET_MARGIN = 30f;
    private const float FADE_IN_DURATION = 3f;
    private readonly Color asteroidIndicatorDefaultColor = new(1f, 0.5f, 0f, 1f);
    private readonly Color asteroidIndicatorTargetColor = new(1f, 1f, 1f, 1f);

    private void Start()
    {
        // TODO: Change text labels if wsad keys are re-mappable
        asteroidIndicator.enabled = false;
        blinkingText.enabled = false;
        buttonText.enabled = false;

        //distanceLabelText.enabled = false;
        //distanceText.enabled = false;
        //velocityLabelText.enabled = false;
        //velocityText.enabled = false;

        //distanceLabelText.enabled = true;
        //distanceText.enabled = true;
        //velocityLabelText.enabled = true;
        //velocityText.enabled = true;

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (asteroidIndicator.enabled)
        {
            DrawAsteroidIndicator();
        }
        if (distanceText.enabled)
        {
            distanceText.text = $"{capsuleLandingController.GetDistanceToAsteroid():F3}";
        }
        if (velocityText.enabled)
        {
            velocityText.text = $"{capsuleLandingController.CapsuleRigidbody.velocity.magnitude:F3}";
        }
    }

    public void SetCapsuleLandingState(CapsuleLandingState newCapsuleLandingState)
    {
        switch (newCapsuleLandingState)
        {
            case CapsuleLandingState.Coasting:
                asteroidIndicator.color = asteroidIndicatorDefaultColor;
                asteroidIndicator.enabled = true;
                blinkingText.color = Color.yellow;
                blinkingText.text = "ATTITUDE CONTROL REQUIRED";
                blinker = StartCoroutine(Blink(blinkingText));
                buttonText.text = "[CTRL]";
                break;
            case CapsuleLandingState.Stabilizing:
                asteroidIndicator.enabled = false;
                StopCoroutine(blinker);
                blinkingText.color = Color.green;
                blinkingText.enabled = true;
                blinkingText.text = "ATTITUDE CONTROL ACTIVE";
                buttonText.enabled = false;
                break;
            case CapsuleLandingState.Stabilized:
                blinkingText.enabled = false;
                break;
            case CapsuleLandingState.PreBurn:
                blinkingText.color = Color.yellow;
                blinkingText.text = "INITIATE RETROGRADE BURN";
                blinker = StartCoroutine(Blink(blinkingText));
                buttonText.text = "[SPACE]";
                buttonText.enabled = true;
                Engine1LabelText.enabled = false;
                Engine1Text.enabled = false;
                Engine2LabelText.text = "All Engines [SPACE]";
                Engine2Text.enabled = true;
                Engine3LabelText.enabled = false;
                Engine3Text.enabled = false;
                Engine4LabelText.enabled = false;
                Engine4Text.enabled = false;
                break;
            case CapsuleLandingState.RetrogradeBurn:
                StopCoroutine(blinker);
                blinkingText.enabled = false;
                buttonText.enabled = false;
                break;
        }
    }

    IEnumerator Blink(TextMeshProUGUI text)
    {
        while (true)
        {
            text.enabled = true;
            yield return new WaitForSeconds(1.5f);
            
            text.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(Constants.PRE_COASTING_DELAY);

        canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < FADE_IN_DURATION)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / FADE_IN_DURATION;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void DrawAsteroidIndicator()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(Constants.ASTEROID_CENTER);
        screenPos.z = Mathf.Sign(screenPos.z);

        if (screenPos.z <= 0f)
        {
            AsteroidIndicatorRect.gameObject.SetActive(false);
            return;
        }

        AsteroidIndicatorRect.gameObject.SetActive(true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 canvasPos
        );

        canvasPos.x = Mathf.Clamp(canvasPos.x, -canvasRect.sizeDelta.x / 2, canvasRect.sizeDelta.x / 2);
        canvasPos.y = Mathf.Clamp(canvasPos.y, -canvasRect.sizeDelta.y / 2, canvasRect.sizeDelta.y / 2);

        AsteroidIndicatorRect.anchoredPosition = canvasPos;

        bool targetAquired = 
            Mathf.Abs(AsteroidIndicatorRect.anchoredPosition.x) < ASTEROID_TARGET_MARGIN && 
            Mathf.Abs(AsteroidIndicatorRect.anchoredPosition.y) < ASTEROID_TARGET_MARGIN;

        if (targetAquired)
        {
            asteroidIndicator.color = asteroidIndicatorTargetColor;
            buttonText.enabled = true;
            capsuleLandingController.CanStabilize = true;
        }
        else
        {
            asteroidIndicator.color = asteroidIndicatorDefaultColor;
            buttonText.enabled = false;
            capsuleLandingController.CanStabilize = false;
        }
    }
}