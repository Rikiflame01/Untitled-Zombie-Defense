using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RadialHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthFillImage; // Radial fill image

    [Header("Settings")]
    [SerializeField] private Color healthBarColor = Color.red;
    [Tooltip("Flash color for when the health changes.")]
    [SerializeField] private Color flashColor = Color.white;
    [Tooltip("Time the health bar flashes.")]
    [SerializeField] private float flashDuration = 0.2f;
    [Tooltip("Speed at which the health bar smooths out.")]
    [SerializeField] private float lerpSpeed = 2.0f;
    [Tooltip("Scale amount for the grow/shrink effect.")]
    [SerializeField] private float pulseScale = 1.2f;
    [Tooltip("Speed of the grow/shrink effect.")]
    [SerializeField] private float pulseSpeed = 5.0f;

    private IHealth healthComponent;
    private float targetFillAmount;
    private Vector3 originalScale;
    private bool isFlashing = false;

    private void Start()
    {
        if (healthFillImage == null)
        {
            Debug.LogError("RadialHealthBar: Missing reference to Fill Image.");
            return;
        }

        healthComponent = GetComponentInParent<IHealth>();

        if (healthComponent == null)
        {
            Debug.LogError("RadialHealthBar: No IHealth component found on parent.");
            Destroy(gameObject);
            return;
        }

        targetFillAmount = healthComponent.CurrentHealth / (float)healthComponent.MaxHealth;
        healthFillImage.fillAmount = targetFillAmount;
        healthFillImage.color = healthBarColor;
        originalScale = transform.localScale;

        if (healthComponent is Health health)
        {
            health.OnHealthChanged += HandleHealthChange;
        }
    }

    private void Update()
    {
        if (Mathf.Abs(healthFillImage.fillAmount - targetFillAmount) > 0.01f)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
        }
    }

    private void HandleHealthChange(int currentHealth, int maxHealth)
    {
        targetFillAmount = currentHealth / (float)maxHealth;

        if (!isFlashing)
        {
            StartCoroutine(FlashEffect());
        }
        StartCoroutine(PulseEffect());
    }

    private IEnumerator FlashEffect()
    {
        isFlashing = true;
        Color originalColor = healthFillImage.color;
        healthFillImage.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        healthFillImage.color = originalColor;
        isFlashing = false;
    }

    private IEnumerator PulseEffect()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 0.5f / pulseSpeed)
        {
            transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, elapsedTime * pulseSpeed * 2);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < 0.5f / pulseSpeed)
        {
            transform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, elapsedTime * pulseSpeed * 2);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        if (healthComponent is Health health)
        {
            health.OnHealthChanged -= HandleHealthChange;
        }
    }
}
