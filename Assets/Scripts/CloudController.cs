using UnityEngine;

public class CloudController : MonoBehaviour
{
    private Animator _animator;
    private Renderer _renderer;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _renderer = GetComponentInChildren<Renderer>();


        float randomOffset = Random.Range(0f, 1f);
        _animator.Play(0, -1, randomOffset);
    }

}