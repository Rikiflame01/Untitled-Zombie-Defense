using UnityEngine;
using TMPro;
using System.Collections;

public class BuildTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Duration of the build phase in seconds.")]
    public float buildDuration = 30f;

    [Header("UI Elements")]
    [Tooltip("TextMeshProUGUI to display the countdown.")]
    public TextMeshProUGUI timerText;

    private float timeLeft;
    private bool isRunning = false;
    private Coroutine countdownCoroutine;

    void Start()
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        
        ActionManager.OnBuildStart += StartBuildTimer;
        ActionManager.OnBuildSkip += EndBuildTimerEarly;
    }

    void OnDisable()
    {
        ActionManager.OnBuildStart -= StartBuildTimer;
        ActionManager.OnBuildSkip -= EndBuildTimerEarly;
    }

    private void StartBuildTimer()
    {
        if (isRunning) return;

        timeLeft = buildDuration;
        isRunning = true;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }

        countdownCoroutine = StartCoroutine(BuildCountdownCoroutine());
    }

    private IEnumerator BuildCountdownCoroutine()
    {
        while (timeLeft > 0)
        {
            UpdateTimerUI(timeLeft);
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        EndBuildTimer();
    }

    private void UpdateTimerUI(float time)
    {
        if (timerText != null)
        {
            timerText.text = $"Build Phase: {Mathf.Ceil(time)}s";
        }
        else
        {
            Debug.LogWarning("Timer TextMeshPro reference is missing!");
        }
    }

    public void EndBuildTimerEarly()
    {
        if (!isRunning) return;

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        EndBuildTimer();
    }

    private void EndBuildTimer()
    {
        isRunning = false;
        timeLeft = 0;
        UpdateTimerUI(0);

        Debug.Log("Build phase ended early. Starting defense phase.");

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        ActionManager.InvokeDefenseStart();
    }
}
