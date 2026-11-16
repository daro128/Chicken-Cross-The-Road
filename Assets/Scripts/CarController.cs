using UnityEngine;

namespace ChickenRush
{
    /// <summary>
    /// Moves one car along its lane and wraps it to the other side when it leaves the playable area.
    /// The car has a kinematic Rigidbody and a trigger BoxCollider; touching the chicken kills it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        private Rigidbody body;
        private float direction = 1f;
        private float speed = 3f;
        private float halfLength = 14f;
        private bool moving = true;

        public void Configure(int laneDirection, float laneSpeed, float laneHalfLength)
        {
            direction = laneDirection >= 0 ? 1f : -1f;
            speed = laneSpeed;
            halfLength = laneHalfLength;
        }

        public void SetMoving(bool value)
        {
            moving = value;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (!moving || body == null) return;

            Vector3 p = body.position;
            p.x += direction * speed * Time.fixedDeltaTime;

            bool wrapped = false;
            if (direction > 0f && p.x > halfLength) { p.x -= 2f * halfLength; wrapped = true; }
            else if (direction < 0f && p.x < -halfLength) { p.x += 2f * halfLength; wrapped = true; }

            if (wrapped)
            {
                body.position = p;      // teleport (off-screen) so there is no long interpolated sweep
                transform.position = p;
            }
            else
            {
                body.MovePosition(p);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryHit(other);
        }

        private static void TryHit(Collider other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null) player.Die();
        }
    }
}
