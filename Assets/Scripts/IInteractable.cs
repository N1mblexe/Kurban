interface IInteractable
{
    public void Interact();
    public virtual void OnHover() { }
    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }
}