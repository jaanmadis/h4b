using System.Collections;
using UnityEngine;

public enum CapsuleLandingState
{
    Initial,
    PreCoasting,
    Coasting,
    Stabilizing,
    Stabilized,
    PreBurn,
    RetrogradeBurn,
    Landing,
    Landed
}

public class CapsuleLandingController : MonoBehaviour
{
    [SerializeField] GameObject asteroid;
    [SerializeField] LanderCameraCanvasController landerCameraCanvasController;
    [SerializeField] Rigidbody capsuleRigidbody;
    [SerializeField] ThrusterController thruster1;
    [SerializeField] ThrusterController thruster2;
    [SerializeField] ThrusterController thruster3;
    [SerializeField] ThrusterController thruster4;

    public bool CanLand { get; set; }
    public bool CanStabilize { get; set; }
    public CapsuleLandingState CapsuleLandingState => capsuleLandingState;

    public Rigidbody CapsuleRigidbody => capsuleRigidbody;

    private CapsuleLandingState capsuleLandingState = CapsuleLandingState.Initial;

    private const float AUTOPILOT_DURATION = 5f;
    private const float COASTING_SPEED = 1900f;
    private const float MAX_ANGULAR_VELOCITY = 0.3f;
    private const float RETROGRADE_BURN_DISTANCE = 40000f;
    private const float THRUSTER_OFFLINE_DELAY = 2f;
    private const float THRUSTER_READY_DELAY = 2f;
    private const float THRUSTER_MIN_DISTANCE = 10f;

    void Start()
    {
        StartCoroutine(StartPreCoasting());
    }

