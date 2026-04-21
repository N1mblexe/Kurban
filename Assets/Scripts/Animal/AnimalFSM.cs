using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AnimalFSM : FSM
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

    public AnimalIdleState IdleState { get; protected set; }
    public AnimalWalkState WalkState { get; protected set; }
    public AnimalGoToTargetState GoToTargetState { get; protected set; }

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

    public void GoToLocation(Vector3 targetPosition)
    {
        GoToTargetState.SetTarget(targetPosition);
        ChangeState(GoToTargetState);
    }
}
