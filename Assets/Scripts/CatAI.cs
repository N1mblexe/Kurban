using System.Collections;
using States.Cat;
using UnityEngine;
using UnityEngine.AI;

public class CatFSM : FSM
{
    public GameObject target;
    public GameObject mouthMeat;
    public Storage storage;

    public AnimalManager animalManager;

    public ParticleSystem huntEffect;
    public BaseInventory inventory;
    public MeshRenderer meshRenderer;

    [Header("Stats")]
    public float meatMultipler = 5;
    public float huntTime;
    public float wanderTime = 3f;

    [Header("Stats")]
    public HuntState huntState;
    public StoreMeatState storeMeatState;
    public GoToTargetState goToTargetState;
    public WanderState wanderState;

    public NavMeshAgent agent;


    public override void OnAwake()
    {
        inventory = BaseInventory.CatInventory();

        agent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();

        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    public override void OnStart()
    {   
        wanderState = new WanderState(this);
        huntState = new HuntState(this);
        storeMeatState = new StoreMeatState(this);
        goToTargetState = new GoToTargetState(this);

        ChangeState(wanderState);
    }
}
