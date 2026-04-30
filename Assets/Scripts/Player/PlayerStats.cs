using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    private float currentHealth;
    public HealthBar healthBar;

    [Header("Damage Cooldown")]
    public float damageCooldown = 0.5f;
    private float _lastDamageTime = -999f;

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetSliderMax(maxHealth);
    }

    private void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        healthBar.SetSlider(currentHealth);

        if(currentHealth <= 0f)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            SceneManager.LoadScene("LoseScreen");
        }
    }

    public void TakeDamage(float amount)
    {
        SetHealth(currentHealth - amount);
    }

    // I Frames lol
    public void TakeDamageWithCooldown(float amount)
    {
        if (Time.time - _lastDamageTime < damageCooldown) return;
        _lastDamageTime = Time.time;
        SetHealth(currentHealth - amount);
        Debug.Log($"Flame damage: {amount}, health now: {currentHealth}");
    }

    public void Heal(float amount)
    {
        SetHealth(currentHealth + amount);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.G))
        {
            Heal(20f);
        }
    }
}
