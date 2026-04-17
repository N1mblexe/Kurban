using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class Backpack
{
    public Backpack(float capacity, float weight, float maxVolumePerItem)
    {
        this.capacity = capacity;
        this.weight = weight;
        this.maxVolumePerItem = maxVolumePerItem;
    }

    [SerializeField] public float capacity = 20;
    [SerializeField] public float weight = 0;
    [SerializeField] public float maxVolumePerItem = 10;

    public float getRatio()
    {
        return weight / capacity;
    }

    public bool InsertItem(float size)
    {
        if (size > maxVolumePerItem)
        {
            Debug.Log("Bu parça çantaya sığmak için çok büyük! Daha da küçültmelisin.");
            return false;
        }

        if (weight + size > capacity)
        {
            Debug.Log("Çanta tamamen dolu!");
            return false;
        }

        weight += size;
        return true;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private Backpack currBackpack;
    public Backpack getCurrentBackpack()
    {
        return currBackpack;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            currBackpack.InsertItem(10);
        }
    }

    public float GetMaxItemSize() { return currBackpack.capacity; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateBackpack(Backpack newBackpack)
    {
        float currWeight = currBackpack.weight;
        currBackpack = newBackpack;
        currBackpack.weight = currWeight; 
        
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.UpdateWeightDisplay(currBackpack.weight, currBackpack.capacity);
        }
    }

    public bool TryPickupItem(GameObject itemObject, float itemSize)
    {
        if (itemSize <= 0 || itemSize > currBackpack.maxVolumePerItem) 
            return false;

        if (currBackpack.InsertItem(itemSize))
        {
            Debug.Log($"Eşya çantaya girdi! Güncel Ağırlık: {currBackpack.weight}");
            
            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.UpdateWeightDisplay(currBackpack.weight, currBackpack.capacity);
            }
            

            if (itemObject.TryGetComponent(out Collider col)) col.enabled = false;
            if (itemObject.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

            itemObject.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                Destroy(itemObject); 
            });
            
            return true;
        }

        return false; 
    }

    public float FreeInventory()
    {
        float weight = currBackpack.weight;
        currBackpack.weight = 0;
        return weight;
    }

    public bool RemoveMeat(float amount)
    {
        if(amount <= currBackpack.weight)
        {
            currBackpack.weight -= amount;
            InventoryUI.Instance.UpdateWeightDisplay(currBackpack.weight , currBackpack.capacity);
            return true;
        }

        return false;
    }
}