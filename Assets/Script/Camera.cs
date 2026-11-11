using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // Drag your player chicken here
    public Vector3 offset = new Vector3(-5f, 10f, -5f); // Isometric camera offset
    public float smoothSpeed = 5f;  // How fast the camera catches up

    private float maxZ = 0f;        // Tracks the highest forward position of the chicken

    void LateUpdate()
    {
        if (target == null) return;

        // Only track forward progress (Z-axis). If the chicken moves backward, maxZ won't decrease.
        if (target.position.z > maxZ)
        {
            maxZ = target.position.z;
        }

        // Calculate the target position based on the highest Z progress
        // Keeping X and Y locked relative to the offset so the camera doesn't drift sideways
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, maxZ + offset.z);

        // Smoothly interpolate (lerp) from current camera position to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}