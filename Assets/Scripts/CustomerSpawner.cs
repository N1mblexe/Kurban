using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Referanslar")]
    public Shop targetShop;
    public GameObject customerPrefab;
    
    [Header("Pozisyonlar")]
    public Transform spawnPoint;
    public Transform waitPoint; // Kasanın tam önü (Z ekseni (mavi ok) dükkana doğru bakmalı)

    [Header("Kuyruk Ayarları")]
    public int maxQueueSize = 5;       // Kuyrukta en fazla kaç kişi bekleyebilir
    public float queueSpacing = 1.5f;  // Müşteriler arası mesafe

    [Header("Müşteri Ayarları")]
    public float minDemandWeight = 1f;
    public float maxDemandWeight = 5f;
    public float timeBetweenSpawns = 3f; // Kasaya yeni birinin gelme hızı

    private List<GameObject> queue = new List<GameObject>(); // Kuyruktaki müşterilerin listesi

    private void Start()
    {
        StartCoroutine(SpawnLoop());       // Müşteri yaratma döngüsü
        StartCoroutine(ProcessQueueLoop());// Kasadaki satışı yönetme döngüsü
    }

    // 1. MÜŞTERİ YARATMA VE SIRAYA SOKMA DÖNGÜSÜ
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Eğer kuyruk dolu değilse yeni müşteri yarat
            if (queue.Count < maxQueueSize)
            {
                GameObject customer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
                queue.Add(customer);
                UpdateQueuePositions(); // Sırayı düzenle
            }
            
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    // Sıradaki herkesi kendi pozisyonuna yürütür
    private void UpdateQueuePositions()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            // waitPoint'in Z ekseni (forward) tersine doğru dizilmeleri için hesaplama
            Vector3 targetPos = waitPoint.position - (waitPoint.forward * queueSpacing * i);
            
            queue[i].transform.DOMove(targetPos, .3f).SetEase(Ease.OutQuad);
            queue[i].transform.LookAt(waitPoint); // Hepsi kasaya doğru baksın
        }
    }

    // 2. KASADAKİ İŞLEMİ (SATIŞI) YÖNETME DÖNGÜSÜ
    private IEnumerator ProcessQueueLoop()
    {
        while (true)
        {
            // Eğer sırada biri varsa
            if (queue.Count > 0)
            {
                GameObject frontCustomer = queue[0];
                
                // Müşterinin toplam ne kadar et istediğini belirle
                float demandedAmount = Mathf.Round(Random.Range(minDemandWeight, maxDemandWeight));
                float remainingDemand = demandedAmount; // Alması gereken kalan miktar
                
                float pieceWeight = targetShop.GetWeightPerMeat();

                // Müşterinin alacağı et bitene kadar bu döngüde kalır
                while (remainingDemand > 0)
                {
                    // Eğer tezgâhta en az 1 parça et varsa, hemen al
                    if (targetShop.HasAtLeastOneMeat())
                    {
                        yield return StartCoroutine(targetShop.ProcessSingleSale(frontCustomer.transform));
                        remainingDemand -= pieceWeight; // Kalan ihtiyacını düşür
                    }
                    else
                    {
                        // Tezgâhta et yoksa (veya oyuncu henüz koymadıysa), et gelmesini bekle
                        yield return new WaitForSeconds(0.1f); 
                    }
                }

                // Döngüden çıktıysa tüm etlerini almıştır, sevinip gitsin
                frontCustomer.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f);
                yield return new WaitForSeconds(0.5f);
                
                frontCustomer.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
                Destroy(frontCustomer, 0.5f); 

                // Müşteriyi kuyruktan çıkar ve sırayı bir adım öne kaydır
                queue.RemoveAt(0);
                UpdateQueuePositions();
            }
            
            // Sıradaki müşterinin işleminin başlaması için ufak bir bekleme
            yield return new WaitForSeconds(0.5f); 
        }
    }
}