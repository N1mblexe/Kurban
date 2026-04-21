using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : AnimalWalk
{
    public GameObject target;
    public Storage storage;

    public float meatMultipler = 5;

    public ParticleSystem huntEffect;
    public BaseInventory inventory;
    public MeshRenderer meshRenderer;

    public float huntTime;

    public CatHuntState HuntState;
    public CatStoreMeatState StoreMeatState;

    public GameObject mouthMeat;

    public AnimalManager animalManager;

    private new void Awake()
    {
        inventory = BaseInventory.CatInventory();
        agent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();

        agent.updatePosition = true;
        agent.updateRotation = true;

        HuntState = new CatHuntState(this);
        StoreMeatState = new CatStoreMeatState(this);
    }

    IEnumerator thread()
    {
        while (true)
        {
            CatGoToTargetState catGoToState = (CatGoToTargetState)GoToTargetState;

            if (catGoToState.TargetTransform == null)
            {
                SetNewTarget();

                if (target != null && currentState != GoToTargetState)
                {
                    ChangeState(GoToTargetState);
                }
            }

            yield return new WaitForSeconds(3);
        }
    }

    private new void Start()
    {
        GoToTargetState = new CatGoToTargetState(this);

        StartCoroutine(thread());

        ChangeState(GoToTargetState);
    }

    public void SetNewTarget()
    {
        AnimalWalk nextPrey = animalManager.getRandomAnimal();

        CatGoToTargetState catGoToState = (CatGoToTargetState)GoToTargetState;

        if (nextPrey != null)
        {
            target = nextPrey.gameObject;
            catGoToState.SetTarget(target.transform);
        }
        else
        {
            target = null;
            catGoToState.SetTarget((Transform)null);
        }
    }
}

public class CatGoToTargetState : AnimalGoToTargetState, IAnimalState
{
    protected Transform targetTransform;
    protected new CatAI animal;

    public Transform TargetTransform => targetTransform;

    public CatGoToTargetState(AnimalWalk animalController) : base(animalController)
    {
        animal = (CatAI)animalController;
    }

    public new void SetTarget(Transform target)
    {
        targetTransform = target;
        if (target != null)
        {
            base.SetTarget(target);
        }
    }

    public new void EnterState()
    {
        if (targetTransform == null)
        {
            animal.agent.isStopped = true;
        }
        else
        {
            animal.agent.isStopped = false;
            animal.agent.SetDestination(targetTransform.position);

            if (animal.customAnimator != null)
            {
                animal.customAnimator.Play(animal.walkAnimName, true);
            }
        }
    }

    public new void UpdateState()
    {
        if (targetTransform == null)
        {
            return;
        }

        if (animal.agent.isStopped)
        {
            animal.agent.isStopped = false;
            if (animal.customAnimator != null)
            {
                animal.customAnimator.Play(animal.walkAnimName, true);
            }
        }

        animal.agent.SetDestination(targetTransform.position);

        if (!animal.agent.pathPending && animal.agent.remainingDistance <= animal.agent.stoppingDistance)
        {
            if (targetTransform.CompareTag("Animal"))
            {
                animal.ChangeState(animal.HuntState);
            }
            else if (targetTransform.CompareTag("Storage"))
            {
                animal.ChangeState(animal.StoreMeatState);
            }
        }
    }

    public new void ExitState()
    {
        base.ExitState();
    }
}
public class CatHuntState : IAnimalState
{
    private CatAI animal;

    public CatHuntState(CatAI animal)
    {
        this.animal = animal;
    }

    private IEnumerator HuntThread()
    {
        if (animal.huntEffect != null)
            animal.huntEffect.Play();

        yield return new WaitForSeconds(animal.huntTime);

        float weight = 0f;
        if (animal.target != null)
        {
            SliceableObject sliceable = animal.target.GetComponentInChildren<SliceableObject>();
            if (sliceable != null)
            {
                weight = sliceable.CurrentVolume * animal.meatMultipler;;
            }
        }

        if (animal.inventory != null && animal.inventory.getCurrentBackpack() != null)
        {
            animal.inventory.getCurrentBackpack().InsertItem(weight);
        }

        if (animal.target != null)
        {
            Object.Destroy(animal.target);
            animal.target = null;
        }

        if (animal.mouthMeat != null)
            animal.mouthMeat.SetActive(true);

        if (animal.storage != null)
        {
            CatGoToTargetState catGoToState = (CatGoToTargetState)animal.GoToTargetState;
            catGoToState.SetTarget(animal.storage.transform);
            
            animal.ChangeState(animal.GoToTargetState);
        }
    }

    public void EnterState()
    {
        animal.StartCoroutine(HuntThread());
    }

    public void ExitState()
    {
    }

    public void UpdateState()
    {
    }
}

public class CatStoreMeatState : IAnimalState
{
    private CatAI animal;

    public CatStoreMeatState(CatAI animal)
    {
        this.animal = animal;
    }

    public void EnterState()
    {
        animal.StartCoroutine(StoreRoutine());
    }

    private IEnumerator StoreRoutine()
    {
        while (animal.inventory.getCurrentBackpack().weight > 0)
        {
            float meatToStore = animal.inventory.getCurrentBackpack().weight;
            float successfullyStored = animal.storage.Fill(meatToStore);
            animal.inventory.RemoveMeat(successfullyStored);

            if (animal.inventory.getCurrentBackpack().weight <= 0 && animal.mouthMeat != null)
            {
                animal.mouthMeat.SetActive(false);
            }

            if (animal.inventory.getCurrentBackpack().weight > 0)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        animal.target = null;
        CatGoToTargetState catGoToState = (CatGoToTargetState)animal.GoToTargetState;
        catGoToState.SetTarget((Transform)null);

        animal.ChangeState(animal.GoToTargetState);
    }

    public void ExitState()
    {
    }

    public void UpdateState()
    {
    }
}