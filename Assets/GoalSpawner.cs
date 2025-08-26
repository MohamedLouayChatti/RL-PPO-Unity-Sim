using UnityEngine;

public class GoalSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Spawn frequency in seconds. Set to -1 to disable automatic spawning.")]
    public float frequency = 5f;
    public GameObject goodSpherePrefab;
    public GameObject badSpherePrefab;
    public Transform floorTransform;
    
    [Header("Spawn Position")]
    public float spawnHeight = 1f;
    
    private float timer = 0f;
    private bool autoSpawningEnabled = true;
    private Collider floorCollider;
    private Bounds floorBounds;

    void Start()
    {
        // Get floor bounds
        floorCollider = floorTransform.GetComponent<Collider>();
        floorBounds = floorCollider.bounds;

        // Check if automatic spawning should be disabled
        autoSpawningEnabled = frequency > 0;
        
        // Only initialize timer if auto-spawning is enabled
        if (autoSpawningEnabled)
        {
            timer = frequency;
        }
    }

    void Update()
    {
        // Only run the timer logic if auto-spawning is enabled
        if (autoSpawningEnabled && timer > 0f)
        {
            // Count down the timer
            timer -= Time.deltaTime;

            // When timer reaches zero, spawn a sphere
            if (timer <= 0f)
            {
                SpawnRandomSphere();
                timer = frequency; // Reset timer
            }
        }
    }

    public void SpawnSpheresOnStart()
    {
        for (int i = 0; i < 80; i++)
        {
            SpawnRandomSphere();
        }
    }

    public void SpawnRandomSphere()
    {
        // Generate random position within floor bounds
        float randomX = Random.Range(floorBounds.min.x, floorBounds.max.x);
        float randomZ = Random.Range(floorBounds.min.z, floorBounds.max.z);
        float fixedY = spawnHeight; // Fixed height above floor

        Vector3 spawnPosition = new Vector3(randomX, fixedY, randomZ);

        // Randomly choose between good and bad sphere
        bool spawnGoodSphere = Random.Range(0,3) > 0;
        GameObject sphereToSpawn = spawnGoodSphere ? goodSpherePrefab : badSpherePrefab;

        // Spawn the selected sphere
        GameObject spawnedSphere = Instantiate(sphereToSpawn, spawnPosition, Quaternion.identity);
        
        // Add collision script and set sphere type
        SphereCollision collision = spawnedSphere.GetComponent<SphereCollision>();
        if (collision == null)
        {
            collision = spawnedSphere.AddComponent<SphereCollision>();
        }
        collision.isGoodSphere = spawnGoodSphere;
    }

    private bool IsWithinFloorBounds(Vector3 position, Bounds floorBounds)
    {
        // Check only X and Z coordinates (ignore Y height)
        return position.x >= floorBounds.min.x && position.x <= floorBounds.max.x &&
               position.z >= floorBounds.min.z && position.z <= floorBounds.max.z;
    }

    // Add these methods for episode reset
    public void ClearAllSpheres()
    {
        // Get floor bounds to filter spheres
        Collider floorCollider = floorTransform.GetComponent<Collider>();
        Bounds floorBounds = floorCollider.bounds;

        // Find all spheres in the scene
        GameObject[] goodSpheres = GameObject.FindGameObjectsWithTag("GoodSphere");
        GameObject[] badSpheres = GameObject.FindGameObjectsWithTag("BadSphere");

        // Only destroy spheres within this floor's bounds
        foreach (GameObject sphere in goodSpheres)
        {
            // Check if sphere is within the X and Z bounds of this floor
            if (IsWithinFloorBounds(sphere.transform.position, floorBounds))
            {
                Destroy(sphere);
            }
        }

        foreach (GameObject sphere in badSpheres)
        {
            // Check if sphere is within the X and Z bounds of this floor
            if (IsWithinFloorBounds(sphere.transform.position, floorBounds))
            {
                Destroy(sphere);
            }
        }
    }

    public void ResetSpawner()
    {
        // Only reset the timer if auto-spawning is enabled
        if (autoSpawningEnabled)
        {
            timer = frequency;
        }
    }
    
    // Complete episode reset
    public void ResetForNewEpisode()
    {
        ClearAllSpheres();
        ResetSpawner();
    }
}
