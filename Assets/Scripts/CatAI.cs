using States.Cat;
using UnityEngine;
using UnityEngine.AI;

public class CatFSM : FSM
{
    [SerializeField] public GameObject target;
    public GameObject mouthMeat;
    [SerializeField] public Storage storage;

    public AnimalManager animalManager;

    public ParticleSystem huntEffect;
    [SerializeField] public BaseInventory inventory;
    public MeshRenderer meshRenderer;

    [Header("Stats")]
    public float meatMultipler = 1;
    public float huntTime;
    public float wanderTime = 3f;

    [Header("Stats")]
    public HuntState huntState;
    public StoreMeatState storeMeatState;
    public GoToTargetState goToTargetState;
    public WanderState wanderState;

    public NavMeshAgent agent;

    [Header("Animation Info")]
    public TransformAnimator animator;
    public string idleAnimName = "idle";
    public string walkAnimName = "cusdeve";


    public override void OnAwake()
    {
        inventory = BaseInventory.CatInventory();

        agent = GetComponent<NavMeshAgent>();

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