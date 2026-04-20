using UnityEngine;
using UnityEngine.AI; // Required for NavMesh

public interface IAnimalState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalWalk : MonoBehaviour
{
    [Header("Animation Info")]
    public TransformAnimator customAnimator; 
    public string idleAnimName = "idle";     
    public string walkAnimName = "cusdeve";  

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

    public void Step()
    {
        if (currentState == WalkState)
        {
            Vector3 targetPosition = transform.position + transform.forward * stepDistance;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, stepDistance * 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                
                agent.nextPosition = transform.position; 
            }
        }
    }
}

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
        
    }
}

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

        if (animal.customAnimator != null)
        {
            animal.customAnimator.Play(animal.walkAnimName, true);
        }
    }

    public void UpdateState()
    {
        Vector3 centerPos = animal.centerObj.position;
        centerPos.y = animal.transform.position.y; 
        
        float distanceToCenter = Vector3.Distance(animal.transform.position, centerPos);

        if (distanceToCenter > animal.upperDistance)
        {
            FindNewTargetPosition(); 
            return;
        }

        if (!animal.agent.pathPending && animal.agent.remainingDistance <= animal.agent.stoppingDistance)
        {
            animal.ChangeState(animal.IdleState);
        }
    }

    public void ExitState()
    {
    }

    private void FindNewTargetPosition()
    {
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;

        float randomDistance = Random.Range(animal.lowerDistance, animal.upperDistance);

        Vector3 targetPoint = animal.centerObj.position + (randomDirection * randomDistance);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPoint, out hit, animal.upperDistance, NavMesh.AllAreas))
        {
            animal.agent.SetDestination(hit.position);
            RotateTowardsTarget(hit.position); 
        }
        else
        {
            animal.agent.SetDestination(animal.centerObj.position);
            RotateTowardsTarget(animal.centerObj.position);
        }
    }

    private void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - animal.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.1f)
        {
            animal.transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }
}