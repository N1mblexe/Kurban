using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class GoatFSM : AnimalWalk
{
    private Transform player;
    private GoatChargeState chargeState;
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private bool canCharge = true;

    [SerializeField] private float chargeDistance = 12;
    [SerializeField] private float knockPower = 10f;
    void Awake()
    {
        player = PlayerInventory.Instance.gameObject.transform;
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = agent.updateRotation = true;
        defaultSpeed = agent.speed;
        defaultAcceleration = agent.acceleration;

        chargeAcceleration = defaultAcceleration * 5;
    }

    void Start()
    {
        IdleState = new AnimalIdleState(this);
        WalkState = new AnimalWalkState(this);
        GoToTargetState = new AnimalGoToTargetState(this);
        chargeState = new GoatChargeState(this, PlayerInventory.Instance.gameObject);

        ChangeState(IdleState);
    }

    void Update()
    {
        if (currentState != null)
            currentState.UpdateState();

        if (canCharge && Vector3.Distance(transform.position, player.position) < chargeDistance)
        {
            ChangeState(chargeState);
            StartCoroutine(ChargeCooldown());
        }
    }

    [SerializeField] private float chargeSpeed = 40f;
    [SerializeField] private float chargeAcceleration;
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float defaultAcceleration;

    public void ChangeState(IAnimalState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState();
        }

        currentState = newState;
        currentState.EnterState();

        if(currentState == GoToTargetState || currentState == chargeState)
        {
            agent.speed = chargeSpeed;
            agent.acceleration = chargeAcceleration;
        }
        else if(currentState == WalkState)
        {
            agent.speed = defaultSpeed;
            agent.acceleration = defaultAcceleration;
        }

    }

    IEnumerator ChargeCooldown()
    {
        canCharge = false;
        yield return new WaitForSeconds(chargeCooldown);
        canCharge = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && (currentState == GoToTargetState || currentState == chargeState))
        {
            print("YEY");
            Vector3 impactDirection = (player.transform.position - transform.position).normalized;
            player.GetComponent<MovementManager>().AddImpact(impactDirection , knockPower);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position , chargeDistance);
    }
}

public class GoatChargeState : IAnimalState
{
    private AnimalWalk animal;
    private float chargeDelay = 2;
    private GameObject player;
    private float overChargeDistance = 2;

    public GoatChargeState(AnimalWalk controller, GameObject player)
    {
        this.animal = controller;
        this.player = player;
        this.chargeDelay = 2f;

    }


    public void EnterState()
    {

        if (animal.customAnimator != null)
        {
            animal.customAnimator.Play(animal.idleAnimName, true);
        }

        animal.StartCoroutine(StartCharging());
    }

    private IEnumerator StartCharging()
    {
        yield return new WaitForSeconds(chargeDelay);

        Vector3 chargePosition = player.transform.position + (player.transform.position - animal.transform.position).normalized * overChargeDistance;

        animal.GoToLocation(chargePosition);
    }

    public void UpdateState()
    {
        Vector3 chargePosition = new Vector3(player.transform.position.x, animal.transform.position.y, player.transform.position.z);
        animal.gameObject.transform.LookAt(chargePosition);

    }

    public void ExitState()
    {
    }
}