using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Start()
    {
        GameStateManager.Initialize();
        StartCoroutine(StartGameCoRoutine());

        ActionManager.OnCardChosen += ChosenCard;
    }
    void OnDisable()
    {
        ActionManager.OnCardChosen -= ChosenCard;
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

    private void ChosenCard(string cardName){{
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
    }}

    private void Empty(){
        Debug.Log("empty logic executed");
        ActionManager.InvokeChooseCardEnd();
    }

    private void HealPlayer(){
        Debug.Log("heal player logic executed");
        ActionManager.InvokeChooseCardEnd();
    }
    private void RepairWalls(){
        Debug.Log("repair wall logic executed");
    }    
    private void FortifyWalls(){
        Debug.Log("fortify walls logic executed");
    }

    private void IncreaseClipSize(){
        Debug.Log("Increase CS logic executed");
    }
    private void IncreaseDamage(){
        Debug.Log("Increase dmg logic executed");
    }
    private void Piercing(){
        Debug.Log("piercing logic executed");
    }
    private void Rifle(){
        Debug.Log("rifle logic executed");
    }
    #endregion
}
