
using UnityEngine;
using UnityEngine.AI;

public class AnimalIdleState : AnimalBaseState<AnimalFSM>
{
    private float idleTimer;
    private float targetIdleDuration;

    public AnimalIdleState(AnimalFSM machine) : base(machine) { }

    public override void OnEnter()
    {
        idleTimer = 0f;
        targetIdleDuration = Random.Range(machine.minIdleTime, machine.maxIdleTime);

        machine.agent.isStopped = true;

        if (machine.customAnimator != null)
        {
            machine.customAnimator.Play(machine.idleAnimName, true);
        }
    }
    public override void OnUpdate()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= targetIdleDuration)
        {
            machine.ChangeState(machine.WalkState);
        }
    }
}

public class AnimalWalkState : AnimalBaseState<AnimalFSM>
{
    public AnimalWalkState(AnimalFSM machine) : base(machine) { }

    public override void OnEnter()
    {
        machine.agent.isStopped = false;

        FindNewTargetPosition();

        if (machine.customAnimator != null)
        {
            machine.customAnimator.Play(machine.walkAnimName, true);
        }
    }
    public override void OnUpdate()
    {
        if (!machine.agent.pathPending && machine.agent.remainingDistance <= machine.agent.stoppingDistance)
        {
            machine.ChangeState(machine.IdleState);
        }
    }

    private void FindNewTargetPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * machine.wanderRadius;
        Vector3 potentialTarget = machine.transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);

        float distanceToCenter = Vector3.Distance(potentialTarget, machine.centerObj.position);

        if (distanceToCenter <= machine.centerRadius)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialTarget, out hit, 2.0f, NavMesh.AllAreas))
            {
                machine.agent.SetDestination(hit.position);
            }
        }
        else
        {
            Vector3 directionToCenter = (machine.centerObj.position - machine.transform.position).normalized;
            Vector3 moveBackPoint = machine.transform.position + (directionToCenter * machine.wanderRadius);

            machine.agent.SetDestination(moveBackPoint);
        }
    }
}

public class AnimalGoToTargetState : AnimalBaseState<AnimalFSM>
{
    protected Vector3 targetPosition;

    public AnimalGoToTargetState(AnimalFSM machine) : base(machine) { }

    public void SetTarget(Transform target)
    {
        targetPosition = target.position;
    }
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }

    public override void OnEnter()
    {
        machine.agent.isStopped = false;
        machine.agent.SetDestination(targetPosition);

        if (machine.customAnimator != null)
        {
            machine.customAnimator.Play(machine.walkAnimName, true);
        }
    }
    public override void OnUpdate()
    {
        if (!machine.agent.pathPending && machine.agent.remainingDistance <= machine.agent.stoppingDistance)
        {
            machine.ChangeState(machine.IdleState);
        }
    }
}