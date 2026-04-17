using System.Collections;
using UnityEngine;

//TODO: maybe redo the walking system for better control
public class AnimalWalk : MonoBehaviour
{
    public GameObject centerObj;
    [SerializeField] private float lowerDistance, upperDistance;
    
    [SerializeField] private float minIdleTime = 4f, maxIdleTime = 10f;
    [SerializeField] private float minWalkTime = 3f, maxWalkTime = 7f;

    private Animator anim;
    private bool isReturning = false;
    private bool isWalking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(AnimalBrain());
    }

    void Update()
    {
        if (isWalking)
        {
            KeepInDistance();
        }
    }

    public void KeepInDistance()
    {  
        float distance = Vector3.Distance(transform.position, centerObj.transform.position);
        Vector3 centerPos = new Vector3(centerObj.transform.position.x, transform.position.y, centerObj.transform.position.z);
        
        if (distance > upperDistance)
        {
            transform.position = centerPos;
            isReturning = false;
        }
        else if (distance > lowerDistance)
        {
            if (!isReturning)
            {
                transform.LookAt(centerPos);
                transform.Rotate(0, Random.Range(-30, 30), 0, Space.World);
                
                Vector3 currentEuler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(0, currentEuler.y, 0);

                anim.SetTrigger("Walk");
                
                isReturning = true;
            }
        }
        else
        {
            isReturning = false;
        }
    }

    public void TryToTriggerIdle()
    {
        if(Random.Range(0 , 100) > 20)
        {
            isWalking = false;
        }
    }


    IEnumerator AnimalBrain()
    {
        while (true)
        {
            yield return new WaitForSeconds(minWalkTime);
            transform.eulerAngles = new Vector3(0 , transform.rotation.eulerAngles.y , 0);
            if(isWalking)
                continue;

            transform.eulerAngles = new Vector3(0 , transform.rotation.eulerAngles.y , 0);
            anim.SetTrigger("Idle");
            transform.eulerAngles = new Vector3(0 , transform.rotation.eulerAngles.y , 0);
            transform.position = new Vector3(transform.position.x, centerObj.transform.position.y  , transform.position.z);
            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

            transform.eulerAngles = new Vector3(0 , transform.rotation.eulerAngles.y , 0);
            isWalking = true;
            anim.SetTrigger("Walk");
            transform.Rotate(0, Random.Range(0, 360), 0, Space.World);
   
        }
    }
}