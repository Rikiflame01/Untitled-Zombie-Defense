using UnityEngine;
using TMPro;
using System.Collections;

public class DayNightTracker : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text dayText;
    public GameObject winCanvas;

    [Header("Objects to Disable on Win")]
    public GameObject[] objectsToDisable;

    [Header("Animation Settings")]
    public float delayBeforeEffect = 0.5f;
    public float backspaceSpeed = 0.05f;
    public float typingSpeed = 0.1f;
    public float moveBackDuration = 0.5f;

    [Header("Movement Settings")]
    public Vector2 effectPosition = Vector2.zero;

    private int currentDay = 0;
    private Coroutine effectCoroutine;
    private Vector2 originalPosition;
    private RectTransform textRectTransform;

    private void Awake()
    {
        textRectTransform = dayText.GetComponent<RectTransform>();
        originalPosition = textRectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        ActionManager.OnBuildStart += OnBuildStartHandler;
    }

    private void OnDisable()
    {
        ActionManager.OnBuildStart -= OnBuildStartHandler;
    }

    private void OnBuildStartHandler()
    {
        currentDay++;

        if (currentDay >= 10)
        {
            if (winCanvas != null)
                winCanvas.SetActive(true);

            if (objectsToDisable != null)
            {
                foreach (GameObject obj in objectsToDisable)
                {
                    if (obj != null)
                        obj.SetActive(false);
                }
            }
        }

        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);
        effectCoroutine = StartCoroutine(DayUpdateEffect());
    }

    private IEnumerator DayUpdateEffect()
    {
        yield return new WaitForSeconds(delayBeforeEffect);

        textRectTransform.anchoredPosition = effectPosition;

        yield return StartCoroutine(PlayTextEffect());

        yield return StartCoroutine(MoveText(textRectTransform, effectPosition, originalPosition, moveBackDuration));
    }

    private IEnumerator PlayTextEffect()
    {
        while (dayText.text.Length > 0)
        {
            dayText.text = dayText.text.Substring(0, dayText.text.Length - 1);
            yield return new WaitForSeconds(backspaceSpeed);
        }

        string newText = "Day " + currentDay;
        dayText.text = "";
        foreach (char letter in newText)
        {
            dayText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator MoveText(RectTransform rectTransform, Vector2 start, Vector2 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = end;
    }
}
