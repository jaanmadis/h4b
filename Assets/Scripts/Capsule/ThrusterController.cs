using System.Collections;
using TMPro;
using UnityEngine;

public class ThrusterController : MonoBehaviour
{
    [SerializeField] AudioSource exhaustSound;
    [SerializeField] AudioSource readySound;
    [SerializeField] AudioSource standbySound;
    [SerializeField] AudioSource offlineSound;
    [SerializeField] ParticleSystem exhaust;
    [SerializeField] TextMeshProUGUI text;

    public bool IsFiring => exhaust.emission.enabled;
    public bool IsFlagged { get; set; } = false;

    private bool isReady = false;
    
    private const float BURN_FORCE = 200f;
    private const float BURN_RATE_OVER_TIME = 115f;
    private const float HALT_DELAY = 1f;
    private const float NORMAL_RATE_OVER_TIME = 15f;
    private const float TURN_FORCE = 100f;

    private void Start()
    {
        if (exhaust)
        {
            var emission = exhaust.emission;
            emission.enabled = false;
            exhaust.Stop();
        }
        Offline();
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
        if (isReady && !IsFiring)
        {
            exhaustSound.Play();
            exhaust.Play();
            var emission = exhaust.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(burn ? BURN_RATE_OVER_TIME: NORMAL_RATE_OVER_TIME);
            text.text = "Active";
            text.color = Color.green;
        }
    }

    public void Offline()
    {
        text.text = "Offline";
        text.color = Color.gray;
        isReady = false;
    }

    public void Ready()
    {
        text.text = "Ready";
        text.color = Color.cyan;
        isReady = true;
    }

    public void Standby()
    {
        Stop();
        text.text = "Standby";
        text.color = Color.yellow;
        isReady = false;
    }

    public void Stop()
    {
        IsFlagged = false;
        if (IsFiring)
        {
            exhaustSound.Stop();
            var emission = exhaust.emission;
            emission.enabled = false;
            Ready();
        }
    }

    private float GetForce(bool burn)
    { 
        return burn ? BURN_FORCE : TURN_FORCE;
    }
}