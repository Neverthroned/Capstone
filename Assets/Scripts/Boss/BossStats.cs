using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossStats : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    private float currentHealth;

    public HealthBar healthBar;

    [Header("Resistance")]
    public float resistanceThreshold = 0.5f;  // Kicks in at 50% HP
    public float damageMultiplier = 0.5f;      // Takes 50% damage after threshold

    private bool isResistant = false;
    private bool _isInvincible = false;

    private void Start()
    {
        currentHealth = maxHealth;

        healthBar.SetSliderMax(maxHealth);
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public void SetInvincible(bool invincible)
    {
        _isInvincible = invincible;
    }

    public void TakeDamage(float amount)
    {
        // Invicibility
        if (_isInvincible) return;
        float resistance = GetHealthPercent() <= 0.5f ? 0.5f : 1f;
        currentHealth -= amount * resistance;

        //Resistance
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

        if (currentHealth <= 0f)
        {
            Die();
            SceneManager.LoadScene("WinScreen");
        }
        
    }

    private void Die()
    {
        Debug.Log("Boss died");
        Destroy(gameObject);
    }

    private void Update()
    {
       
    }
}