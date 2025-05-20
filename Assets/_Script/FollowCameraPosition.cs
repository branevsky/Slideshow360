using UnityEngine;

public class FollowCameraPosition : MonoBehaviour
{
    public Transform cameraTransform;

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.position = cameraTransform.position;
        }
    }
}