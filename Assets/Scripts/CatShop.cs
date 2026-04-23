using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CatShop : MonoBehaviour, IInteractable
{
    [SerializeField] private AnimalManager targetManager;
    [SerializeField] private Storage targetStorage;

    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private Vector3 spawnOffset;

    [SerializeField] List<CatFSM> cats;
    [SerializeField] private List<float> catPrices;

    [SerializeField] private TextMeshProUGUI priceText;

    [SerializeField] private float animationTime = .4f;
    private bool canInteract = true;

    [Header("Animation")]
    
    [SerializeField] private Vector3 punchAmount = new Vector3(-0.1f, -0.1f, -0.1f);
    [SerializeField] private int vibrato = 5;
    [SerializeField] private float elasticity = 0.5f;
    [SerializeField] private Transform animationBody;
    [SerializeField] private ParticleSystem buyEffect;

    void Start()
    {
        cats = new List<CatFSM>();
        if(catPrices == null)
            catPrices = new List<float>();

        UpdatePriceText();
    }

    private IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(animationTime + .1f);
        canInteract = true;
    }

    public void Interact()
    {
        if(!canInteract)
            return;

        if(catPrices.Count < currentLevel)
            return;

        StartCoroutine(InteractionCooldown());
        
        if(EconomyManager.Instance.TrySpendGold(catPrices[currentLevel]))
        {
            SpawnCat();
            currentLevel++;
            UpdatePriceText();

            buyEffect.Play();

            if(!DOTween.IsTweening(transform))
                animationBody.transform.DOPunchScale(punchAmount, animationTime, vibrato, elasticity);
        }
    }

    private void SpawnCat()
    {
        if(catPrefab == null)
        {
            Debug.LogError("No cat assigned. AKA Missing prefab");
            return;
        }

        GameObject spawningCat = Instantiate(catPrefab);
        
        spawningCat.transform.position = transform.position + spawnOffset;

        CatFSM fsm = spawningCat.GetComponent<CatFSM>();

        fsm.animalManager = targetManager;
        fsm.storage = targetStorage;
        
        cats.Add(fsm);
    }

    private void UpdatePriceText()
    {
        if(currentLevel == maxLevel)
        {
            priceText.text = " ";
            return;
        }

        priceText.text = catPrices[currentLevel].ToString(); 
    }


}
