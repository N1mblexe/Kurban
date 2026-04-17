using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float radius;
    private List<GameObject> animals;
    [SerializeField] private float minAmount = 10;

    void Awake()
    {
        animals = new List<GameObject>();
    }

    public void RemoveAnimal(GameObject obj)
    {
        animals.Remove(obj);
        UpdateAnimals();
    }
    private void UpdateAnimals()
    {
        while(animals.Count < minAmount)
            SpawnAnimal();
    }

    void Start()
    {
        UpdateAnimals();
    }

    void SpawnAnimal()
    {
        GameObject temp = Instantiate(prefab);
        Vector2 randomCirclePoint = Random.insideUnitCircle * radius;
        
        Vector3 randomPoint = new Vector3(
            transform.position.x + randomCirclePoint.x, 
            transform.position.y,                               
            transform.position.z + randomCirclePoint.y  
        );
        
        temp.transform.position = randomPoint;
        temp.transform.eulerAngles = new Vector3(0 , Random.Range(0 , 360) , 0);

        temp.GetComponent<SliceableObject>().manager = this;
        temp.GetComponent<AnimalWalk>().centerObj = this.gameObject;
        
        animals.Add(temp);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position , radius);
    }
}
