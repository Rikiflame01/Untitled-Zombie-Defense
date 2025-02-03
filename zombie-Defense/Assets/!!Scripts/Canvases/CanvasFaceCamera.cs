using UnityEngine;

public class CanvasFaceCamera : MonoBehaviour
{
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;


        transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward, 
        targetCamera.transform.rotation * Vector3.up);
    }
}
