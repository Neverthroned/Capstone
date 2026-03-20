using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStats : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    private float currentHealth;

    public HealthBar healthBar;

    [Header("Resistance")]
    public float resistanceThreshold = 0.5f;  // Kicks in at 50% HP
    public float damageMultiplier = 0.5f;      // Takes 50% damage after threshold

    private bool isResistant = false;

    private void Start()
    {
        currentHealth = maxHealth;

        healthBar.SetSliderMax(maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isResistant)
            amount *= damageMultiplier;

        SetHealth(currentHealth - amount);
    }

    private void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);

        // Trigger resistance at half health
        if (!isResistant && currentHealth <= maxHealth * resistanceThreshold)
        {
            isResistant = true;
            Debug.Log("Boss entered resistant phase!");
        }

        healthBar.SetSlider(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Boss died");
        Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            TakeDamage(20f);
        }
    }
}