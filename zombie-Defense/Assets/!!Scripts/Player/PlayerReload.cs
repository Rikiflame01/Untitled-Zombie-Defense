using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script manages the player’s ammunition and reload behavior. 
/// It displays a radial slider (using an Image with radial fill) that shows the reload progress.
/// The player can shoot up to 9 times before reloading. 
/// Additionally, a UI element communicates when reloading and pulses if the player tries to shoot while reloading.
/// Attach this script to the same GameObject that has PlayerControls.
/// </summary>
public class PlayerReload : MonoBehaviour
{
    [Header("Reload Settings")]
    [Tooltip("Number of shots allowed before reload.")]
    [SerializeField] public int maxShots = 9;
    
    [Tooltip("Time (in seconds) required to reload.")]
    [SerializeField] private float reloadTime = 2f;
    
    [Tooltip("Radial slider image that shows reload progress. Make sure its Image Type is set to Filled (Radial).")]
    [SerializeField] private Image radialSlider;

    [Header("Reloading Indicator")]
    [Tooltip("UI element (e.g., a Text GameObject) that indicates the player is reloading.")]
    [SerializeField] private GameObject reloadingIndicator;

    private int shotsFired = 0;
    private bool isReloading = false;
    private bool isPulsing = false;

    private void Start()
    {
        if (radialSlider != null)
            radialSlider.fillAmount = 0f;
        
        if (reloadingIndicator != null)
            reloadingIndicator.SetActive(false);
    }

    public bool TryShoot()
    {
        if (isReloading)
        {
            if (reloadingIndicator != null && !isPulsing)
                StartCoroutine(PulseIndicator());
            return false;
        }

        if (shotsFired < maxShots)
        {
            shotsFired++;

            if (shotsFired >= maxShots)
            {
                StartCoroutine(ReloadRoutine());
            }
            return true;
        }
        return false;
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        
        if (reloadingIndicator != null)
            reloadingIndicator.SetActive(true);
        
        float timer = 0f;
        SoundManager.Instance.PlaySFX("reload", 1f);

        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            if (radialSlider != null)
                radialSlider.fillAmount = timer / reloadTime;
            yield return null;
        }

        shotsFired = 0;
        if (radialSlider != null)
            radialSlider.fillAmount = 0f;
        
        isReloading = false;
        
        if (reloadingIndicator != null)
            reloadingIndicator.SetActive(false);
    }

    private IEnumerator PulseIndicator()
    {
        isPulsing = true;
        
        Vector3 originalScale = reloadingIndicator.transform.localScale;
        float pulseDuration = 0.3f;
        float pulseScaleFactor = 1.2f;
        float halfDuration = pulseDuration / 2f;
        float timer = 0f;
        
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(1f, pulseScaleFactor, timer / halfDuration);
            reloadingIndicator.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(pulseScaleFactor, 1f, timer / halfDuration);
            reloadingIndicator.transform.localScale = originalScale * scale;
            yield return null;
        }
        
        reloadingIndicator.transform.localScale = originalScale;
        isPulsing = false;
    }
}
