using System.Collections.Generic;
using UnityEngine;

public class DroneAI : FSM
{
    public Storage target;
    public List<Storage> storages;
    public Storage baseStorage;
    public float pickUpHeight = 2f;
    public float speed;

    public Backpack backpack;

    void Awake()
    {
        if (backpack == null)
            backpack = new Backpack(50, 0, float.MaxValue);
    }


    [Header("States")]
    public Drone.DecisionState decisionState;
    public Drone.GoToTargetState goToTargetState;
    public Drone.InteractWithStorageState interactWithStorageState;

    public int targetAmount() => storages.Count;

    void Start()
    {
        decisionState = new Drone.DecisionState(this);
        goToTargetState = new Drone.GoToTargetState(this);
        interactWithStorageState = new Drone.InteractWithStorageState(this);

        ChangeState(decisionState);
    }

}
