using UnityEngine;
using UnityEngine.AI; // Required for NavMesh

// 1. THE STATE INTERFACE
public interface IAnimalState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

// 2. THE MAIN CONTROLLER
[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWalk : MonoBehaviour
{
    [Header("Animation Info")]
    public TransformAnimator customAnimator; // TransformAnimator referansı
    public string idleAnimName = "idle";     // Bekleme animasyonunun JSON'daki adı
    public string walkAnimName = "cusdeve";  // Yürüme animasyonunun JSON'daki adı

    [Header("Pathfind Limits")]
    public Transform centerObj;
    public float lowerDistance = 3f;
    public float upperDistance = 15f;

    [Header("Movement & Animation")]
    public float stepDistance = 1f;
    public float rotationSpeed = 5f;

    [Header("State Durations")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [HideInInspector] public NavMeshAgent agent;

    private IAnimalState currentState;
    public AnimalIdleState IdleState { get; private set; }
    public AnimalWalkState WalkState { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    private void Start()
    {
        IdleState = new AnimalIdleState(this);
        WalkState = new AnimalWalkState(this);
        ChangeState(IdleState);
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    public void ChangeState(IAnimalState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState();
        }

        currentState = newState;
        currentState.EnterState();
    }

    // --- ANIMATION EVENT ---
    // Make sure your Walk animation calls this method when the foot hits the ground
    public void Step()
    {
        // Only step forward if we are actually in the walking state
        if (currentState == WalkState)
        {
            // Calculate where the step will land
            Vector3 targetPosition = transform.position + transform.forward * stepDistance;
            
            // Sample the NavMesh to ensure we don't step through a wall or off a cliff
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, stepDistance * 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                
                // Tell the invisible NavMeshAgent that we physically moved, so it updates its path logic
                agent.nextPosition = transform.position; 
            }
        }
    }
}

// 3. CONCRETE STATES

// --- IDLE STATE ---
public class AnimalIdleState : IAnimalState
{
    private AnimalWalk animal;
    private float idleTimer;
    private float targetIdleDuration;

    public AnimalIdleState(AnimalWalk animalController)
    {
        animal = animalController;
    }

    public void EnterState()
    {
        idleTimer = 0f;
        targetIdleDuration = Random.Range(animal.minIdleTime, animal.maxIdleTime);
        animal.agent.isStopped = true;

        if (animal.customAnimator != null)
        {
            animal.customAnimator.Play(animal.idleAnimName, true); 
        }
    }

    public void UpdateState()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= targetIdleDuration)
        {
            animal.ChangeState(animal.WalkState);
        }
    }

    public void ExitState()
    {
        // Clean up when leaving Idle
    }
}

// --- WALK STATE ---
public class AnimalWalkState : IAnimalState
{
    private AnimalWalk animal;

    public AnimalWalkState(AnimalWalk animalController)
    {
        animal = animalController;
    }

    public void EnterState()
    {
        animal.agent.isStopped = false;
        FindNewTargetPosition();

        // TODO YERİNE BURA GELECEK:
        if (animal.customAnimator != null)
        {
            // Yürüme animasyonunu döngülü (loop = true) oynat
            animal.customAnimator.Play(animal.walkAnimName, true);
        }
    }

    public void UpdateState()
    {
        // Check if the agent has reached its destination
        // (pathPending ensures we don't trigger this before the path is calculated)
        if (!animal.agent.pathPending && animal.agent.remainingDistance <= animal.agent.stoppingDistance)
        {
            animal.ChangeState(animal.IdleState);
        }
    }

    public void ExitState()
    {
        // Clean up when leaving Walk
    }

    private void FindNewTargetPosition()
    {
        // 1. Pick a random 2D direction and distance based on your upper/lower limits
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(animal.lowerDistance, animal.upperDistance);

        // 2. Convert it to a 3D point relative to the center object
        Vector3 targetPoint = animal.centerObj.position + new Vector3(randomCircle.x, 0, randomCircle.y) * randomDistance;

        // 3. Ensure the point is actually walkable on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPoint, out hit, animal.upperDistance, NavMesh.AllAreas))
        {
            animal.agent.SetDestination(hit.position);
        }
        else
        {
            // If the random point was off the map, just walk back to the center object as a fallback
            animal.agent.SetDestination(animal.centerObj.position);
        }
    }
}