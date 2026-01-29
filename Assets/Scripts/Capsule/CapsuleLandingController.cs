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

    public bool CanStabilize { get; set; }
    public Rigidbody CapsuleRigidbody => capsuleRigidbody;

    private CapsuleLandingState capsuleLandingState = CapsuleLandingState.Initial;

    private const float COASTING_SPEED = 1000f;
    private const float MAX_ANGULAR_VELOCITY = 0.3f;
    private const float RETROGRADE_BURN_DISTANCE = 30000f;
    private const float STABILIZE_DURATION = 5f;
    private const float THRUSTER_ONLINE_DELAY = 2f;

    void Start()
    {
        StartCoroutine(StartPreCoasting());
    }

    void Update()
    {
        switch (capsuleLandingState)
        {
            case CapsuleLandingState.PreCoasting:
                StartCoroutine(StartCoasting());
                break;
            case CapsuleLandingState.Coasting:
                // TODO: Make these re-mappable and add arrow key support
                HandleThruster(KeyCode.W, new ThrusterController[] { thruster4 }, false);
                HandleThruster(KeyCode.S, new ThrusterController[] { thruster2 }, false);
                HandleThruster(KeyCode.A, new ThrusterController[] { thruster1 }, false);
                HandleThruster(KeyCode.D, new ThrusterController[] { thruster3 }, false);
                if (Input.GetKeyDown(KeyCode.LeftControl) && CanStabilize)
                {
                    SetCapsuleLandingState(CapsuleLandingState.Stabilizing);
                    StartCoroutine(Stabilize());
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
                    SetCapsuleLandingState(CapsuleLandingState.RetrogradeBurn);
                }
                break;
            case CapsuleLandingState.RetrogradeBurn:
                HandleThruster(KeyCode.Space, new ThrusterController[] { thruster1, thruster2, thruster3, thruster4 }, true);
                break;
        }
        capsuleRigidbody.angularVelocity = Vector3.ClampMagnitude(capsuleRigidbody.angularVelocity, MAX_ANGULAR_VELOCITY);
    }

    void FixedUpdate()
    {
        if (capsuleLandingState == CapsuleLandingState.RetrogradeBurn)
        {
            // Start adding gravity force towards asteroid center
            Vector3 up = (transform.position - Constants.ASTEROID_CENTER).normalized;
            Vector3 gravityDir = -up;
            capsuleRigidbody.AddForce(gravityDir * Constants.GRAVITY_STRENGTH, ForceMode.Acceleration);
        }
    }

    public float GetDistanceToAsteroid()
    {
        float approxRadius = 200f;
        float approxDistance = (Constants.ASTEROID_CENTER - transform.position).magnitude - approxRadius;
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

    private void HandleThruster(KeyCode key, ThrusterController[] thrusters, bool burn)
    {
        if (Input.GetKeyDown(key))
        {
            foreach (var thruster in thrusters)
            {
                thruster.Fire(capsuleRigidbody, burn);
            }
        }
        if (Input.GetKey(key))
        {
            foreach (var thruster in thrusters)
            { 
                thruster.Sustain(capsuleRigidbody, burn);
            }
        }
        if (Input.GetKeyUp(key))
        {
            foreach (var thruster in thrusters)
            {
                thruster.Stop();
            }
        }
    }

    private IEnumerator StartPreCoasting()
    {
        yield return new WaitForSeconds(Constants.PRE_COASTING_DELAY);
        SetCapsuleLandingState(CapsuleLandingState.PreCoasting);
    }

    private IEnumerator StartCoasting()
    {
        Vector3 up = (transform.position - Constants.ASTEROID_CENTER).normalized;
        Vector3 asteroidDir = -up;
        capsuleRigidbody.AddForce(asteroidDir * COASTING_SPEED, ForceMode.VelocityChange);
        SetCapsuleLandingState(CapsuleLandingState.Coasting);

        yield return new WaitForSeconds(THRUSTER_ONLINE_DELAY);
        thruster3.Ready();

        yield return new WaitForSeconds(THRUSTER_ONLINE_DELAY);
        thruster1.Ready();

        yield return new WaitForSeconds(THRUSTER_ONLINE_DELAY);
        thruster4.Ready();

        yield return new WaitForSeconds(THRUSTER_ONLINE_DELAY);
        thruster2.Ready();
    }

    private IEnumerator Stabilize()
    {
        bool cosmeticDone = false;
        bool physicsDone = false;

        StartCoroutine(StabilizeWrapper(StabilizeCosmetic(), () => cosmeticDone = true));
        StartCoroutine(StabilizeWrapper(StabilizePhysics(), () => physicsDone = true));

        yield return new WaitUntil(() => cosmeticDone && physicsDone);

        SetCapsuleLandingState(CapsuleLandingState.Stabilized);
    }

    private IEnumerator StabilizeWrapper(IEnumerator coroutine, System.Action onComplete)
    {
        yield return StartCoroutine(coroutine);
        onComplete?.Invoke();
    }

    private IEnumerator StabilizeCosmetic()
    {
        ThrusterController[] thrusters = { thruster1, thruster2, thruster3, thruster4 };
        
        float elapsed = 0f;
        while (elapsed < STABILIZE_DURATION)
        {
            int rng = Random.Range(0, thrusters.Length);
            ThrusterController thruster = thrusters[rng];
            if (thruster.IsFiring)
            {
                thruster.Stop();
            }
            else
            {
                thruster.FireCosmetic(false);
            }
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var thruster in thrusters)
        {
            thruster.Stop();
            thruster.Standby();
        }
    }

    private IEnumerator StabilizePhysics()
    {
        // Set velocity toward asteroid center
        Vector3 direction = -(transform.position - Constants.ASTEROID_CENTER).normalized;
        capsuleRigidbody.velocity = direction * capsuleRigidbody.velocity.magnitude;

        // Set initial values
        Vector3 initialAngularVelocity = capsuleRigidbody.angularVelocity;
        Quaternion initialRotation = transform.rotation;

        float elapsed = 0f;
        while (elapsed < STABILIZE_DURATION)
        {
            float t = elapsed / STABILIZE_DURATION;

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
}