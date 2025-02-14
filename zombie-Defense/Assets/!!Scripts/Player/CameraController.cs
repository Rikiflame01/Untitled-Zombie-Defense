using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CameraController : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Canvas healthCanvas;
    public GameObject MainMenu;

    [Header("Camera Follow Settings")]
    public Transform target;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    private Coroutine smoothYCoroutine;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
    }
    void Awake()
    {
        DeactivateHealthCanvas();
        BuildMode();
    }
    void Start()
    {
        ActionManager.OnBuildStart += BuildMode;
        ActionManager.OnDefenseStart += DefenseMode;
    }

    void OnDisable()
    {
        ActionManager.OnBuildStart -= BuildMode;
        ActionManager.OnDefenseStart -= DefenseMode;
    }

    public void ActivateHealthCanvas()
    {
        if (healthCanvas != null)
            healthCanvas.gameObject.SetActive(true);
    }

    public void DeactivateHealthCanvas()
    {
        if (healthCanvas != null)
            healthCanvas.gameObject.SetActive(false);
    }

    public void DeactivateMainMenu()
    {
        if (MainMenu != null)
            MainMenu.SetActive(false);
    }

    public void BuildMode(){
        ChangeCameraYPosition(25f, 0.5f);
    }
    public void DefenseMode(){
        ChangeCameraYPosition(15f, 0.5f);
    }

    public void ChangeCameraYPosition(float targetY, float duration)
    {
        if (smoothYCoroutine != null)
            StopCoroutine(smoothYCoroutine);
        smoothYCoroutine = StartCoroutine(SmoothSetYCoroutine(targetY, duration));
    }

    private IEnumerator SmoothSetYCoroutine(float targetY, float duration)
    {
        float startY = transform.position.y;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startY, targetY, elapsed / duration);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }
}
