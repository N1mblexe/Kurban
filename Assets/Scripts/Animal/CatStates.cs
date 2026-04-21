using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace States.Cat
{
    public class WanderState : AnimalBaseState<CatFSM>
    {
        public WanderState(CatFSM machine) : base(machine) { }

        public override void OnEnter()
        {
            AnimalFSM nextHunt = machine.animalManager.getNextHunt();

            if (nextHunt == null)
            {
                machine.animator.Play(machine.idleAnimName);
                machine.StartCoroutine(SearchHunt());
                return;
            }

            machine.target = nextHunt.gameObject;
            machine.ChangeState(machine.goToTargetState);
        }

        private IEnumerator SearchHunt()
        {
            yield return new WaitForSeconds(machine.wanderTime);
            OnEnter();
        }
    }

    public class GoToTargetState : AnimalBaseState<CatFSM>
    {
        public GoToTargetState(CatFSM machine) : base(machine) { }

        public override void OnEnter()
        {
            machine.animator.Play(machine.walkAnimName, true);
        }

        public override void OnUpdate()
        {
            if (machine.target == null)
            {
                machine.ChangeState(machine.wanderState);
                return;
            }

            machine.agent.SetDestination(machine.target.transform.position);

            if (HasReachedTarget(machine.agent))
            {
                if (machine.target.CompareTag("Storage"))
                {
                    machine.ChangeState(machine.storeMeatState);
                }
                else if (machine.target.CompareTag("Animal"))
                {
                    machine.ChangeState(machine.huntState);
                }
                else { Debug.Log($"Nereye geldim ben aq... buraya geldin aq :{machine.target.name}"); }
            }
        }

        bool HasReachedTarget(NavMeshAgent agent)
        {
            if (!agent.pathPending)
                if (agent.remainingDistance <= agent.stoppingDistance)
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                        return true;

            return false;
        }
    }

    public class HuntState : AnimalBaseState<CatFSM>
    {
        public HuntState(CatFSM machine) : base(machine) { }

        public override void OnEnter()
        {
            machine.StartCoroutine(HuntProcess());
            machine.animator.Play(machine.idleAnimName);

        }

        private IEnumerator HuntProcess()
        {
            float weight = 0f;
            if (machine.target != null)
            {
                SliceableObject slicable = machine.target.GetComponentInChildren<SliceableObject>();
                if (slicable != null)
                    weight = slicable.CurrentVolume * machine.meatMultipler;
                else
                    Debug.Log("Ya slicableobject bulamıyom");
            }

            Backpack backpack = machine.inventory.getCurrentBackpack();

            if (machine.inventory != null && backpack != null)
                backpack.InsertItem(weight);

            GameObject effectObj = machine.huntEffect.gameObject;
            Vector3 defaultScale = effectObj.transform.localScale;

            if (machine.target != null)
            {
                NavMeshAgent agent = machine.target.GetComponent<NavMeshAgent>();
                agent.ResetPath();
                agent.enabled = false;

                machine.transform.DOJump(machine.target.transform.position, 3, 1, .5f);

                effectObj.transform.position = machine.target.transform.position;
                effectObj.SetActive(true);
                effectObj.transform.localScale = Vector3.zero;
                effectObj.transform.DOScale(defaultScale, .6f).SetEase(Ease.OutBack);

                machine.meshRenderer.enabled = false;
                Object.Destroy(machine.target);
                machine.target = null;
            }

            yield return new WaitForSeconds(machine.huntTime);

            effectObj.transform.DOScale(Vector3.zero, .6f).OnComplete(() =>
            {
                effectObj.SetActive(false);
                effectObj.transform.localScale = defaultScale;
            });

            machine.transform.position = machine.huntEffect.transform.position;
            machine.mouthMeat.SetActive(true);
            machine.meshRenderer.enabled = true;

            machine.target = machine.storage.gameObject;
            machine.ChangeState(machine.goToTargetState);
        }
    }

    public class StoreMeatState : AnimalBaseState<CatFSM>
    {
        public StoreMeatState(CatFSM machine) : base(machine) { }

        public override void OnEnter()
        {
            machine.StartCoroutine(StoreProcess());
        }
        private IEnumerator StoreProcess()
        {
            while (machine.inventory.getCurrentBackpack().weight > 0)
            {
                float meatToStore = machine.inventory.getCurrentBackpack().weight;
                float successfullyStored = machine.storage.Fill(meatToStore);
                machine.inventory.RemoveMeat(successfullyStored);

                if (machine.inventory.getCurrentBackpack().weight <= 0)
                    machine.mouthMeat.SetActive(false);

                if (machine.inventory.getCurrentBackpack().weight > 0)
                    yield return new WaitForSeconds(1f);
            }

            AnimalFSM nextHunt = machine.animalManager.getNextHunt();
            if (nextHunt != null)
            {
                machine.target = nextHunt.gameObject;
                machine.ChangeState(machine.goToTargetState);
            }
            else
            {
                machine.ChangeState(machine.wanderState);
            }

        }
    }
}