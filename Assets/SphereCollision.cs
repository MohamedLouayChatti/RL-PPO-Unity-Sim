using UnityEngine;

public class SphereCollision : MonoBehaviour
{
    [Header("Sphere Type")]
    public bool isGoodSphere = true;
    public GameObject goalSpawner;

    void Start()
    {
        Collider sphereCollider = GetComponent<Collider>();
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check for both manual player and ML-Agent
        if (other.CompareTag("Player"))
        {
            /*// Update score for UI
            if (isGoodSphere)
            {
                goalSpawner.AddGoodScore();
            }
            else
            {
                goalSpawner.AddBadScore();
            }*/
            
            // Reward the ML-Agent if it's an agent
            RobotAgent agent = other.GetComponent<RobotAgent>();
            if (agent != null)
            {
                agent.OnSphereCollected(isGoodSphere);
            }
            
            Destroy(gameObject);
        }
    }
}
