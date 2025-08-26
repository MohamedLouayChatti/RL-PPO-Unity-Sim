using UnityEngine;

public class RobotSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject robotPrefab;
    public Transform floorTransform;

    void Start()
    {
        SpawnRobotRandomly();
    }

    void SpawnRobotRandomly()
    {
        // Get floor bounds
        Collider floorCollider = floorTransform.GetComponent<Collider>();
        Bounds floorBounds = floorCollider.bounds;

        // Random position within floor bounds
        float randomX = Random.Range(floorBounds.min.x, floorBounds.max.x);
        float randomZ = Random.Range(floorBounds.min.z, floorBounds.max.z);
        float floorY = floorBounds.max.y; // Top of floor

        float rotationY = Random.Range(0f, 360f); // Random rotation around Y-axis

        Vector3 spawnPosition = new Vector3(randomX, floorY + 1f, randomZ);

        // Spawn robot
        robotPrefab.transform.position = spawnPosition;
    }
}