using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Health playerHealth;
    public Slider slider;

    private void Start()
    {
        slider.maxValue = playerHealth.maxHealth;
        slider.value = playerHealth.currentHealth;
    }

    private void Update()
    {
        slider.value = playerHealth.currentHealth;
    }
}