using UnityEngine;
using UnityEngine.AI;

// 1. THE STATE INTERFACE
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
    public float centerRadius;
    public float wanderRadius = 5f;

    [Header("State Durations")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [HideInInspector] public NavMeshAgent agent;

    private IAnimalState currentState;
    public AnimalIdleState IdleState { get; private set; }
    public AnimalWalkState WalkState { get; private set; }
    public AnimalGoToTargetState GoToTargetState { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    private void Start()
    {
        IdleState = new AnimalIdleState(this);
        WalkState = new AnimalWalkState(this);
        GoToTargetState = new AnimalGoToTargetState(this);

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

    // Step fonksiyonunun içini boşalttık. 
    // Unity'nin kendi Event'i arayıp hata vermemesi için metodun ismi kalabilir ama içi boş olmalı.
    public void Step()
    {
        // Şimdilik boş. NavMesh kendisi yürüyor.
    }

    public void GoToLocation(Vector3 targetPosition)
    {
        GoToTargetState.SetTarget(targetPosition);
        ChangeState(GoToTargetState);
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

        // 1. Hedef belirle ve NavMesh'i oraya gönder
        FindNewTargetPosition();

        // 2. Yürüme animasyonunu oynat
        if (animal.customAnimator != null)
        {
            animal.customAnimator.Play(animal.walkAnimName, true);
        }
    }

    public void UpdateState()
    {
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

        Vector2 randomOffset = Random.insideUnitCircle * animal.wanderRadius;
        Vector3 potentialTarget = animal.transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        float distanceToCenter = Vector3.Distance(potentialTarget, animal.centerObj.position);

        if (distanceToCenter <= animal.centerRadius)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialTarget, out hit, 2.0f, NavMesh.AllAreas))
            {
                animal.agent.SetDestination(hit.position);
            }
        }
        else
        {
            Vector3 directionToCenter = (animal.centerObj.position - animal.transform.position).normalized;
            Vector3 moveBackPoint = animal.transform.position + (directionToCenter * animal.wanderRadius);

            animal.agent.SetDestination(moveBackPoint);
        }
    }
}

// --- GO TO TARGET STATE ---
public class AnimalGoToTargetState : IAnimalState
{
    private AnimalWalk animal;
    private Vector3 targetPosition;

    public AnimalGoToTargetState(AnimalWalk animalController)
    {
        animal = animalController;
    }

    // Hedefi belirlemek için kullanacağız
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }

    public void EnterState()
    {
        animal.agent.isStopped = false;
        animal.agent.SetDestination(targetPosition);

        if (animal.customAnimator != null)
        {
            animal.customAnimator.Play(animal.walkAnimName, true);
        }
    }

    public void UpdateState()
    {
        // Hedefe ulaştıysa Idle (bekleme) durumuna geri dön
        if (!animal.agent.pathPending && animal.agent.remainingDistance <= animal.agent.stoppingDistance)
        {
            animal.ChangeState(animal.IdleState);
        }
    }

    public void ExitState()
    {
        // Gerekirse hedefe ulaştığında yapılacak temizlik işlemleri
    }
}