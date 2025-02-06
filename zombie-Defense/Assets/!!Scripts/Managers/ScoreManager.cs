using UnityEngine;
using System;
using TMPro;

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
        totalScoreText.text = "0";
        enemyDeathCount = 0;
        playerDamaged = false;
        roundStartTime = Time.time;
        UpdateUI();
    }

    private void EndRound()
    {
        roundDuration = Time.time - roundStartTime;
        UpdateUI();
    }

    private void OnPlayerDied(GameObject player)
    {
        playerDamaged = true;
        UpdateUI();
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
        UpdateUI();
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

    private void UpdateUI()
    {
        if (enemyDeathText != null)
            enemyDeathText.text = enemyDeathCount.ToString();

        if (roundTimeText != null)
            roundTimeText.text = roundDuration.ToString("F2") + "s";

        if (playerDamageText != null)
            playerDamageText.text = (playerDamaged ? "Yes - 1 " : "No + 3");

        int damagePenalty = playerDamaged ? -1 : 3;
        int enemyScore = enemyDeathCount;
        float timePenalty = Mathf.Max(0, roundDuration / 5);

        totalScore = Mathf.RoundToInt(damagePenalty + enemyScore * 1.5f - timePenalty);
        
        UpdateScoreUI();
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
