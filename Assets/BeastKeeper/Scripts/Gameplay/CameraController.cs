using UnityEngine;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// Smooth follow camera targeting the player, preventing jitter.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Bounds Settings")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        private void Start()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null)
            {
                Vector3 targetPosition = target.position + offset;
                if (useBounds)
                {
                    targetPosition = ClampToBounds(targetPosition);
                }
                transform.position = targetPosition;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            if (useBounds)
            {
                targetPosition = ClampToBounds(targetPosition);
            }
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }

        private Vector3 ClampToBounds(Vector3 targetPos)
        {
            Camera cam = GetComponent<Camera>();
            if (cam == null) return targetPos;

            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float clampedX = Mathf.Clamp(targetPos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
            float clampedY = Mathf.Clamp(targetPos.y, minBounds.y + camHeight, maxBounds.y - camHeight);

            // If bounds are smaller than camera viewport, keep it centered
            if (maxBounds.x - minBounds.x < camWidth * 2f)
                clampedX = (minBounds.x + maxBounds.x) / 2f;
            if (maxBounds.y - minBounds.y < camHeight * 2f)
                clampedY = (minBounds.y + maxBounds.y) / 2f;

            return new Vector3(clampedX, clampedY, targetPos.z);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;

            // Immediately apply clamp to prevent single frame snap visual jumps
            if (target != null)
            {
                transform.position = ClampToBounds(target.position + offset);
            }
        }
    }
}
