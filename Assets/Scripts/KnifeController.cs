using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class WeaponStats
{
    public float damage = 1f;
    public float attackRange = 4f;
    public float attackSpeed = .1f;

    public float economyMultiplier = 1.0f;
    public float minSliceVolume = 0.5f;
    public int visualIndex = 0;
    public GameObject slashEffect;
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
    [SerializeField] private GameObject defaultSlash;
    [SerializeField] private ParticleSystem confettiEffect;
    [SerializeField] private ParticleSystem distortionRing;

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
            currentStats.attackSpeed = 1.7f;
            currentStats.minSliceVolume = .1f;
            currentStats.slashEffect = defaultSlash;
        }
        EquipKnife(currentStats);

    }

    private void Update()
    {
        HandleAttackInput();
        //HandleProceduralAnimation();
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
            Invoke("PerformAttack", 0.1f);
            nextAttackTime = Time.time + (1f / currentStats.attackSpeed);
        }

        inputManager.AttackInput = false;
    }

    private void PerformAttack()
    {
        Vector3 attackCenter = playerCamera.transform.position + playerCamera.transform.forward * (currentStats.attackRange * 0.5f);

        float attackRadius = currentStats.attackRange * 0.5f;

        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent(out Health health))
            {
                Vector3 closestPoint = hitCollider.ClosestPoint(attackCenter);

                health.TakeDamage(currentStats.damage, closestPoint, knifeModel.transform.eulerAngles);

                if (health.IsDead())
                {
                    PlayDieEffect(closestPoint - knifeModel.transform.up * 2, knifeModel.transform.eulerAngles);
                }

                CameraEffects.Instance.TriggerShake(.05f, .25f);
                Debug.Log($"Alan Hasarı Verildi! Vurulan: {hitCollider.name}, Kalan Can: {health.MaxHealth}");
            }
        }
    }

    private void PlayDieEffect(Vector3 diePosition, Vector3 angle)
    {
        confettiEffect.Stop();
        distortionRing.Stop();

        confettiEffect.transform.position = diePosition;
        confettiEffect.transform.eulerAngles = angle;

        distortionRing.transform.position = diePosition;

        distortionRing.Play();
        confettiEffect.Play();
    }
    private void PlaySlashEffect()
    {

    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null && currentStats != null)
        {
            Gizmos.color = Color.red;

            Vector3 attackCenter = playerCamera.transform.position + playerCamera.transform.forward * (currentStats.attackRange * 0.5f);
            float attackRadius = currentStats.attackRange * 0.5f;

            Gizmos.DrawWireSphere(attackCenter, attackRadius);
        }
    }

}