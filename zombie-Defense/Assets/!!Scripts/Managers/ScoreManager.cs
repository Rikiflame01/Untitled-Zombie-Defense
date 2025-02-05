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

    private void UpdateUI()
    {
        if (enemyDeathText != null)
            enemyDeathText.text = enemyDeathCount.ToString();

        if (roundTimeText != null)
            roundTimeText.text = roundDuration.ToString("F2") + "s";

        if (playerDamageText != null)
            playerDamageText.text = (playerDamaged ? "Yes - 1 " : "No + 3");

        if (totalScoreText != null)
        {
            
            int damagePenalty = playerDamaged ? -1 : 3;
            int enemyScore = enemyDeathCount;
            float timePenalty = Mathf.Max(0, roundDuration / 5);

            float totalScore = damagePenalty + enemyScore*1.5f - timePenalty;

            totalScoreText.text = totalScore.ToString("F0");
        }
    }
}
