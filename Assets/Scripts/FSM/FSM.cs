using UnityEngine;

public interface IState
{
    public void OnEnter() {}
    public void OnUpdate() {}
    public void OnExit() {}
}

public class FSM : MonoBehaviour
{
    protected IState currentState;

    void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdate();
        }
        OnUpdate();
    }

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }

        currentState = newState;
        currentState.OnEnter();
    }

    public virtual void OnAwake() {}
    public virtual void OnStart() {}
    public virtual void OnUpdate() {}
}