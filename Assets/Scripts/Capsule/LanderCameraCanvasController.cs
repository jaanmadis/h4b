using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LanderCameraCanvasController : MonoBehaviour
{
    [SerializeField] AudioSource stabilizingAudio;
    [SerializeField] AudioSource targetAquiredAudio;
    [SerializeField] AudioSource warningAudio;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image asteroidIndicatorImage;
    [SerializeField] Image fadeImage;
    [SerializeField] CapsuleLandingController capsuleLandingController;
    [SerializeField] RectTransform canvasRect;
    [SerializeField] TextMeshProUGUI blinkingText;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] TextMeshProUGUI targetDistanceLabelText;
    [SerializeField] TextMeshProUGUI targetDistanceText;
    [SerializeField] TextMeshProUGUI targetVelocityLabelText;
    [SerializeField] TextMeshProUGUI targetVelocityText;
    [SerializeField] TextMeshProUGUI distanceLabelText;
    [SerializeField] TextMeshProUGUI distanceText;
    [SerializeField] TextMeshProUGUI velocityLabelText;
    [SerializeField] TextMeshProUGUI velocityText;
    [SerializeField] TextMeshProUGUI engine1LabelText;
    [SerializeField] TextMeshProUGUI engine1Text;
    [SerializeField] TextMeshProUGUI engine2LabelText;
    [SerializeField] TextMeshProUGUI engine2Text;
    [SerializeField] TextMeshProUGUI engine3LabelText;
    [SerializeField] TextMeshProUGUI engine3Text;
    [SerializeField] TextMeshProUGUI engine4LabelText;
    [SerializeField] TextMeshProUGUI engine4Text;

    private Coroutine blinkerCoroutine = null;
    private bool targetAquiredAudioPlayed = false;

    private const float ASTEROID_TARGET_MARGIN = 30f;
    private const float BLACKOUT_DELAY = 3f;
    private const float FADE_IN_DURATION = 3f;
    private const float FADE_OUT_DURATION = 1f;
    private const string Engine1LabelText = "Starboard Engine [A]";
    private const string Engine2LabelText = "Ventral Engine [S]";
    private const string Engine3LabelText = "Port Engine [D]";
    private const string Engine4LabelText = "Dorsal Engine [W]";
    private const string EnginesLabelText = "All Engines [SPACE]";

    // TODO: make camera more spacey -- radiation hits, sensor noise, extreme contrast etc

    private void Start()
    {
        canvasGroup.alpha = 0;
        fadeImage.enabled = false;
        asteroidIndicatorImage.enabled = false;
        blinkingText.enabled = false;
        buttonText.enabled = false;
        targetDistanceLabelText.enabled = false;
        targetDistanceText.enabled = false;
        targetDistanceText.text = $"{Constants.LANDING_TARGET_DISTANCE:F3}";
        targetVelocityLabelText.enabled = false;
        targetVelocityText.enabled = false;
        targetVelocityText.text = $"{Constants.LANDING_TARGET_VELOCITY:F3}";
        engine1LabelText.text = Engine1LabelText;
        engine2LabelText.text = Engine2LabelText;
        engine3LabelText.text = Engine3LabelText;
        engine4LabelText.text = Engine4LabelText;
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (capsuleLandingController.CapsuleLandingState == CapsuleLandingState.Coasting)
        {
            DrawAsteroidIndicator();
        }
        DrawDistanceAndVelocity();
    }

    public void SetCapsuleLandingState(CapsuleLandingState newCapsuleLandingState)
    {
        switch (newCapsuleLandingState)
        {
            case CapsuleLandingState.Coasting:
                blinkingText.color = Color.red;
                blinkingText.text = "ATTITUDE CONTROL REQUIRED";
                BlinkOn();
                buttonText.text = "[CTRL]";
                break;
            case CapsuleLandingState.Stabilizing:
                stabilizingAudio.Play();
                asteroidIndicatorImage.enabled = false;
                BlinkOff();
                blinkingText.color = Color.cyan;
                blinkingText.enabled = true;
                blinkingText.text = "ATTITUDE CONTROL ACTIVE";
                buttonText.enabled = false;
                break;
            case CapsuleLandingState.Stabilized:
                blinkingText.enabled = false;
                break;
            case CapsuleLandingState.PreBurn:
                blinkingText.color = Color.red;
                blinkingText.text = "INITIATE RETROGRADE BURN";
                BlinkOn();
                buttonText.text = "[SPACE]";
                buttonText.enabled = true;
                engine1LabelText.enabled = false;
                engine1Text.enabled = false;
                engine2LabelText.text = EnginesLabelText;
                engine2Text.enabled = true;
                engine3LabelText.enabled = false;
                engine3Text.enabled = false;
                engine4LabelText.enabled = false;
                engine4Text.enabled = false;
                targetDistanceLabelText.enabled = true;
                targetDistanceText.enabled = true;
                targetVelocityLabelText.enabled = true;
                targetVelocityText.enabled = true;
                break;
            case CapsuleLandingState.RetrogradeBurn:
                BlinkOff();
                blinkingText.color = Color.red;
                blinkingText.text = "INITIATE LANDING SEQUENCE";
                buttonText.enabled = false;
                buttonText.text = "[CTRL]";
                break;
            case CapsuleLandingState.Landing:
                BlinkOff();
                blinkingText.color = Color.cyan;
                blinkingText.enabled = true;
                blinkingText.text = "LANDING SEQUENCE ACTIVE";
                buttonText.enabled = false;
                engine1LabelText.enabled = true;
                engine1Text.enabled = true;
                engine2LabelText.text = Engine2LabelText;
                engine2Text.enabled = true;
                engine3LabelText.enabled = true;
                engine3Text.enabled = true;
                engine4LabelText.enabled = true;
                engine4Text.enabled = true;
                targetDistanceLabelText.enabled = false;
                targetDistanceText.enabled = false;
                targetVelocityLabelText.enabled = false;
                targetVelocityText.enabled = false;
                break;
            case CapsuleLandingState.Landed:
                blinkingText.enabled = false;
                distanceLabelText.enabled = false;
                distanceText.enabled = false;
                velocityLabelText.enabled = false;
                velocityText.enabled = false;
                StartCoroutine(FadeOut());
                break;
        }
    }

    private void BlinkOn()
    {
        if (blinkerCoroutine != null)
        {
            return;
        }
        blinkerCoroutine = StartCoroutine(Blink());
        warningAudio.Play();
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            blinkingText.enabled = true;
            yield return new WaitForSeconds(1.5f);

            blinkingText.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void BlinkOff()
    {
        if (blinkerCoroutine == null)
        {
            return;
        }
        warningAudio.Stop();
        StopCoroutine(blinkerCoroutine);
        blinkerCoroutine = null;
        blinkingText.enabled = false;
    }

    private IEnumerator FadeIn()
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

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(Constants.POST_LANDING_DELAY);

        canvasGroup.alpha = 1f;
        float t = FADE_OUT_DURATION;
        while (t >= 0)
        {
            t -= Time.deltaTime;
            canvasGroup.alpha = t / FADE_OUT_DURATION;
            yield return null;
        }
        canvasGroup.alpha = 0f;

        yield return new WaitForSeconds(BLACKOUT_DELAY);
        fadeImage.enabled = true;

        SceneManager.LoadScene("Asteroid");
    }

    private void DrawAsteroidIndicator()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(Constants.ASTEROID_CENTER_LANDING);
        screenPos.z = Mathf.Sign(screenPos.z);

        if (screenPos.z <= 0f)
        {
            asteroidIndicatorImage.enabled = false;
            return;
        }
        asteroidIndicatorImage.enabled = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 canvasPos
        );

        canvasPos.x = Mathf.Clamp(canvasPos.x, -canvasRect.sizeDelta.x / 2, canvasRect.sizeDelta.x / 2);
        canvasPos.y = Mathf.Clamp(canvasPos.y, -canvasRect.sizeDelta.y / 2, canvasRect.sizeDelta.y / 2);

        RectTransform indicatorRect = asteroidIndicatorImage.rectTransform;
        indicatorRect.anchoredPosition = canvasPos;

        bool targetAquired = 
            Mathf.Abs(indicatorRect.anchoredPosition.x) < ASTEROID_TARGET_MARGIN && 
            Mathf.Abs(indicatorRect.anchoredPosition.y) < ASTEROID_TARGET_MARGIN;

        if (targetAquired)
        {
            if (!targetAquiredAudioPlayed)
            {
                targetAquiredAudio.Play();
                targetAquiredAudioPlayed = true;
            }
            asteroidIndicatorImage.color = Color.cyan;
            buttonText.enabled = true;
            capsuleLandingController.CanStabilize = true;
        }
        else
        {
            targetAquiredAudioPlayed = false;
            asteroidIndicatorImage.color = Color.red;
            buttonText.enabled = false;
            capsuleLandingController.CanStabilize = false;
        }
    }

    private void DrawDistanceAndVelocity()
    {
        float distance = capsuleLandingController.GetDistanceToAsteroid();
        distanceText.text = $"{distance:F3}";

        float velocity = capsuleLandingController.CapsuleRigidbody.velocity.magnitude;
        velocityText.text = $"{velocity:F3}";
        
        if (capsuleLandingController.CapsuleLandingState == CapsuleLandingState.PreBurn || 
            capsuleLandingController.CapsuleLandingState == CapsuleLandingState.RetrogradeBurn)
        {
            bool tooFar = distance > Constants.LANDING_TARGET_DISTANCE;
            distanceText.color = tooFar ? Color.red : Color.green;

            bool tooFast = velocity > Constants.LANDING_TARGET_VELOCITY;
            velocityText.color = tooFast ? Color.red : Color.green;

            if (capsuleLandingController.CapsuleLandingState == CapsuleLandingState.RetrogradeBurn)
            {
                bool canLand = !tooFar && !tooFast;
                if (canLand)
                {
                    BlinkOn();
                    buttonText.enabled = true;
                    capsuleLandingController.CanLand = true;
                }
                else
                {
                    BlinkOff();
                    buttonText.enabled = false;
                    capsuleLandingController.CanLand = false;
                }
            }
        }
    }
}