using UnityEngine;
using System;
using System.Collections;
using TMPro;
using DG.Tweening;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    private int enemyDeathCount = 0;
    private float roundStartTime;
    private bool playerDamaged = false;
    private float roundDuration;
    private Health playerHealth;
    public int totalScore = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI enemyDeathText;
    public TextMeshProUGUI roundTimeText;
    public TextMeshProUGUI playerDamageText;
    public TextMeshProUGUI totalScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        ActionManager.OnDefenseStart += StartRound;
        ActionManager.OnDefenseStop += EndRound;
    }

    private void OnDisable()
    {
        ActionManager.OnDefenseStart -= StartRound;
        ActionManager.OnDefenseStop -= EndRound;
    }

    private void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.OnDied += OnPlayerDied;
            playerHealth.OnHealthChanged += OnPlayerHealthChanged;
        }
    }

    private void StartRound()
    {
        totalScore = 0;
        enemyDeathCount = 0;
        playerDamaged = false;
        roundStartTime = Time.time;
        if (enemyDeathText != null) enemyDeathText.text = "0";
        if (roundTimeText != null) roundTimeText.text = "0s";
        if (playerDamageText != null) playerDamageText.text = "0";
        if (totalScoreText != null) totalScoreText.text = "0";
        UpdateScoreUI();
    }

    private void EndRound()
    {
        roundDuration = Time.time - roundStartTime;
        int damagePenalty = playerDamaged ? -1 : 3;
        int enemyScore = enemyDeathCount;
        float timePenalty = Mathf.Max(0, roundDuration / 5);
        totalScore = Mathf.RoundToInt(damagePenalty + enemyScore * 1.5f - timePenalty);
        StartCoroutine(AnimateScoreSequence());
    }

    private void OnPlayerDied(GameObject player)
    {
        playerDamaged = true;
    }

    private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth < maxHealth)
        {
            playerDamaged = true;
        }
    }

    public void OnEnemyDeath(GameObject enemy)
    {
        enemyDeathCount++;
    }

    public void DecreaseTotalScore(int amount)
    {
        if (totalScore == 0) return;
        totalScore -= amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (totalScoreText != null)
            totalScoreText.text = totalScore.ToString();
        GameObject buildPreviewObject = FindObjectByTag("TxtBuildPreview");
        if (buildPreviewObject != null)
        {
            TextMeshProUGUI tmpComponent = buildPreviewObject.GetComponent<TextMeshProUGUI>();
            if (tmpComponent != null)
            {
                tmpComponent.text = totalScore.ToString();
            }
            else
            {
                Debug.LogError("TxtBuildPreview found, but it has no TextMeshProUGUI component!");
            }
        }
        else
        {
            Debug.LogError("TxtBuildPreview object not found in the scene! Make sure it's tagged correctly.");
        }
    }

    private IEnumerator AnimateScoreSequence()
    {
        if (enemyDeathText != null)
        {
            yield return StartCoroutine(AnimateCountUp(enemyDeathText, enemyDeathCount, "", ""));
        }
        if (roundTimeText != null)
        {
            int roundTimeSeconds = Mathf.RoundToInt(roundDuration);
            yield return StartCoroutine(AnimateCountUp(roundTimeText, roundTimeSeconds, "", "s"));
        }
        if (playerDamageText != null)
        {
            int damageValue = playerDamaged ? 1 : 3;
            string prefix = playerDamaged ? "Yes - " : "No + ";
            yield return StartCoroutine(AnimateCountUp(playerDamageText, damageValue, prefix, ""));
        }
        if (totalScoreText != null)
        {
            yield return StartCoroutine(AnimateCountUp(totalScoreText, totalScore, "", ""));
        }
        UpdateScoreUI();
    }

    private IEnumerator AnimateCountUp(TextMeshProUGUI textObj, int finalValue, string prefix, string suffix)
    {
        int currentValue = 0;
        Vector3 originalScale = textObj.transform.localScale;
        textObj.text = prefix + "0" + suffix;
        float allocatedTime = 1.25f;
        float popDuration = 0.2f;
        float countDuration = allocatedTime - popDuration;
        float delayPerCount = finalValue > 0 ? countDuration / finalValue : 0f;
        while (currentValue < finalValue)
        {
            currentValue++;
            textObj.text = prefix + currentValue.ToString() + suffix;
            SoundManager.Instance.PlaySFX("scoreCount", 1f);
            textObj.transform.DOShakePosition(0.1f, new Vector3(5, 5, 0));
            yield return new WaitForSeconds(delayPerCount);
        }
        SoundManager.Instance.PlaySFX("scoreEnd", 1f);
        textObj.transform.DOScale(originalScale * 1.2f, 0.1f).OnComplete(() =>
        {
            textObj.transform.DOScale(originalScale, 0.1f);
        });
        yield return new WaitForSeconds(popDuration);
    }

    private GameObject FindObjectByTag(string tag)
    {
        GameObject obj = GameObject.FindGameObjectWithTag(tag);
        if (obj == null)
        {
            Debug.LogError($"No object found with tag '{tag}'. Make sure it's assigned in the Unity Editor.");
        }
        return obj;
    }
}
