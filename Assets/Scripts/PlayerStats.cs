using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    private float currentHealth;

    public HealthBar healthBar;

    private void Start()
    {
        currentHealth = maxHealth;

        healthBar.SetSliderMax(maxHealth);
    }

    private void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        healthBar.SetSlider(currentHealth);
    }

    public void TakeDamage(float amount)
    {
        SetHealth(currentHealth - amount);
    }

    public void Heal(float amount)
    {
        SetHealth(currentHealth + amount);
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.F))
        {
            TakeDamage(20f);
        }
    }
}
