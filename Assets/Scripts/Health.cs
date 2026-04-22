using UnityEngine;
using System;
using DG.Tweening;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    private Renderer rend;
    private Color originalColor;

    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    public void InitializeHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
    }

    public bool IsDead() { return isDead; }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 mouseDirection)
    {
        if (isDead)
        {
            Die(hitPoint, mouseDirection);
            return;
        }
        currentHealth -= damage;
        Debug.Log($"Hasar Verildi! Kalan Can: {currentHealth}");


        if (currentHealth <= 0f)
        {
            Die(hitPoint, mouseDirection);
        }
        else
        {
            PlayDamageFlash();
        }
    }

    private void PlayDamageFlash()
    {
        if (rend != null)
        {
            rend.material.DOKill();
            rend.material.color = originalColor;

            rend.material.DOColor(Color.red, "_BaseColor", 0.1f).SetLoops(2, LoopType.Yoyo);
        }
    }

    private void Die(Vector3 hitPoint, Vector3 mouseDirection)
    {
        isDead = true;
        GetComponent<SliceableObject>().HandleDeath(hitPoint, mouseDirection);


    }
}