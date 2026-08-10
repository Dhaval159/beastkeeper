using UnityEngine;
using UnityEngine.InputSystem;
using BeastKeeper.Core;
using BeastKeeper.Systems;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// Handles player movement using Unity's modern Input System and Rigidbody2D.
    /// Performs raycast/overlap checks to interact with IInteractable objects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 1.2f;
        [SerializeField] private LayerMask interactableLayer;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down;
        private PlayerInput playerInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerInput = GetComponent<PlayerInput>();
            
            // Configure Rigidbody2D for top-down 2D physics
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                var moveAction = playerInput.actions.FindAction("Move");
                if (moveAction != null)
                {
                    moveAction.performed += OnMovePerformed;
                    moveAction.canceled += OnMoveCanceled;
                }

                var interactAction = playerInput.actions.FindAction("Interact");
                if (interactAction != null)
                {
                    interactAction.started += OnInteractStarted;
                }
            }
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                var moveAction = playerInput.actions.FindAction("Move");
                if (moveAction != null)
                {
                    moveAction.performed -= OnMovePerformed;
                    moveAction.canceled -= OnMoveCanceled;
                }

                var interactAction = playerInput.actions.FindAction("Interact");
                if (interactAction != null)
                {
                    interactAction.started -= OnInteractStarted;
                }
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = moveInput.normalized;
            }
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = Vector2.zero;
        }

        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            if (ServiceLocator.TryGet<IDialogueSystem>(out var dialogueSystem) && dialogueSystem.IsDialogueActive)
            {
                return;
            }
            TryInteract();
        }

        private void FixedUpdate()
        {
            if (ServiceLocator.TryGet<IDialogueSystem>(out var dialogueSystem) && dialogueSystem.IsDialogueActive)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // Smooth physics-based movement using Rigidbody2D.linearVelocity
            rb.linearVelocity = moveInput * moveSpeed;
        }

        private void TryInteract()
        {
            // Find interactable objects nearby in the direction the player is facing
            Vector2 scanCenter = (Vector2)transform.position + lastMoveDirection * 0.5f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(scanCenter, interactionRadius, interactableLayer);
            
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.TryGetComponent<IInteractable>(out var interactable))
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            if (closestInteractable != null)
            {
                closestInteractable.Interact();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector2 scanCenter = (Vector2)transform.position + lastMoveDirection * 0.5f;
            Gizmos.DrawWireSphere(scanCenter, interactionRadius);
        }
    }
}
