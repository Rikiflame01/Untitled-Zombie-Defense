using UnityEngine;

public class HeightLimiter : MonoBehaviour
{
    private void FixedUpdate()
    {
        Vector3 position = transform.position;

        if (position.y > 0.99f)
        {
            position.y = 0.99f;
            transform.position = position;
        }
    }

}
