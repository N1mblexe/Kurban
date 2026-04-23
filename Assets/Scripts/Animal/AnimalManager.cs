using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    public class Data
    {
        public AnimalFSM animalFSM;
        public bool huntStatus;

        public Data(AnimalFSM animalFSM)
        {
            this.animalFSM = animalFSM;
            huntStatus = false;
        }
    }
    [SerializeField] private GameObject prefab;
    [SerializeField] private float radius;
    [SerializeField] private List<Data> animals;
    public List<Data> getAnimals() { return animals; }
    
    [Header("Dynamic Spawn Settings")]
    [SerializeField] private int maxLimit = 10;      
    [SerializeField] private float fastestSpawnDelay = 0.5f; 
    [SerializeField] private float slowestSpawnDelay = 3.0f; 

    public AnimalFSM getNextHunt()
    {
        if (animals == null || animals.Count == 0)
            return null;
            
        foreach(var animal in animals)
        {
            if(!animal.huntStatus)
            {
                animal.huntStatus = true;
                return animal.animalFSM;
            }
        }

        return null;;
    }

    void Awake()
    {
        animals = new List<Data>();
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
            if (animals[i].animalFSM == null)
            {
                animals.RemoveAt(i);
            }
        }
    }

    void SpawnAnimal()
    {
        AnimalFSM temp = Instantiate(prefab).GetComponent<AnimalFSM>();
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
        animals.Add(new Data(temp));
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
}