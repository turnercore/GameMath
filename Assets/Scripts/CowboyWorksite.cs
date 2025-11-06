using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class CowboyWorksite : Worksite
{
    [SerializeField]
    private Cow cowPrefab;

    [SerializeField]
    private int numberOfCowsToCreate = 5;
    private Collider worksiteArea;

    [SerializeField]
    private Vector2 nightHours = new(24, 6); // 8 PM to 6 AM
    private TimeOfDay clock;
    private readonly Cow[] herdOfCows = new Cow[0];

    void Awake()
    {
        worksiteArea = GetComponent<Collider>();
        clock = FindFirstObjectByType<TimeOfDay>();
        if (clock == null)
        {
            Debug.LogError(
                "No TimeOfDay object found in the scene. CowboyWorksite requires a TimeOfDay component to function properly."
            );
        }
    }

    // Create cows at this worksite
    void Start()
    {
        CreateCows();
    }

    //Create cows at random locations throughout the area of the worksite defined by its collider
    void CreateCows()
    {
        for (int i = 0; i < numberOfCowsToCreate; i++)
        {
            Vector3 randomPosition = new(
                Random.Range(worksiteArea.bounds.min.x, worksiteArea.bounds.max.x),
                1.0f,
                Random.Range(worksiteArea.bounds.min.z, worksiteArea.bounds.max.z)
            );
            // Create the cow instance
            Cow cowInstance = Instantiate(cowPrefab, randomPosition, Quaternion.identity);
            // Set as child of self for organization
            cowInstance.transform.parent = transform;
            // Add to herd array
            herdOfCows.Append(cowInstance);
        }
    }

    // Set all cows navmesh agent speed to 0 at night
    void Update()
    {
        float currentHour = clock.GetCurrentHour();
        bool isNightTime = (currentHour >= nightHours.x || currentHour < nightHours.y);

        foreach (Cow cow in herdOfCows)
        {
            if (cow.TryGetComponent<NavMeshAgent>(out var navMeshAgent))
            {
                navMeshAgent.isStopped = isNightTime;
            }
        }
    }
}
