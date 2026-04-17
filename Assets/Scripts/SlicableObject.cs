using System;
using System.Collections;
using DG.Tweening;
using Hanzzz.MeshSlicerFree;
using Unity.VisualScripting;
using UnityEngine;

public class SliceableObject : MonoBehaviour
{
    [Header("Kesim Ayarları")]
    [SerializeField] private Material crossSectionMaterial;
    [SerializeField] private float separationForce = 150f;
    [SerializeField] private float horizontalTiltMultiplier = 0.3f;

    [Header("Sağlık ve Kesim Ayarları")]
    public bool isOriginalBody = true; 
    
    [Header("Hacim ve Envanter")]
    [SerializeField] private float minVolumeToSlice = 0.05f; 
    
    public AnimalManager manager;

    [SerializeField] private float liveTime = 10f;

    public float CurrentVolume { get; private set; }

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeathWithCutInfo += HandleDeath;
        }
        CalculateVolume();
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeathWithCutInfo -= HandleDeath;
        }
    }

    private void HandleDeath(Vector3 hitPoint, Vector3 mouseDirection)
    {
        Slice(hitPoint, mouseDirection);
    }

    private void Start()
    {
        CalculateVolume();

        if(!isOriginalBody)
            StartCoroutine(DieAfterTime(liveTime));
    }

    public void CalculateVolume()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            CurrentVolume = rend.bounds.size.x * rend.bounds.size.y * rend.bounds.size.z;
        }
        else
        {
            CurrentVolume = 0f;
        }
    }

    public void Slice(Vector3 hitPoint, Vector3 mouseDirection)
    {
        if (CurrentVolume < KnifeController.Instance.currentStats.minSliceVolume)
        {
            Debug.Log($"Bu bıçakla daha küçük kesemezsin! Hacim: {CurrentVolume}");
            return; 
        }
        if (TryGetComponent(out Outline outline))
        {
            outline.enabled = false;
        }

        Vector3 p1 = hitPoint;
        Vector3 p2 = hitPoint + Camera.main.transform.forward; 
        Vector3 baseDirection = Camera.main.transform.up;
        Vector3 tiltOffset = Camera.main.transform.right * (mouseDirection.x * horizontalTiltMultiplier);
        Vector3 sliceLine = (baseDirection + tiltOffset).normalized;
        Vector3 p3 = hitPoint + sliceLine;

        MeshSlicer meshSlicer = new MeshSlicer();
        var result = meshSlicer.Slice(gameObject, (p1, p2, p3), crossSectionMaterial);

        if (result.Item1 != null && result.Item2 != null)
        {
            Vector3 pushDirection = Vector3.Cross(Camera.main.transform.forward, sliceLine).normalized;

            SetupSlicedPiece(result.Item1, pushDirection);
            SetupSlicedPiece(result.Item2, -pushDirection); 

            if (manager != null) manager.RemoveAnimal(this.gameObject);
            Destroy(gameObject); 
        }
    }

    private void SetupSlicedPiece(GameObject piece, Vector3 forceDirection)
    {
        Collider[] oldColliders = piece.GetComponents<Collider>();
        foreach (Collider col in oldColliders)
        {
            Destroy(col);
        }

        MeshCollider newMeshCollider = piece.AddComponent<MeshCollider>();
        newMeshCollider.convex = true; 

        MeshFilter meshFilter = piece.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            newMeshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        
        if (!piece.TryGetComponent(out Rigidbody rb))
        {
            rb = piece.AddComponent<Rigidbody>();
        }

        if (!piece.TryGetComponent(out SliceableObject newSliceable))
        {
            newSliceable = piece.AddComponent<SliceableObject>();
        }
        
        newSliceable.GetComponent<SliceableObject>().isOriginalBody = false;
        
        if(isOriginalBody)
            newSliceable.GetComponent<Health>().InitializeHealth(health.MaxHealth/2);
        else
            newSliceable.GetComponent<Health>().InitializeHealth(health.MaxHealth);

        newSliceable.crossSectionMaterial = this.crossSectionMaterial;
        newSliceable.separationForce = this.separationForce;
        newSliceable.horizontalTiltMultiplier = this.horizontalTiltMultiplier;
        newSliceable.minVolumeToSlice = this.minVolumeToSlice;
        newSliceable.manager = this.manager; 

        if (piece.TryGetComponent(out AnimalWalk animalWalk)) animalWalk.enabled = false;
        if (piece.TryGetComponent(out Animator animator)) animator.enabled = false;

        if (!piece.TryGetComponent(out Interactable interactable))
        {
            piece.AddComponent<Interactable>();
        }
        
        Vector3 finalForce = (forceDirection + (Camera.main.transform.forward * 0.5f)).normalized * separationForce;
        rb.AddForce(finalForce);
    }

    private IEnumerator DieAfterTime(float time)
    {
        float waitDuration = time / 2f;
        float shrinkDuration = time / 2f;

        DG.Tweening.Sequence dieSequence = DOTween.Sequence();

        dieSequence.AppendInterval(waitDuration);

        dieSequence.Append(transform.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InQuad));

        dieSequence.OnComplete(() => {
            Destroy(this.gameObject);
        });

        yield return null; 
    }
}