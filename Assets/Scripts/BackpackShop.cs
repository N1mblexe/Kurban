using UnityEngine;
using DG.Tweening;

public class BackpackShop : MonoBehaviour, IInteractable
{
    [Header("Çanta Özellikleri")]
    public string backpackName = "Deri Kasap Çantası";
    public float price = 3000f;
    
    [Header("Yeni Değerler")]
    public float newCapacity = 350f;
    public float newMaxVolumePerItem = 60f;

    private bool isBought = false;

    public void Interact()
    {
        if (isBought)
        {
            Debug.Log("Bu çantayı zaten satın aldın!");
            return;
        }

        if (EconomyManager.Instance.TrySpendGold(price))
        {
            Backpack upgradedBackpack = new Backpack(newCapacity, 0, newMaxVolumePerItem);
            
            InventoryManager.Instance.UpdateBackpack(upgradedBackpack);

            isBought = true;
            Debug.Log($"{backpackName} satın alındı! Yeni Kapasite: {newCapacity} kg");

            float currentWeight = InventoryManager.Instance.FreeInventory(); 

            PlayPurchaseEffect();
        }
        else
        {
            Debug.Log("Yeterli altının yok! Gerekli: " + price);
            EconomyManager.Instance.ShakeGoldUI();
        }
    }

    void PlayPurchaseEffect()
    {
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f).OnComplete(() => 
        {
            gameObject.SetActive(false);
        });
    }
}