using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public EntityStats playerStats;
    public EntityStats wallStats;

    void Start()
    {
        playerStats.damage = 34;
        wallStats.health = 30;
        wallStats.maxHealth = 30;
        playerStats.isPiercing = false;
        GameStateManager.Initialize();

        ActionManager.OnCardChosen += ChosenCard;
        ActionManager.OnDefenseStart += DisableCurser;
        ActionManager.OnBuildStart += DisableCurser;
        ActionManager.OnPlayerDied += EnableCurser;
    }

    private void DisableCurser()
    {
        Cursor.visible = false;
    }
    private void EnableCurser()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        if (GameStateManager.CurrentState == GameStateManager.GameState.MainMenu || GameStateManager.CurrentState == GameStateManager.GameState.ChooseCard)
        {
            Cursor.visible = true;
        }

    }

    void OnDisable()
    {
        ActionManager.OnCardChosen -= ChosenCard;
        ActionManager.OnDefenseStart -= DisableCurser;
        ActionManager.OnBuildStart -= DisableCurser;
        ActionManager.OnPlayerDied -= EnableCurser;
    }

    public void StartGame(){
        StartCoroutine(StartGameCoRoutine());
    }

    private IEnumerator StartGameCoRoutine(){
        yield return new WaitForSeconds(1);
        ActionManager.InvokeDefenseStart();
    }

    public static void ExitGame(){
        Application.Quit();
    }

    public static void RestartGame(){
        SceneManager.LoadScene("GameScene");
    }

    #region Card Logic

    private void ChosenCard(string cardName)
    {
        switch(cardName)
        {
            case "Empty":
                Empty();
                break;
            case "HealPlayer":
                HealPlayer();
                break;
            case "RepairWalls":
                RepairWalls();
                break;
            case "FortifyWalls":
                FortifyWalls();
                break;
            case "IncreaseClipSize":
                IncreaseClipSize();
                break;
            case "IncreaseDamage":
                IncreaseDamage();
                break;
            case "Piercing":
                Piercing();
                break;
            case "Rifle":
                Rifle();
                break;
            default:
                Debug.LogWarning("Unknown card chosen: " + cardName);
                break;
        }
    }

    private void Empty(){
        Debug.Log("Empty logic executed");
        ActionManager.InvokeChooseCardEnd();
    }

    public static void HealPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                int healAmount = playerHealth.MaxHealth - playerHealth.CurrentHealth;
                playerHealth.Heal(healAmount);
                Debug.Log("Player healed to full health.");
            }
            else
            {
                Debug.LogWarning("Player does not have a Health component.");
            }
        }
        else
        {
            Debug.LogWarning("Player not found in the scene.");
        }
        ActionManager.InvokeChooseCardEnd();
    }

    public void RepairWalls()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            Health wallHealth = wall.GetComponent<Health>();
            if (wallHealth != null)
            {
                int healAmount = wallStats.maxHealth - wallHealth.CurrentHealth;
                wallHealth.Heal(healAmount);
                Debug.Log("Wall " + wall.name + " repaired to full health.");
            }
        }
        ActionManager.InvokeChooseCardEnd();
    }

    public void FortifyWalls()
    {
        wallStats.maxHealth += 25;

        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            Health wallHealth = wall.GetComponent<Health>();
            if (wallHealth != null)
            {
                int healAmount = wallStats.maxHealth - wallHealth.CurrentHealth;
                wallHealth.Heal(healAmount);
            }
        }
        ActionManager.InvokeChooseCardEnd();
    }

    private void IncreaseClipSize(){
        GameObject player = GameObject.FindWithTag("Player");
        PlayerReload playerReload = player.GetComponent<PlayerReload>();
        playerReload.maxShots += 5;
        ActionManager.InvokeChooseCardEnd();
    }

    private void IncreaseDamage(){
        Debug.Log("Increase dmg logic executed");
        playerStats.damage += 5;
        ActionManager.InvokeChooseCardEnd();
    }

    private void Piercing(){
        playerStats.isPiercing = true;
        Debug.Log("Piercing logic executed");
        ActionManager.InvokeChooseCardEnd();
    }
    private void Rifle(){
        Debug.Log("Rifle logic executed");
        playerStats.damage -= 19;

        GameObject player = GameObject.FindWithTag("Player");
        PlayerReload playerReload = player.GetComponent<PlayerReload>();
        PlayerControls playerControls = player.GetComponent<PlayerControls>();
        playerReload.maxShots += 21; // Rifle - pistol capacity
        playerControls.rifleEnabled = true;

        ActionManager.InvokeChooseCardEnd();
    }
    #endregion
}
