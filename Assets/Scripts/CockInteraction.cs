using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockInteraction : MonoBehaviour , IInteractable
{
    [SerializeField] private float coolDown = 30f;
    [SerializeField] private bool trigger = true;
    [SerializeField] private float gatherRadius = 5f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cockSound;

    [SerializeField] private AnimalManager manager;


    public void Interact()
    {
        if(!trigger)
            return;

        StartCoroutine(StartCooldown());
        List<AnimalManager.Data> animals = manager.getAnimals();

        audioSource.PlayOneShot(cockSound);

        foreach(var animal in animals)
        {
            Vector2 randomCircle = Random.insideUnitCircle * gatherRadius;
            Vector3 targetPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            animal.animalFSM.GoToLocation(targetPos);
        }
    }

    private IEnumerator StartCooldown()
    {
        trigger = false;
        yield return new WaitForSeconds(coolDown);
        trigger = true;
    }
}
