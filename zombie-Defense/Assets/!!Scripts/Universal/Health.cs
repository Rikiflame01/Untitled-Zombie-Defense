#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine;
using System;

public interface IHealth
{
    void TakeDamage(int damage);
    void Heal(int amount);
    int CurrentHealth { get; }
    int MaxHealth { get; }

    bool IsDead { get; }

    event Action<int, int> OnHealthChanged;

    event Action<GameObject> OnDied;
}

public class Health : MonoBehaviour, IHealth
{

    public event Action<int, int> OnHealthChanged;

    public event Action OnHealthDepleted;

    public event Action<GameObject> OnDied;

    [Header("Entity Stats")]
    [SerializeField] private EntityStats entityStats;

    [Header("Floating Damage")]
    [Tooltip("Floating damage prefab to display damage taken.")]
    [SerializeField] private GameObject floatingDamagePrefab;

    public int currentHealth;

    public int MaxHealth => entityStats != null ? entityStats.maxHealth : 0;
    public int CurrentHealth => currentHealth;

    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        if (entityStats == null)
        {
            Debug.LogWarning("No EntityStats assigned to " + gameObject.name);
            return;
        }

        currentHealth = entityStats.health;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || entityStats == null || IsDead == true) return;

        if(entityStats.isPlayer == true){
            CameraShake.Instance.ShakeCamera();
        }
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        if (floatingDamagePrefab != null)
        {
            ShowFloatingDamage(damage);
        }

        if (currentHealth <= 0)
        {
            OnHealthDepleted?.Invoke();
            HandleDeath();
        }
    }

    private void ShowFloatingDamage(int damage)
    {
        GameObject damageTextInstance = Instantiate(floatingDamagePrefab, transform.position, Quaternion.identity);

        FloatingDamage floatingDamage = damageTextInstance.GetComponent<FloatingDamage>();
        if (floatingDamage != null)
        {
            floatingDamage.SetDamageText(damage);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || entityStats == null) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        if (currentHealth > MaxHealth)
        {
            currentHealth = MaxHealth;
        }
        entityStats.health = currentHealth;

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    protected virtual void HandleDeath()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Enemy")){
        ScoreManager.Instance.OnEnemyDeath(gameObject);
        }
        OnDied?.Invoke(gameObject);
    }

    public void EditorDamage(int damage)
    {
        TakeDamage(damage);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Health))]
public class HealthEditor : Editor
{
    private float damageAmount = 10f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Health health = (Health)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Options", EditorStyles.boldLabel);

        damageAmount = EditorGUILayout.FloatField("Damage Amount", damageAmount);

        if (GUILayout.Button("Apply Damage"))
        {
            health.EditorDamage((int)damageAmount);
            EditorUtility.SetDirty(health);
        }
    }
}
#endif
