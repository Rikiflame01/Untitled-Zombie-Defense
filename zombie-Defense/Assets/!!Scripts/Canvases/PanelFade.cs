using UnityEngine;
using System.Collections;

public class PanelFade : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("Time (in seconds) it takes to fade in and fade out.")]
    public float fadeDuration = 1f;
    
    [Tooltip("Time (in seconds) the panel remains fully visible before fading out.")]
    public float displayDuration = 1f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component is missing. Please add a CanvasGroup to this panel.");
        }
    }

    public void FadePanel()
    {
        StartCoroutine(FadeInAndOut());
    }

    private IEnumerator FadeInAndOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
