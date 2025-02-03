using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        GameStateManager.Initialize();
        ActionManager.InvokeDefenseStart();
    }

}
