using System;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject endGameCanvas;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth is Health health)
        {
            health.OnDied += EnableDeathCanvas;
        }
    }

    private void EnableDeathCanvas(GameObject @object)
    {
        if (@object.CompareTag("Player")){
            endGameCanvas.SetActive(true);
        }
    }
}
