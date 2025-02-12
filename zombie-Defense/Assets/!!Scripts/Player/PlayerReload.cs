using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script manages the player’s ammunition and reload behavior. 
/// It displays a radial slider (using an Image with radial fill) that shows the reload progress.
/// The player can shoot up to 9 times before reloading. 
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

    private int shotsFired = 0;
    private bool isReloading = false;

    private void Start()
    {
        if (radialSlider != null)
            radialSlider.fillAmount = 0f;
    }

    public bool TryShoot()
    {
        if (isReloading)
            return false;

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
    }
}
