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

public class BaseInventory : MonoBehaviour
{
    [SerializeField] protected Backpack currBackpack;
    public Backpack getCurrentBackpack()
    {
        return currBackpack;
    }

    public float GetMaxItemSize() { return currBackpack.capacity; }

    public void UpdateBackpack(Backpack newBackpack)
    {
        float currWeight = currBackpack.weight;
        currBackpack = newBackpack;
        currBackpack.weight = currWeight;
    }

    public float FreeInventory()
    {
        float weight = currBackpack.weight;
        currBackpack.weight = 0;
        return weight;
    }

    public bool RemoveMeat(float amount)
    {
        if (amount <= currBackpack.weight)
        {
            currBackpack.weight -= amount;
            return true;
        }

        return false;
    }
}
