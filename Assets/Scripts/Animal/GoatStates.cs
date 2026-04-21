using System.Collections;
using UnityEngine;

public class GoatChargeState : IState
{
    private AnimalFSM animal;
    private float chargeDelay = 2;
    private GameObject player;
    private float overChargeDistance = 2;

    public GoatChargeState(AnimalFSM controller, GameObject player)
    {
        this.animal = controller;
        this.player = player;
        this.chargeDelay = 2f;

    }

    public void OnEnter()
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

    public void OnUpdate()
    {
        Vector3 chargePosition = new Vector3(player.transform.position.x, animal.transform.position.y, player.transform.position.z);
        animal.gameObject.transform.LookAt(chargePosition);

    }

}