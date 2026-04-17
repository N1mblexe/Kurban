using UnityEngine;
using DG.Tweening;

public class Shop : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        float totalWeight = InventoryManager.Instance.FreeInventory();

        if (totalWeight <= 0)
        {
            Debug.Log("Satacak bir şeyin yok!");
            return;
        }

        float basePrice = EconomyManager.Instance.currentIslandBasePrice;
        float knifeMult = EconomyManager.Instance.currentKnifeMultiplier;
        
        float earnedGold = totalWeight * basePrice * knifeMult;

        Backpack currBackpack = InventoryManager.Instance.getCurrentBackpack();

        EconomyManager.Instance.AddGold(earnedGold);
        InventoryUI.Instance.UpdateWeightDisplay(currBackpack.weight, currBackpack.capacity);

        Debug.Log($"{totalWeight} kg et satıldı. Kazanç: {earnedGold} Altın!");
        PlaySaleEffect();
    }

    void PlaySaleEffect()
    {
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f);
    }
}