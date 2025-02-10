using System;
using System.Collections;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject endGameCanvas;
    public GameObject scorePanel;
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth is Health health)
        {
            health.OnDied += EnableDeathCanvas;
        }
        ActionManager.OnDefenseStop += EnableScoreCanvas;
    }

    private void EnableScoreCanvas()
    {
        SoundManager.Instance.PlaySFX("waveDone",1f);
        SoundManager.Instance.SwitchToBuildingMode();
        scorePanel.SetActive(true);
        StartCoroutine(DisableCanvas());
    }

    private IEnumerator DisableCanvas(){
        yield return new WaitForSeconds(5);
        scorePanel.SetActive(false);
    }

    private void EnableDeathCanvas(GameObject @object)
    {
        if (@object.CompareTag("Player")){
            SoundManager.Instance.PlaySFX("playerDeath",1f);
            endGameCanvas.SetActive(true);
        }
    }

    void OnDisable()
    {
        ActionManager.OnDefenseStop -= EnableScoreCanvas;
    }
}
