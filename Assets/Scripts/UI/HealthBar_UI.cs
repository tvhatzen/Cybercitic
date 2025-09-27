using UnityEngine;
using UnityEngine.UI;

public class HealthBar_UI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private HealthSystem targetHealth; // player or enemy

    private void Awake()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        // set its position to the player
        //transform.position = GameObject.targetHealth.position;
    }

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            // initialise the slider
            healthSlider.maxValue = targetHealth.CurrentHealth; 
            healthSlider.value = targetHealth.CurrentHealth;

            targetHealth.OnHealthChanged += UpdateBar;
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(int newHealth)
    {
        healthSlider.value = newHealth;
    }

    // set target at runtime for enemies spawned
    public void SetTarget(HealthSystem newTarget)
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= UpdateBar;

        targetHealth = newTarget;

        if (targetHealth != null)
        {
            healthSlider.maxValue = targetHealth.CurrentHealth;
            healthSlider.value = targetHealth.CurrentHealth;
            targetHealth.OnHealthChanged += UpdateBar;
        }
    }
}
// also, get rid of slider part at the start, start at 100