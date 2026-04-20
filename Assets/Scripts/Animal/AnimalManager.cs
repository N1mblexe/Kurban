using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float radius;
    [SerializeField] private List<AnimalWalk> animals;
    
    [Header("Dynamic Spawn Settings")]
    [SerializeField] private int maxLimit = 10;      
    [SerializeField] private float fastestSpawnDelay = 0.5f; 
    [SerializeField] private float slowestSpawnDelay = 3.0f; 

    void Awake()
    {
        animals = new List<AnimalWalk>();
    }

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            CleanUpAnimals();

            if (animals.Count < maxLimit)
            {
                SpawnAnimal();

                float ratio = (float)animals.Count / maxLimit;
                float currentDelay = Mathf.Lerp(fastestSpawnDelay, slowestSpawnDelay, ratio);

                yield return new WaitForSeconds(currentDelay);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public void CleanUpAnimals()
    {
        for (int i = animals.Count - 1; i >= 0; i--)
        {
            if (animals[i] == null)
            {
                animals.RemoveAt(i);
            }
        }
    }

    void SpawnAnimal()
    {
        AnimalWalk temp = Instantiate(prefab).GetComponent<AnimalWalk>();
        Vector2 randomCirclePoint = Random.insideUnitCircle * radius;
        
        Vector3 randomPoint = new Vector3(
            transform.position.x + randomCirclePoint.x, 
            transform.position.y,                               
            transform.position.z + randomCirclePoint.y  
        );
        
        temp.agent.Warp(randomPoint); 
        temp.centerObj = this.transform;
        
        if (temp.GetComponentInChildren<SliceableObject>() != null)
        {
            temp.GetComponentInChildren<SliceableObject>().manager = this;
        }
        
        temp.centerRadius = radius;
        animals.Add(temp);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
}