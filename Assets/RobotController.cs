using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    private Rigidbody rb;
    private Animator animator;
    private Vector3 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrows

        // Calculate movement
        movement = new Vector3(horizontal, 0f, vertical).normalized;

        // Update animator
        bool isWalking = movement.magnitude > 0.1f;
        animator.SetBool("IsWalking", isWalking);

        float yRotation = transform.eulerAngles.y;
        // Rotate robot to face movement direction
        if (horizontal > 0)
        {
            yRotation = 90f; // Right
        }
        else if (horizontal < 0)
        {
            yRotation = -90f; // Left
        }
        else if (vertical > 0)
        {
            yRotation = 0f; // Forward
        }
        else if (vertical < 0)
        {
            yRotation = 180f; // Backward
        }
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void FixedUpdate()
    {
        // Apply movement
        Vector3 velocity = movement * moveSpeed;
        velocity.y = rb.linearVelocity.y; // Preserve gravity
        rb.linearVelocity = velocity;
    }
}