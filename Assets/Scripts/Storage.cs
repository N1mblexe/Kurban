using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Storage : MonoBehaviour, IInteractable
{
    public static GameObject player;
    [Header("Storage")]
    [SerializeField] private float capacity;
    [SerializeField] private float weight;

    [Header("Animations")]
    [SerializeField] private float animationTime;
    [SerializeField] private Vector3 defaultScale;

    [Header("Other")]
    [SerializeField] private bool canInteract = true;

    void Awake()
    {
        if (player == null)
            player = InventoryUI.Instance.gameObject;
    }

    [SerializeField] private bool place = false;

    public void OnHover()
    {
        transform.DOScale(transform.localScale , animationTime).SetEase(Ease.InOutBack);        
    }
    private IEnumerator StartCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(animationTime + .1f);
        canInteract = true;
    }

    public void Interact()
    {
        if (!canInteract)
            return;

        StartCoroutine(StartCooldown());

        transform.DOScale(transform.localScale , animationTime).SetEase(Ease.InOutBack);        

        PlayerInventory playerInv = PlayerInventory.Instance;
        if (playerInv == null) return;

        if (place)
        {
            float playerMeat = playerInv.currBackpack.weight;

            if (playerMeat > 0)
            {
                float placedAmount = Fill(playerMeat);

                if (placedAmount > 0)
                {
                    playerInv.RemoveMeat(placedAmount);
                    Debug.Log($"Depoya {placedAmount} birim eklendi. Depo: {weight}/{capacity}");
                }
                else
                {
                    Debug.Log("Depo tamamen dolu!");
                }
            }
            else
            {
                Debug.Log("Çantanda depoya koyacak bir şey yok!");
            }
        }
        else
        {
            if (weight > 0)
            {
                float takenAmount = playerInv.AddMeat(weight);
                if (takenAmount > 0)
                {
                    weight -= takenAmount;
                    Debug.Log($"Depodan {takenAmount} birim alındı. Depo: {weight}/{capacity}");
                }
                else
                {
                    Debug.Log("Çantan tamamen dolu, daha fazla alamazsın!");
                }
            }
            else
            {
                Debug.Log("Depo tamamen boş!");
            }
        }
    }
    public float Fill(float amount)
    {
        if (weight >= capacity)
            return 0;

        float availableSpace = capacity - weight;

        if (amount > availableSpace)
        {
            weight = capacity;
            return availableSpace;
        }

        weight += amount;
        return amount;
    }
}