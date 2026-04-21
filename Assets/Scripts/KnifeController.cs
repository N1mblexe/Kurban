using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class WeaponStats
{
    public float damage = 1f;
    public float attackRange = 4f;
    public float attackSpeed = .7f;

    public float economyMultiplier = 1.0f;
    public float minSliceVolume = 0.5f;
    public int visualIndex = 0;
    public ParticleSystem slashEffect;
}

public class KnifeController : MonoBehaviour
{
    public static KnifeController Instance { get; private set; }

    [Header("Bıçak İstatistikleri")]
    public WeaponStats currentStats;

    [Header("Referanslar")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject[] knifeVisualModels;
    [SerializeField] private Transform knifeModel;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem defaultSlash;

    private float nextAttackTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        animator = knifeModel.gameObject.GetComponent<Animator>();

        //Default knife
        if (currentStats.visualIndex == 0)
        {
            currentStats.damage = 1;
            currentStats.attackRange = 4;
            currentStats.attackSpeed = 1.1f;
            currentStats.minSliceVolume = .1f;
            currentStats.slashEffect = defaultSlash;
        }
    }

    private void Update()
    {
        HandleAttackInput();
        //HandleProceduralAnimation();

        EquipKnife(currentStats);
    }
    private void HandleAnimation()
    {
        animator.Play("Slash1");
        
    }

    public void EquipKnife(WeaponStats newStats)
    {
        currentStats = newStats;

        for (int i = 0; i < knifeVisualModels.Length; i++)
        {
            if (knifeVisualModels[i] != null)
            {
                knifeVisualModels[i].SetActive(i == currentStats.visualIndex);
            }
        }
    }

    private void HandleAttackInput()
    {
        if (inputManager.AttackInput && Time.time >= nextAttackTime)
        {
            HandleAnimation();
            Invoke("PerformAttack" , 0.1f);
            nextAttackTime = Time.time + (1f / currentStats.attackSpeed);
        }

        inputManager.AttackInput = false;
    }

    private void PerformAttack()
    {

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, currentStats.attackRange))
        {
            if (hit.collider.TryGetComponent(out Health health))
            {
                health.TakeDamage(currentStats.damage, hit.point, knifeModel.transform.eulerAngles);
                CameraEffects.Instance.TriggerShake(.05f , .25f);
                Debug.Log($"Hasar Verildi! Kalan Can: {health.MaxHealth}");
            }
        }

    }

}