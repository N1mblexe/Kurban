public class AnimalBaseState<T> : IState
{
    protected T machine;

    public AnimalBaseState(T machine)
    {
        this.machine = machine;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}