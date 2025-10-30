using UnityEngine;
using UnityEngine.AI;

public class NPCBrain : MonoBehaviour
{
    public NavMeshAgent agent;
    private Vector3 destination;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get a random distance in front of the NPC
        float _randomDistance = Random.Range(5f, 20f);
        destination = transform.position + transform.forward * _randomDistance;
        agent.SetDestination(destination);
    }

    // When the NPC reaches its destination, pick a new one
    Vector3 GetRandomDestination()
    {
        float _randomAngle = Random.Range(0f, 360f);
        float _randomDistance = Random.Range(5f, 20f);
        Vector3 _newDirection = Quaternion.Euler(0, _randomAngle, 0) * Vector3.forward;
        return transform.position + _newDirection * _randomDistance;
    }

    // When the NPC reaches its destination, pick a new one
    void OnDestinationReached()
    {
        destination = GetRandomDestination();
        SetAgentDestinationReliably(destination);
        // turn to face the new destination
        Vector3 _lookDirection = (destination - transform.position).normalized;
        if (_lookDirection != Vector3.zero)
        {
            Quaternion _lookRotation = Quaternion.LookRotation(_lookDirection);
            transform.rotation = _lookRotation;
        }
    }

    void SetAgentDestinationReliably(Vector3 dest, int maxAttempts = 10)
    {
        bool _destinationSet = false;
        for (int i = 0; i < maxAttempts; i++)
        {
            if (agent.SetDestination(dest))
            {
                _destinationSet = true;
                break;
            }
        }
        if (!_destinationSet)
        {
            Debug.LogWarning("Failed to set destination for NPC after multiple attempts.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the NPC has reached its destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            OnDestinationReached();
        }
    }
}
