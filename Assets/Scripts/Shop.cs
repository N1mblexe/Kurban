using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class Shop : MonoBehaviour, IInteractable
{
    private EconomyManager economyManager;
    private InventoryManager inventoryManager;

    [SerializeField] private float capacity = 1000f;
    [SerializeField] private float weight = 0f;
    [SerializeField] private float accumulatedMoney = 0f;

    [SerializeField] private GameObject m_Prefab;
    [SerializeField] private Stack<GameObject> m_Stack;
    [SerializeField] private Transform m_Holder;
    [SerializeField] private float m_YDistance = .16f;

    [SerializeField] private GameObject m_MoneyPrefab; 
    [SerializeField] private Stack<GameObject> m_MoneyStack; 
    [SerializeField] private Transform m_MoneyHolder; 
    
    [SerializeField] private float weightPerMeat = 1f;

    [SerializeField] private float defaultInteractDelay = 0.2f;
    [SerializeField] private float minInteractDelay = 0.02f;
    [SerializeField] private float speedUpAmount = 0.015f;

    private Coroutine meatStoringCoroutine;
    private Coroutine moneyCollectingCoroutine;
    private Transform playerTransform; 

    void Awake()
    {
        m_Stack = new Stack<GameObject>();
        m_MoneyStack = new Stack<GameObject>();
    }

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
        PlaySaleEffect(this.gameObject);
    }

    void PlaySaleEffect(GameObject obj)
    {
        obj.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerTransform = other.transform; 

            if(economyManager == null)
                economyManager = other.gameObject.GetComponent<EconomyManager>();
            
            if(inventoryManager == null)
                inventoryManager = other.gameObject.GetComponent<InventoryManager>();

            StartStoringMeat();
            StartCollectingMoney(); 
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerTransform = null; 
            StopStoringMeat();
            StopCollectingMoney(); 
        }        
    }

    private void StartStoringMeat()
    {
        if (meatStoringCoroutine == null)
            meatStoringCoroutine = StartCoroutine(MeatThread());
    }

    private void StopStoringMeat()
    {
        if (meatStoringCoroutine != null)
        {
            StopCoroutine(meatStoringCoroutine);
            meatStoringCoroutine = null;
        }
    }

    private void StartCollectingMoney()
    {
        if (moneyCollectingCoroutine == null)
            moneyCollectingCoroutine = StartCoroutine(MoneyThread());
    }

    private void StopCollectingMoney()
    {
        if (moneyCollectingCoroutine != null)
        {
            StopCoroutine(moneyCollectingCoroutine);
            moneyCollectingCoroutine = null;
        }
    }

    // YENİ: Müşterinin tezgahtan et satın almayı denediği fonksiyon
    public bool TrySellMeat(float demandedWeight)
    {
        if (weight >= demandedWeight) // Tezgahta yeterli et varsa
        {
            weight -= demandedWeight;

            // Kaç tane fiziksel et modeli sileceğimizi hesapla
            int piecesToRemove = Mathf.FloorToInt(demandedWeight / weightPerMeat);
            
            for (int i = 0; i < piecesToRemove; i++)
            {
                if (m_Stack.Count > 0)
                {
                    GameObject topMeat = m_Stack.Pop();
                    Destroy(topMeat);
                }
            }

            // Para hesabı
            float basePrice = EconomyManager.Instance.currentIslandBasePrice;
            float knifeMult = EconomyManager.Instance.currentKnifeMultiplier;
            float earnAmount = demandedWeight * basePrice * knifeMult;

            accumulatedMoney += earnAmount;

            // Satılan et miktarı kadar tezgaha para modeli koy
            for (int i = 0; i < piecesToRemove; i++)
            {
                GameObject money = Instantiate(m_MoneyPrefab);
                money.transform.position = m_MoneyHolder.transform.position + (Vector3.up * m_YDistance * m_MoneyStack.Count);
                m_MoneyStack.Push(money);
            }

            return true; // Satış başarılı
        }
        
        return false; // Satış başarısız (Yeterli et yok)
    }

    private IEnumerator MeatThread()
    {
        float currentDelay = defaultInteractDelay;

        while(inventoryManager.RemoveMeat(weightPerMeat))
        {
            if (playerTransform == null) yield break; 

            weight += weightPerMeat;
            
            GameObject meat = Instantiate(m_Prefab);
            
            Vector3 startPos = playerTransform.position + Vector3.up * 1.5f + playerTransform.forward * -0.2f; 
            meat.transform.position = startPos;
            meat.transform.localScale = Vector3.zero;

            Vector3 targetPos = m_Holder.transform.position + (Vector3.up * m_YDistance * m_Stack.Count);
            
            float moveDuration = 0.5f; 
            meat.transform.DOMove(targetPos, moveDuration);
            meat.transform.DOScale(Vector3.one, moveDuration); 
            
            m_Stack.Push(meat); 

            yield return new WaitForSeconds(currentDelay);
            currentDelay = Mathf.Max(minInteractDelay, currentDelay - speedUpAmount);
        }
        meatStoringCoroutine = null;
    }

    private IEnumerator MoneyThread()
    {
        float currentDelay = defaultInteractDelay;

        while (true) 
        {
            if (playerTransform == null) yield break; 

            if (m_MoneyStack.Count > 0)
            {
                GameObject money = m_MoneyStack.Pop(); 
                
                Vector3 targetPos = playerTransform.position + Vector3.up * 1.0f; 
                
                float moveDuration = 0.5f; 
                money.transform.DOMove(targetPos, moveDuration);
                money.transform.DOScale(Vector3.zero, moveDuration); 
                
                float collectAmount = weightPerMeat * EconomyManager.Instance.currentIslandBasePrice * EconomyManager.Instance.currentKnifeMultiplier;
                
                if (m_MoneyStack.Count == 0)
                    collectAmount = accumulatedMoney;
                else
                    accumulatedMoney -= collectAmount;
                
                economyManager.AddGold(collectAmount); 

                Destroy(money, moveDuration); 
                
                yield return new WaitForSeconds(currentDelay); 
                currentDelay = Mathf.Max(minInteractDelay, currentDelay - speedUpAmount);
            }
            else
            {
                currentDelay = defaultInteractDelay;
                yield return null; 
            }
        }
    }

    // YENİ: Tezgâhta en az 1 parça (satılabilir) et var mı?
    public bool HasAtLeastOneMeat()
    {
        return weight >= weightPerMeat;
    }

    // YENİ: Dışarıdan 1 birimlik etin ağırlığını okuyabilmek için
    public float GetWeightPerMeat()
    {
        return weightPerMeat;
    }

    // YENİ: Sadece 1 parça etin satışı ve animasyonu
    public IEnumerator ProcessSingleSale(Transform customerTransform)
    {
        if (weight < weightPerMeat) yield break; // Güvenlik kontrolü

        weight -= weightPerMeat; // 1 parça eti rezerve et

        float basePrice = EconomyManager.Instance.currentIslandBasePrice;
        float knifeMult = EconomyManager.Instance.currentKnifeMultiplier;
        float earnAmount = weightPerMeat * basePrice * knifeMult;

        // 1. Eti müşteriye ver (Animasyonlu)
        if (m_Stack.Count > 0)
        {
            GameObject topMeat = m_Stack.Pop();
            Vector3 meatTargetPos = customerTransform.position + Vector3.up * 1f; 
            
            topMeat.transform.DOMove(meatTargetPos, 0.3f);
            topMeat.transform.DOScale(Vector3.zero, 0.3f);
            Destroy(topMeat, 0.3f);
        }

        // 2. Parayı tezgaha koy (Animasyonlu)
        accumulatedMoney += earnAmount;
        
        GameObject money = Instantiate(m_MoneyPrefab);
        money.transform.position = customerTransform.position + Vector3.up * 1f;
        money.transform.localScale = Vector3.zero;

        Vector3 moneyTargetPos = m_MoneyHolder.transform.position + (Vector3.up * m_YDistance * m_MoneyStack.Count);
        m_MoneyStack.Push(money);

        money.transform.DOMove(moneyTargetPos, 0.3f);
        money.transform.DOScale(Vector3.one, 0.3f);

        // Satışın ard arda çok hızlı olmaması için ufak bekleme
        yield return new WaitForSeconds(0.15f); 
    }
}