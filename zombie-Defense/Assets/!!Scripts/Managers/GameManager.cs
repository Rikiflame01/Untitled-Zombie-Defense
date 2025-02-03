using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        GameStateManager.Initialize();
        StartCoroutine(StartGameCoRoutine());
    }

    private IEnumerator StartGameCoRoutine(){
        yield return new WaitForSeconds(1);
        ActionManager.InvokeDefenseStart();
    }



}
