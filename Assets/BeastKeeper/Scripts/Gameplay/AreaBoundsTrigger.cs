using UnityEngine;
using BeastKeeper.Core;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// Component that updates the CameraController's active clamping boundaries when the Player
    /// enters this trigger, and publishes an AreaEnteredEvent so quests can react to entering
    /// an area (e.g. "enter:forest") without touching camera behaviour.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class AreaBoundsTrigger : MonoBehaviour
    {
        [SerializeField] private string areaName;
        [SerializeField] private string areaId;
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        /// <summary>
        /// Stable id used for quest objectives ("enter:&lt;areaId&gt;"). Falls back to the area name.
        /// </summary>
        public string AreaId => string.IsNullOrEmpty(areaId) ? areaName : areaId;

        private void Start()
        {
            // Ensure collider is set to IsTrigger
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            CameraController cameraController = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
            if (cameraController != null)
            {
                cameraController.SetBounds(minBounds, maxBounds);
                Debug.Log($"[AreaBoundsTrigger] Transitioned to area: '{areaName}'. Camera bounds set to Min: {minBounds}, Max: {maxBounds}");
            }

            EventBus.Raise(new AreaEnteredEvent { AreaId = AreaId });
        }
    }
}
