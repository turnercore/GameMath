// ...existing code...
using GameMath.Demo;
using UnityEngine;
using UnityEngine.AI;

public class NPCBrain : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform modelTransform; // assign the visual child (mesh/rig) in the Inspector
    private Vector3 destination;
    public bool invertModelForward = true; // set true if the mesh faces -Z instead of +Z
    public float modelRotateSpeed = 360f; // degrees per second

    void Start()
    {
        // If we have a visual child, let us rotate the model instead of the agent root
        if (agent != null)
        {
            if (modelTransform != null)
                agent.updateRotation = false; // we'll rotate the visual model manually
            else
                agent.updateRotation = true; // let agent rotate the GameObject root

            // make sure angularSpeed > 0 so manual rotation uses a sensible speed
            if (agent.angularSpeed <= 0f)
                agent.angularSpeed = 120f;
        }

        // get a random distance in front of the NPC
        float _randomDistance = Random.Range(5f, 20f);
        destination = transform.position + transform.forward * _randomDistance;
        SetAgentDestinationReliably(destination);
    }

    Vector3 GetRandomDestination()
    {
        float _randomAngle = Random.Range(0f, 360f);
        float _randomDistance = Random.Range(5f, 20f);
        Vector3 _newDirection = Quaternion.Euler(0, _randomAngle, 0) * Vector3.forward;
        return transform.position + _newDirection * _randomDistance;
    }

    void OnDestinationReached()
    {
        destination = GetRandomDestination();
        SetAgentDestinationReliably(destination);
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
        // If we have a visual child, rotate it to face movement direction using agent.velocity
        if (modelTransform != null && agent != null)
        {
            Vector3 vel = agent.velocity;
            vel.y = 0f;
            if (vel.sqrMagnitude > 0.0001f)
            {
                // flip direction if the model's forward is reversed
                Vector3 dir = invertModelForward ? -vel.normalized : vel.normalized;
                Quaternion target = Quaternion.LookRotation(dir);
                // use agent.angularSpeed as degrees/sec to rotate the model
                modelTransform.rotation = Quaternion.RotateTowards(
                    modelTransform.rotation,
                    target,
                    modelRotateSpeed * Time.deltaTime
                );
            }
        }

        // Check if the NPC has reached its destination
        if (
            agent != null
            && !agent.pathPending
            && agent.remainingDistance <= agent.stoppingDistance
        )
        {
            OnDestinationReached();
        }
    }
}
// ...existing code...
