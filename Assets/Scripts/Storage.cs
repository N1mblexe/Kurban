using System;
using System.Collections;
using UnityEngine;

public class Storage : MonoBehaviour, IInteractable
{
    public static GameObject player;
    [Header("Storage")]
    [SerializeField] private float capacity;
    [SerializeField] private float weight;

    [Header("Animations")]
    [SerializeField] private float animationTime;

    [Header("Other")]
    [SerializeField] private bool canInteract = true;

    void Awake()
    {
        if (player == null)
            player = InventoryUI.Instance.gameObject;

        actionGet = new Action(() => get());
        actionPut = new Action(() => put());
    }


    [SerializeField] private GameObject capFront, capBack;
    [SerializeField] [Range(0, 1)] float angle = .4f;
    [SerializeField] private bool xAxis = false;

    private Action currentAction;
    private Action actionPut;
    private Action actionGet;

    public void OnHover()
    {
        Vector3 normalizedDistance = (transform.position - player.transform.position).normalized;

        float distance = (xAxis) ? normalizedDistance.x : normalizedDistance.z;

        if (distance > angle)
        {
            capFront.SetActive(false);
            capBack.SetActive(true);
            currentAction = actionGet;
        }
        else if (distance < -angle)
        {
            capFront.SetActive(true);
            capBack.SetActive(false);
            currentAction = actionPut;
        }
        else
        {
            capFront.SetActive(true);
            capBack.SetActive(true);
            currentAction = null;
        }
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

        if(currentAction != null)
        {
            currentAction.Invoke();
        }
    }

    public float put()
    {
        print("put");
        return 0;
    }

    public float get()
    {
        print("get");
        return 0;
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