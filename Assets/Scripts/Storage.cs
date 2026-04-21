using System;
using System.Collections;
using UnityEngine;

public class Storage : MonoBehaviour , IInteractable
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

        //Implement here to free the cats inventory and fill the storage
    }

    public float Fill(float amount)
    {
        if(capacity == weight)
            return 0;
        
        if(weight + amount > capacity)
        {
            weight = capacity;
            return (capacity - weight);
        }

        capacity = weight + amount;
        return amount;
    }
}
