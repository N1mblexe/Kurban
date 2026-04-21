using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : AnimalWalk
{
    public SliceableObject target;
    public Storage storage;

    public ParticleSystem huntEffect;
    public BaseInventory inventory;
    public MeshRenderer meshRenderer;

    public float huntTime;

    public CatHuntState HuntState;
    public CatStoreMeatState StoreMeatState;

    public GameObject mouthMeat;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;
        agent.updateRotation = true;
    }

}


public class CatGoToTargetState : AnimalGoToTargetState
{
    protected Transform targetTransform;
    protected new CatAI animal;
    public CatGoToTargetState(AnimalWalk animalController) : base(animalController)
    {
    }

    public new void SetTarget(Transform target)
    {
        base.SetTarget(target);
        targetTransform = target;
    }

    public new void UpdateState()
    {
        if (!animal.agent.pathPending && animal.agent.remainingDistance <= animal.agent.stoppingDistance)
        {
            if(targetTransform.CompareTag("Animal"))
                animal.ChangeState(animal.HuntState);
            
            if(targetTransform.CompareTag("Storage"))
                animal.ChangeState(animal.StoreMeatState);
        }
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
        yield return new WaitForSeconds(animal.huntTime);
        //avlama için bekle sonra sıradaki state'e geç
    }

    public void EnterState()
    {
        float weight = animal.target.CurrentVolume;
        animal.inventory.getCurrentBackpack().InsertItem(weight);
        Object.Destroy(animal.target);
        //TODO : dövüşme efektini kullan
        //TODO : Ağzındaki eti aç
        animal.GoToTargetState.SetTarget(animal.storage.transform);
        animal.ChangeState(animal.GoToTargetState);
    }

    public void ExitState()
    {
    }

    public void UpdateState()
    {
    }
}


//Todo init
public class CatStoreMeatState : IAnimalState
{
    public void EnterState()
    {
        throw new System.NotImplementedException();
    }

    public void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}