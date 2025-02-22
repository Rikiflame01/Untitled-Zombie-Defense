using System;
using System.Collections;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject endGameCanvas;
    public GameObject scorePanel;

    void Start()
    {
        if(scorePanel == null)
        {
            scorePanel = GameObject.Find("Score");
            if(scorePanel == null)
                Debug.LogWarning("Score canvas not found in the scene. Please ensure a GameObject named 'Score' exists.");
        }
        if(endGameCanvas == null)
        {
            endGameCanvas = GameObject.Find("EndGame");
            if(endGameCanvas == null)
                Debug.LogWarning("EndGame canvas not found in the scene. Please ensure a GameObject named 'EndGame' exists.");
        }

        GameObject player = GameObject.FindWithTag("Player");
        if(player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                Rigidbody rigidbody = player.GetComponent<Rigidbody>();
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                playerHealth.OnDied += EnableDeathCanvas;
            }
            else
            {
                Debug.LogWarning("Player does not have a Health component.");
            }
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player' not found in the scene.");
        }
        
        ActionManager.OnChooseCardEnd += EnableScoreCanvas;
    }

    private void EnableScoreCanvas()
    {
        SoundManager.Instance.PlaySFX("waveDone", 1f);
        SoundManager.Instance.SwitchToBuildingMode();
        if(scorePanel != null)
        {
            scorePanel.SetActive(true);
            StartCoroutine(DisableCanvas());
        }
    }

    private IEnumerator DisableCanvas()
    {
        yield return new WaitForSeconds(5.5f);
        if(scorePanel != null)
            scorePanel.SetActive(false);
    }

    private void EnableDeathCanvas(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            Cursor.visible = true;
            SoundManager.Instance.PlaySFX("playerDeath", 2f);
            ActionManager.PlayerDied();
            if(endGameCanvas != null)
                endGameCanvas.SetActive(true);
        }
    }

    void OnDisable()
    {
        ActionManager.OnDefenseStop -= EnableScoreCanvas;
    }
}
