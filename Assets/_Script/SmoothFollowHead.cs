using UnityEngine;

public class SmoothFollowHead : MonoBehaviour
{
    public Transform cameraTransform; // MainCamera (OVRCameraRig ou XR Origin)
    public float followDistance = 2f;
    public float heightOffset = 0f;
    public float followSpeed = 2f;
    public float maxAngleOffset = 30f; // Só começa a ajustar se passar esse ângulo

    private Vector3 targetPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        UpdateTargetPosition(true);
    }

    void Update()
    {
        Vector3 directionToPlane = transform.position - cameraTransform.position;
        float angle = Vector3.Angle(cameraTransform.forward, directionToPlane);

        if (angle > maxAngleOffset)
        {
            UpdateTargetPosition();
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.LookAt(cameraTransform.position);
    }

    void UpdateTargetPosition(bool instant = false)
    {
        targetPosition = cameraTransform.position + cameraTransform.forward * followDistance + Vector3.up * heightOffset;

        if (instant)
            transform.position = targetPosition;

        transform.LookAt(cameraTransform.position);
    }
}