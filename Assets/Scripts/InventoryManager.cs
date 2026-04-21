using DG.Tweening;
using UnityEngine;

public class PlayerInventory : BaseInventory
{
    public static PlayerInventory Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public new void UpdateBackpack(Backpack newBackpack)
    {
        base.UpdateBackpack(newBackpack);

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.UpdateWeightDisplay(currBackpack.weight, currBackpack.capacity);
        }
    }

    public new float FreeInventory()
    {
        float output = base.FreeInventory();
        InventoryUI.Instance.UpdateWeightDisplay(currBackpack.weight, currBackpack.capacity);
        return output;
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

    public new bool RemoveMeat(float amount)
    {
        bool output = base.RemoveMeat(amount);

        if(output)
            InventoryUI.Instance.UpdateWeightDisplay(currBackpack.weight, currBackpack.capacity);

        return output;

    }

}
