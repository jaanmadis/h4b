using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] GameObject asteroid;
    [SerializeField] Rigidbody rigidBody;
    
    //private readonly float STEP_SPEED = 100f;
    private readonly float MOVE_ACCELERATION = 20f;
    private readonly float JUMP_FORCE = 30f;
    private readonly float UPRIGHT_SPEED = 5f;

    private bool jumpRequested = false;
    private float x, z;

    void Update()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        Vector3 up = (transform.position - Constants.ASTEROID_CENTER_X).normalized;

        // Add jump force
        if (jumpRequested)
        {
            rigidBody.AddForce(up * JUMP_FORCE, ForceMode.Impulse);
            jumpRequested = false;
        }

        // Add gravity force towards asteroid center
        Vector3 gravityDir = -up;
        rigidBody.AddForce(gravityDir * Constants.GRAVITY_STRENGTH, ForceMode.Acceleration);

        // Add movement force            
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, up).normalized;
        Vector3 moveDir = (forward * z + right * x).normalized;
        rigidBody.AddForce(moveDir * MOVE_ACCELERATION, ForceMode.Acceleration);

        // Force upright orientation
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, up) * rigidBody.rotation;
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, Time.fixedDeltaTime * UPRIGHT_SPEED));
    }
}
