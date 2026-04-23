using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Storage : MonoBehaviour, IInteractable
{
    public static GameObject player;
    [Header("Storage")]
    [SerializeField] private float capacity;
    [SerializeField] private float weight;

    public float getWeightRatio() { return weight / capacity; }

    [Header("Animations")]
    [SerializeField] private float animationTime;
    [SerializeField] private Vector3 defaultScale;
    [SerializeField] private Transform body;

    [SerializeField] private TextMeshProUGUI displayText;

    void UpdateText()
    {
        if (displayText == null)
            return;

        displayText.text = ((int)weight).ToString() + "kg";

        if (weight == 0)
            displayText.text = "EMPTY";
    }

    [Header("Other")]
    [SerializeField] private bool canInteract = true;

    void Awake()
    {
        defaultScale = body.localScale;
    }

    private void Start()
    {
        if (player == null)
            player = InventoryUI.Instance.gameObject;

    }

    [SerializeField] private bool place = false;
    public bool getPlaceStatus() { return place; }

    public void OnHoverEnter()
    {
        OpenAnimation();
        UpdateText();
    }

    public void OnHoverExit()
    {
        CloseAnimation();
    }
    private IEnumerator StartCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(animationTime + .1f);
        canInteract = true;
    }

    [SerializeField] private Vector3 punchAmount = new Vector3(-0.1f, -0.1f, -0.1f);
    [SerializeField] private int vibrato = 5;
    [SerializeField] private float elasticity = 0.5f;

    public void Interact()
    {
        if (!canInteract)
            return;

        StartCoroutine(StartCooldown());

        if (!DOTween.IsTweening(body))
            body.transform.DOPunchScale(punchAmount, animationTime, vibrato, elasticity);

        PlayerInventory playerInv = PlayerInventory.Instance;
        if (playerInv == null) return;

        HandleInteraction(playerInv.getCurrentBackpack(), place);
        InventoryUI.Instance.UpdateWeightDisplay(playerInv.getCurrentBackpack().weight, playerInv.getCurrentBackpack().capacity);
        UpdateText();
    }

    public void HandleInteraction(Backpack backpack, bool state)
    {
        if (state)
        {
            float playerMeat = backpack.weight;

            if (playerMeat > 0)
            {
                float placedAmount = Fill(playerMeat);

                if (placedAmount > 0)
                {
                    backpack.RemoveMeat(placedAmount);
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
                float takenAmount = backpack.AddMeat(weight);
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
        if (!DOTween.IsTweening(body))
            body.transform.DOPunchScale(punchAmount / 2, animationTime, vibrato, elasticity);
        UpdateText();
        return amount;
    }

    [SerializeField] private Transform openPosition, closePosition;
    [SerializeField] private Transform top;

    void OpenAnimation()
    {
        top.transform.DOMove(openPosition.position, animationTime);
        top.transform.DORotate(openPosition.eulerAngles, animationTime);

        if (DOTween.IsTweening(body)) return;

        body.transform.DOPunchScale(punchAmount, animationTime, vibrato, elasticity);

    }
    void CloseAnimation()
    {
        top.transform.DOMove(closePosition.position, animationTime);
        top.transform.DORotate(closePosition.eulerAngles, animationTime);
    }
}