using System;
using System.Collections;
using UnityEngine;

public class playerDamaged : MonoBehaviour
{
    public GameObject damageObject;
    public CanvasGroup damagedGroup;

    private void Start()
    {
        ActionManager.OnPlayerDamaged += HandleDamage;

    }

    private void OnDisable()
    {
        ActionManager.OnPlayerDamaged -= HandleDamage;
    }

    private void HandleDamage()
    {
        damageObject.SetActive(true);
        damagedGroup.alpha = 1;
        StartCoroutine(DamageCanvasCoRoutine());
    }

    public IEnumerator DamageCanvasCoRoutine()
    {
        for (float f = 0; f <= 1; f += Time.deltaTime)
        {
            damagedGroup.alpha = Mathf.Lerp(1f, 0f, f / 1);
            yield return null;
        }
        damageObject.SetActive(false);
    }
}
