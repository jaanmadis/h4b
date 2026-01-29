using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI introText;
    private const float FADE_DURATION = 2f;
    private const float SHORT_HOLD_DURATION = 1f;
    private const float LONG_HOLD_DURATION = 4f;

    void Start()
    {
        introText.alpha = 0f;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        yield return ShowTextSequence(new[] { 
            ("In 2017 1I/'Oumuamua was discovered.", SHORT_HOLD_DURATION),
            ("The first interstellar object ever detected passing through our solar system.", LONG_HOLD_DURATION),
            ("It came from outside.", SHORT_HOLD_DURATION),
            ("It will never return.", SHORT_HOLD_DURATION)
        });
        yield return ShowTextSequence(new[] {
            ("Now, another object has been found.", SHORT_HOLD_DURATION),
            ("Every space agency on Earth is racing to put the first human footprint on it.", LONG_HOLD_DURATION),
            ("We are not winning.", SHORT_HOLD_DURATION),
        });
        SceneManager.LoadScene("Liftoff");
    }

    private IEnumerator ShowTextSequence((string text, float hold)[] content)
    {
        var texts = createTextComponents(content.Select(item => item.text).ToArray());

        for (int i = 0; i < content.Length; i++)
        {
            yield return FadeText(new[] { texts[i] }, FadeType.fadeIn);
            float hold = content[i].hold;
            if (hold > 0f)
            {
                yield return new WaitForSeconds(hold);
            }
        }

        yield return new WaitForSeconds(LONG_HOLD_DURATION);
        yield return FadeText(texts.ToArray(), FadeType.fadeOut);

        // Cleep template, clean up rest
        texts.Remove(introText);
        foreach (var text in texts)
        {
            Destroy(text.gameObject);
        }
    }

    private List<TextMeshProUGUI> createTextComponents(string[] content)
    {
        introText.text = content[0];
        List<TextMeshProUGUI> result = new() { introText };
        for (int i = 1; i < content.Length; i++)
        {
            var clone = Instantiate(introText.gameObject, introText.transform.parent);
            var cloneTMP = clone.GetComponent<TextMeshProUGUI>();
            cloneTMP.text = content[i];
            result.Add(cloneTMP);
        }
        return result;  
    }

    private enum FadeType
    {
        fadeIn,
        fadeOut
    }

    private IEnumerator FadeText(TextMeshProUGUI[] texts, FadeType fadeType)
    {
        float from = fadeType == FadeType.fadeIn ? 0f : 1f;
        float to = fadeType == FadeType.fadeIn ? 1f : 0f;
        for (float t = 0; t < FADE_DURATION; t += Time.deltaTime)
        {
            float a = Mathf.SmoothStep(from, to, t / FADE_DURATION);
            foreach (var text in texts)
            {
                text.alpha = a;
            }
            yield return null;
        }
        foreach (var text in texts)
        {
            text.alpha = to;
        }
    }
}