    void Update()
    {
        switch (capsuleLandingState)
        {
            case CapsuleLandingState.Coasting:
                // TODO: Make these re-mappable and add arrow key support
                HandleThrusterFlag(KeyCode.W, thruster4);
                HandleThrusterFlag(KeyCode.S, thruster2);
                HandleThrusterFlag(KeyCode.A, thruster1);
                HandleThrusterFlag(KeyCode.D, thruster3);
                if (Input.GetKeyDown(KeyCode.LeftControl) && CanStabilize)
                {
                    SetCapsuleLandingState(CapsuleLandingState.Stabilizing);
                    thruster1.Stop();
                    thruster2.Stop();
                    thruster3.Stop();
                    thruster4.Stop();
                    StartCoroutine(AutoStabilize());
                }
                break;
            case CapsuleLandingState.Stabilized:
                if (GetDistanceToAsteroid() < RETROGRADE_BURN_DISTANCE)
                {
                    thruster1.Ready();
                    thruster2.Ready();
                    thruster3.Ready();
                    thruster4.Ready();
                    SetCapsuleLandingState(CapsuleLandingState.PreBurn);
                }
                break;
            case CapsuleLandingState.PreBurn:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    HandleThrusterFlag(KeyCode.Space, thruster1);
                    HandleThrusterFlag(KeyCode.Space, thruster2);
                    HandleThrusterFlag(KeyCode.Space, thruster3);
                    HandleThrusterFlag(KeyCode.Space, thruster4);
                    SetCapsuleLandingState(CapsuleLandingState.RetrogradeBurn);
                }
                break;
            case CapsuleLandingState.RetrogradeBurn:
                HandleThrusterFlag(KeyCode.Space, thruster1);
                HandleThrusterFlag(KeyCode.Space, thruster2);
                HandleThrusterFlag(KeyCode.Space, thruster3);
                HandleThrusterFlag(KeyCode.Space, thruster4);
                if (Input.GetKeyDown(KeyCode.LeftControl) && CanLand)
                {
                    SetCapsuleLandingState(CapsuleLandingState.Landing);
                    thruster1.Stop();
                    thruster2.Stop();
                    thruster3.Stop();
                    thruster4.Stop();
                    StartCoroutine(AutoLand());
                }
                break;
        }
    }

    void FixedUpdate()
    {
        switch (capsuleLandingState)
        {
            case CapsuleLandingState.PreCoasting:
                Vector3 asteroidDir = (Constants.ASTEROID_CENTER_LANDING - transform.position).normalized;
                capsuleRigidbody.AddForce(asteroidDir * COASTING_SPEED, ForceMode.VelocityChange);
                SetCapsuleLandingState(CapsuleLandingState.Coasting);
                break;
            case CapsuleLandingState.Coasting:
                HandleThrusterFire(thruster1, false);
                HandleThrusterFire(thruster2, false);
                HandleThrusterFire(thruster3, false);
                HandleThrusterFire(thruster4, false);
                capsuleRigidbody.angularVelocity = Vector3.ClampMagnitude(capsuleRigidbody.angularVelocity, MAX_ANGULAR_VELOCITY);
                break;
            case CapsuleLandingState.RetrogradeBurn:
                Vector3 gravityDir = (Constants.ASTEROID_CENTER_LANDING - transform.position).normalized;
                capsuleRigidbody.AddForce(gravityDir * Constants.GRAVITY_STRENGTH_LANDING, ForceMode.Acceleration);
                float dot = Vector3.Dot(capsuleRigidbody.velocity, gravityDir);
                if (dot < 1)
                {
                    thruster1.Stop();
                    thruster2.Stop();
                    thruster3.Stop();
                    thruster4.Stop();
                }
                HandleThrusterFire(thruster1, true);
                HandleThrusterFire(thruster2, true);
                HandleThrusterFire(thruster3, true);
                HandleThrusterFire(thruster4, true);
                break;
        }
    }

    public float GetDistanceToAsteroid()
    {
        float approxRadius = 200f;
        float approxDistance = (Constants.ASTEROID_CENTER_LANDING - transform.position).magnitude - approxRadius;
        if (approxDistance < 20000f)
        {
            Vector3 dir = (asteroid.transform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, Mathf.Infinity))
            {
                float distanceToSurface = hit.distance;
                return distanceToSurface;
            }
        }
        return approxDistance;
    }

    private void HandleThrusterFire(ThrusterController thruster, bool burn)
    {
        if (thruster.IsFlagged)
        {
            thruster.Fire(capsuleRigidbody, burn);
        }
        else
        {
            thruster.Stop();
        }
    }

    private void HandleThrusterFlag(KeyCode key, ThrusterController thruster)
    {
        if (Input.GetKeyDown(key))
        {
            thruster.IsFlagged = true;
        }
        if (Input.GetKeyUp(key))
        {
            thruster.IsFlagged = false;
        }
    }

    private IEnumerator StartCoroutineWithCallback(IEnumerator coroutine, System.Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }

    private IEnumerator StartPreCoasting()
    {
        yield return new WaitForSeconds(Constants.PRE_COASTING_DELAY);
        SetCapsuleLandingState(CapsuleLandingState.PreCoasting);
        StartCoroutine(ThrustersReady());
    }

    private IEnumerator AutoLand()
    {
        bool cosmeticsDone = false;
        bool physicsDone = false;

        StartCoroutine(StartCoroutineWithCallback(AutopilotCosmetics(), () => cosmeticsDone = true));
        StartCoroutine(StartCoroutineWithCallback(LandPhysics(), () => physicsDone = true));

        yield return new WaitUntil(() => cosmeticsDone && physicsDone);

        StartCoroutine(ThrustersOffline());

        SetCapsuleLandingState(CapsuleLandingState.Landed);
    }

    private IEnumerator AutoStabilize()
    {
        bool cosmeticsDone = false;
        bool physicsDone = false;

        StartCoroutine(StartCoroutineWithCallback(AutopilotCosmetics(), () => cosmeticsDone = true));
        StartCoroutine(StartCoroutineWithCallback(StabilizePhysics(), () => physicsDone = true));

        yield return new WaitUntil(() => cosmeticsDone && physicsDone);

        SetCapsuleLandingState(CapsuleLandingState.Stabilized);
    }

    private IEnumerator AutopilotCosmetics()
    {
        ThrusterController[] thrusters = { thruster1, thruster2, thruster3, thruster4 };

        float elapsed = 0f;
        while (elapsed < AUTOPILOT_DURATION)
        {
            if (GetDistanceToAsteroid() < THRUSTER_MIN_DISTANCE)
            {
                break;
            }

            int rng = Random.Range(0, thrusters.Length);
            ThrusterController thruster = thrusters[rng];
            if (thruster.IsFiring)
            {
                thruster.Standby();
            }
            else
            {
                thruster.Ready();
                thruster.FireCosmetic(false);
            }
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var thruster in thrusters)
        {
            thruster.Standby();
        }
    }

    private IEnumerator LandPhysics()
    {
        float startDistance = GetDistanceToAsteroid();
        Vector3 direction = (Constants.ASTEROID_CENTER_LANDING - transform.position).normalized;

        capsuleRigidbody.angularVelocity = Vector3.zero;
        capsuleRigidbody.velocity = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < AUTOPILOT_DURATION)
        {
            float t = elapsed / AUTOPILOT_DURATION;

            float remainingDistance = Mathf.Lerp(startDistance, 0f, t);
            float remainingTime = Mathf.Max(AUTOPILOT_DURATION - elapsed, 0.001f);
            float targetSpeed = remainingDistance / remainingTime;

            capsuleRigidbody.velocity = direction * targetSpeed;

            elapsed += Time.deltaTime;
            yield return null;
        }

        capsuleRigidbody.angularVelocity = Vector3.zero;
        capsuleRigidbody.velocity = Vector3.zero;
    }

    private IEnumerator StabilizePhysics()
    {
        // Set velocity toward asteroid center
        Vector3 direction = (Constants.ASTEROID_CENTER_LANDING - transform.position).normalized;
        capsuleRigidbody.velocity = direction * capsuleRigidbody.velocity.magnitude;

        // Set initial values
        Vector3 initialAngularVelocity = capsuleRigidbody.angularVelocity;
        Quaternion initialRotation = transform.rotation;

        float elapsed = 0f;
        while (elapsed < AUTOPILOT_DURATION)
        {
            float t = elapsed / AUTOPILOT_DURATION;

            // Dampen angular velocity
            capsuleRigidbody.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);

            // Smooth retrograde alignment
            Vector3 velocityDir = capsuleRigidbody.velocity.normalized;
            Vector3 targetUp = -velocityDir;
            Vector3 projectedForward = Vector3.ProjectOnPlane(initialRotation * Vector3.forward, targetUp).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, targetUp);
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        capsuleRigidbody.angularVelocity = Vector3.zero;
    }

    private void SetCapsuleLandingState(CapsuleLandingState newCapsuleLandingState)
    {
        capsuleLandingState = newCapsuleLandingState;
        landerCameraCanvasController.SetCapsuleLandingState(newCapsuleLandingState);
    }

    private IEnumerator ThrustersOffline()
    {
        yield return new WaitForSeconds(THRUSTER_OFFLINE_DELAY);
        thruster3.Offline();

        yield return new WaitForSeconds(THRUSTER_OFFLINE_DELAY);
        thruster1.Offline();

        yield return new WaitForSeconds(THRUSTER_OFFLINE_DELAY);
        thruster4.Offline();

        yield return new WaitForSeconds(THRUSTER_OFFLINE_DELAY);
        thruster2.Offline();
    }

    private IEnumerator ThrustersReady()
    {
        yield return new WaitForSeconds(THRUSTER_READY_DELAY);
        thruster3.Ready();

        yield return new WaitForSeconds(THRUSTER_READY_DELAY);
        thruster1.Ready();

        yield return new WaitForSeconds(THRUSTER_READY_DELAY);
        thruster4.Ready();

        yield return new WaitForSeconds(THRUSTER_READY_DELAY);
        thruster2.Ready();
    }
}