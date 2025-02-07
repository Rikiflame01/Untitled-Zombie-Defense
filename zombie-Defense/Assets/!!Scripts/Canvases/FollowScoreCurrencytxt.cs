using UnityEngine;

public class FollowScoreCurrencytxt : MonoBehaviour
{
    [Header("Fallback (Default) Transform")]
    [Tooltip("If the ScoreCurrencytxt child cannot be found, the object will revert to this transform's position and rotation.")]
    public Transform fallbackTransform;

    private Transform scoreCurrencyTransform;

    private void FixedUpdate()
    {

        scoreCurrencyTransform = FindScoreCurrencytxtTransform();

        if (scoreCurrencyTransform != null)
        {

            transform.position = scoreCurrencyTransform.position;
            transform.rotation = scoreCurrencyTransform.rotation;
        }
        else
        {

            if (fallbackTransform != null)
            {
                transform.position = fallbackTransform.position;
                transform.rotation = fallbackTransform.rotation;
            }
        }
    }

    private Transform FindScoreCurrencytxtTransform()
    {

        GameObject buildPreviewObject = GameObject.FindGameObjectWithTag("BuildPreview");

        if (buildPreviewObject == null)
        {
            return null;
        }

        Transform scoreCurrency = buildPreviewObject.transform.Find("ScoreCurrencytxt");

        if (scoreCurrency == null)
        {
            Debug.LogError("ScoreCurrencytxt child not found under BuildPreview!");
        }

        return scoreCurrency;
    }
}
