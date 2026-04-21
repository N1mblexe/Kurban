using System;
using System.Collections;
using UnityEngine;

public class Storage : MonoBehaviour, IInteractable
{
    [Header("Storage")]
    [SerializeField] private float capacity;
    [SerializeField] private float weight;
    
    [Header("Animations")]
    [SerializeField] private float animationTime;

    [Header("Other")]
    [SerializeField] private bool canInteract = true;    
    
    private IEnumerator StartCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(animationTime + .1f);
        canInteract = true;
    }

    public void Interact()
    {
        if(!canInteract)
            return;

        StartCoroutine(StartCooldown());

        // MISSING CONNECTION: Interact() lacks a reference to CatAI to pull from its inventory. The actual transfer logic has been placed in CatStoreMeatState instead.
    }

    public float Fill(float amount)
    {
        if(weight >= capacity)
            return 0;
        
        float availableSpace = capacity - weight;

        if(amount > availableSpace)
        {
            weight = capacity;
            return availableSpace;
        }

        weight += amount;
        return amount;
    }
}