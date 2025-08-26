using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RobotAgent : Agent
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    [Header("Training Settings")]
    public Transform floorTransform;
    public float episodeTimeout = 40f;

    [Header("References")]
    public GoalSpawner goalSpawner;

    private Rigidbody rb;
    private Animator animator;
    private Vector3 movement;
    private float yRotation;
    private float episodeTimer;
    private RayPerceptionSensorComponent3D raySensor;

    // Action mappings for DQN (discrete actions)
    // 0 = Nothing, 1 = Left, 2 = Right, 3 = Up, 4 = Down
    private readonly Vector3[] actionMap = new Vector3[]
    {
        Vector3.zero,           // 0: Nothing
        Vector3.left,           // 1: Left
        Vector3.right,          // 2: Right  
        Vector3.forward,        // 3: Up/Forward
        Vector3.back            // 4: Down/Backward
    };

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        yRotation = transform.eulerAngles.y;
        raySensor = GetComponent<RayPerceptionSensorComponent3D>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset robot position randomly on the floor
        ResetAgentPosition();

        // Reset physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Reset timer
        episodeTimer = 0f;

        goalSpawner.ResetForNewEpisode();
        goalSpawner.SpawnSpheresOnStart();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Collider floorCollider = floorTransform.GetComponent<Collider>();
        Bounds floorBounds = floorCollider.bounds;

        float positionX = (transform.position.x - floorBounds.min.x) / (floorBounds.max.x - floorBounds.min.x);
        float positionZ = (transform.position.z - floorBounds.min.z) / (floorBounds.max.z - floorBounds.min.z);
        Vector2 position = new Vector2(positionX, positionZ);
        sensor.AddObservation(position);

        float distanceTop = (floorBounds.max.z - transform.position.z) / (floorBounds.max.z - floorBounds.min.z);
        sensor.AddObservation(distanceTop);
        float distanceBottom = (transform.position.z - floorBounds.min.z) / (floorBounds.max.z - floorBounds.min.z);
        sensor.AddObservation(distanceBottom);
        float distanceRight = (floorBounds.max.x - transform.position.x) / (floorBounds.max.x - floorBounds.min.x);
        sensor.AddObservation(distanceRight);
        float distanceLeft = (transform.position.x - floorBounds.min.x) / (floorBounds.max.x - floorBounds.min.x);
        sensor.AddObservation(distanceLeft);

        Vector2 linearVelocity = new Vector2(rb.linearVelocity.x / moveSpeed, rb.linearVelocity.z / moveSpeed);
        sensor.AddObservation(linearVelocity);

        int yRotationForward = 0;
        int yRotationBackward = 0;
        int yRotationLeft = 0;
        int yRotationRight = 0;

        switch(transform.eulerAngles.y)
        {
            case 0f: yRotationForward = 1; break;   // Forward
            case 90f: yRotationRight = 1; break;    // Right
            case 180f: yRotationBackward = 1; break; // Backward
            case 270f: yRotationLeft = 1; break;    // Left
        }
        sensor.AddObservation(yRotationForward);
        sensor.AddObservation(yRotationBackward);
        sensor.AddObservation(yRotationLeft);
        sensor.AddObservation(yRotationRight);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the discrete action (0-4)
        int action = actions.DiscreteActions[0];    
        
        if(action == 0) {
            AddReward(-0.01f);
        }

        // Map action to movement
        if (action > 0 && action < actionMap.Length)
        {
            movement = actionMap[action].normalized;
            
            // Update rotation based on movement
            UpdateRotation(action);
        }
        else
        {
            movement = Vector3.zero;
        }
        
        // Apply movement
        ApplyMovement();
        
        // Update animator
        bool isWalking = movement.magnitude > 0.1f;
        if (animator != null)
        {
            animator.SetBool("IsWalking", isWalking);
        }

        ProcessRayPerceptionRewards();

        CheckForFall();

        // Update episode timer
        episodeTimer += Time.fixedDeltaTime;
        
        // End episode if timeout
        if (episodeTimer >= episodeTimeout)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // For manual testing - maps keyboard input to actions
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0; // Default: do nothing
        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            discreteActions[0] = 1; // Left
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            discreteActions[0] = 2; // Right
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            discreteActions[0] = 3; // Forward
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            discreteActions[0] = 4; // Backward
    }

    private void UpdateRotation(int action)
    {
        switch (action)
        {
            case 1: yRotation = -90f; break;  // Left
            case 2: yRotation = 90f; break;   // Right
            case 3: yRotation = 0f; break;    // Forward
            case 4: yRotation = 180f; break;  // Backward
        }
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void ApplyMovement()
    {
        Vector3 velocity = movement * moveSpeed;
        velocity.y = rb.linearVelocity.y; // Preserve gravity
        rb.linearVelocity = velocity;
    }

    private void ResetAgentPosition()
    {
        if (floorTransform != null)
        {
            Collider floorCollider = floorTransform.GetComponent<Collider>();
            Bounds floorBounds = floorCollider.bounds;

            float randomX = Random.Range(floorBounds.min.x + 1.5f, floorBounds.max.x - 1.5f);
            float randomZ = Random.Range(floorBounds.min.z + 1.5f, floorBounds.max.z - 1.5f);
            float spawnY = floorBounds.max.y + 1f;

            transform.position = new Vector3(randomX, spawnY, randomZ);
        }
    }

    private void ProcessRayPerceptionRewards()
    {
        if (raySensor == null || raySensor.RaySensor == null)
            return;

        // Get the ray perception data
        var rayOutput = raySensor.RaySensor.RayPerceptionOutput;
        if (rayOutput == null || rayOutput.RayOutputs == null)
            return;

        // Calculate rewards based on ray detection
        for (int i = 0; i < rayOutput.RayOutputs.Length; i++)
        {
            var ray = rayOutput.RayOutputs[i];

            // Skip rays that didn't hit anything
            if (!ray.HasHit || ray.HitGameObject == null)
                continue;

            // Check if we hit a sphere
            if (ray.HitTaggedObject)
            {
                // Calculate reward based on detected tag and distance

                float distance = 1f - ray.HitFraction;

                // DetectableTags index: 0 = GoodSphere, 1 = BadSphere, 2 = Floor (based on your description)
                if (ray.HitTagIndex == 0) // GoodSphere
                {
                    // Positive reward for seeing good sphere (higher reward for closer objects)
                    AddReward(0.005f * distance);
                }
                else if (ray.HitTagIndex == 1) // BadSphere
                {
                    // Negative reward for seeing bad sphere (higher penalty for closer objects)
                    AddReward(-0.005f * distance);
                }
            }
        }
    }

    // Called when hitting spheres
    public void OnSphereCollected(bool isGoodSphere)
    {
        if (isGoodSphere)
        {
            AddReward(3f); // Positive reward for good spheres
        }
        else
        {
            AddReward(-3f); // Negative reward for bad spheres
        }
    }

    private void CheckForFall()
    {
        float fallThreshold = 0f; // Adjust this value based on your floor height

        if (transform.position.y < fallThreshold)
        {
            OnAgentFailed(); // This calls the method you asked about
        }
    }

    // Optional: Called when agent falls off the floor or other failure conditions
    public void OnAgentFailed()
    {
        AddReward(-10f);
        EndEpisode();
    }
}
