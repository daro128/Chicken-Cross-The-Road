using UnityEngine;

namespace ChickenRush
{
    /// <summary>Fixed-angle camera that smoothly follows the chicken.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Range(20f, 80f)] private float pitch = 48f;
        [SerializeField, Range(-45f, 45f)] private float yaw = -18f;
        [SerializeField, Min(3f)] private float distance = 14f;
        [SerializeField, Min(0.1f)] private float followSpeed = 6f;
        [SerializeField, Range(0f, 1f)] private float sidewaysFollow = 0.6f;
        [SerializeField] private float lookAheadZ = 2.5f;

        private void Start()
        {
            Snap();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = DesiredPosition();
            float blend = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, blend);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        public void Snap()
        {
            if (target == null) return;
            transform.position = DesiredPosition();
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private Vector3 DesiredPosition()
        {
            Vector3 focus = new Vector3(target.position.x * sidewaysFollow, 0f, target.position.z + lookAheadZ);
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            return focus - rot * Vector3.forward * distance;
        }
    }
}
