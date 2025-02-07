using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.2f;
    [SerializeField] private float dampingSpeed = 1.0f;

    private float remainingShakeTime;
    private Vector3 originalPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (remainingShakeTime > 0)
        {
            transform.localPosition = originalPosition + Random.insideUnitSphere * shakeMagnitude;
            remainingShakeTime -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            remainingShakeTime = 0f;
            transform.localPosition = originalPosition;
        }
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        remainingShakeTime = shakeDuration;
    }

    public void ShakeCamera()
    {
        remainingShakeTime = shakeDuration;
    }
}
