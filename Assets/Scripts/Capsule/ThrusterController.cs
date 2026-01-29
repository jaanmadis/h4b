using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class ThrusterController : MonoBehaviour
{
    [SerializeField] ParticleSystem exhaust;
    [SerializeField] TextMeshProUGUI text;

    public bool IsFiring => exhaust.emission.enabled;

    private const float BURN_FORCE = 25f;
    private const float NORMAL_FORCE = 10f;
    private const float BURN_SUSTAIN = 20f;
    private const float NORMAL_SUSTAIN = 7f;
    private const float BURN_RATE_OVER_TIME = 115f;
    private const float NORMAL_RATE_OVER_TIME = 15f;
    private bool isReady = false;

    private void Start()
    {
        if (exhaust)
        {
            var emission = exhaust.emission;
            emission.enabled = false;
            exhaust.Stop();
        }
        text.text = "Offline";
        text.color = Color.gray;
        isReady = false;
    }

    public void Fire(Rigidbody rb, bool burn)
    {
        if (isReady)
        {
            rb.AddForceAtPosition(
                -1 * GetForce(burn) * transform.forward,
                transform.position,
                ForceMode.Force
            );
            FireCosmetic(burn);
        }
    }

    public void FireCosmetic(bool burn)
    {
        if (exhaust && !exhaust.emission.enabled && isReady)
        {
            exhaust.Play();
            var emission = exhaust.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(burn ? BURN_RATE_OVER_TIME: NORMAL_RATE_OVER_TIME);
            text.text = "Active";
            text.color = Color.green;
        }
    }

    public void Ready()
    {
        text.text = "Ready";
        text.color = Color.cyan;
        isReady = true;
    }

    public void Standby()
    {
        text.text = "Standby";
        text.color = Color.yellow;
        isReady = false;
    }

    public void Stop()
    {
        if (exhaust && exhaust.emission.enabled)
        {
            var emission = exhaust.emission;
            emission.enabled = false;
            Ready();
        }
    }

    public void Sustain(Rigidbody rb, bool burn)
    {
        if (isReady)
        {
            rb.AddForceAtPosition(
                -1 * GetSustain(burn) * transform.forward,
                transform.position,
                ForceMode.Force
            );
            FireCosmetic(burn);
        }
    }

    private float GetForce(bool burn)
    { 
        return burn ? BURN_FORCE : NORMAL_FORCE;
    }

    private float GetSustain(bool burn)
    {
        return burn ? BURN_SUSTAIN : NORMAL_SUSTAIN;
    }
}