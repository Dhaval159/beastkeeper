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

        public enum FacingDirection { Down, Left, Right, Up }
        private FacingDirection currentFacingDirection = FacingDirection.Down;
        public FacingDirection CurrentFacingDirection => currentFacingDirection;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down;
        private PlayerInput playerInput;
        private Animator animator;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerInput = GetComponent<PlayerInput>();
            animator = GetComponent<Animator>();
            
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

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            if (animator == null) return;

            bool isDialogueActive = ServiceLocator.TryGet<IDialogueSystem>(out var dialogueSystem) && dialogueSystem.IsDialogueActive;
            
            if (isDialogueActive)
            {
                PlayIdleAnimation();
                return;
            }

            if (moveInput.sqrMagnitude > 0.01f)
            {
                // Determine facing direction based on dominant axis
                float absX = Mathf.Abs(moveInput.x);
                float absY = Mathf.Abs(moveInput.y);

                if (absX > absY)
                {
                    currentFacingDirection = moveInput.x > 0 ? FacingDirection.Right : FacingDirection.Left;
                    lastMoveDirection = moveInput.x > 0 ? Vector2.right : Vector2.left;
                }
                else
                {
                    currentFacingDirection = moveInput.y > 0 ? FacingDirection.Up : FacingDirection.Down;
                    lastMoveDirection = moveInput.y > 0 ? Vector2.up : Vector2.down;
                }

                PlayWalkAnimation();
            }
            else
            {
                PlayIdleAnimation();
            }
        }

        private void PlayWalkAnimation()
        {
            switch (currentFacingDirection)
            {
                case FacingDirection.Down:
                    animator.Play("Walk_Down");
                    break;
                case FacingDirection.Left:
                    animator.Play("Walk_Left");
                    break;
                case FacingDirection.Right:
                    animator.Play("Walk_Right");
                    break;
                case FacingDirection.Up:
                    animator.Play("Walk_Up");
                    break;
            }
        }

        private void PlayIdleAnimation()
        {
            switch (currentFacingDirection)
            {
                case FacingDirection.Down:
                    animator.Play("Idle_Down");
                    break;
                case FacingDirection.Left:
                    animator.Play("Idle_Left");
                    break;
                case FacingDirection.Right:
                    animator.Play("Idle_Right");
                    break;
                case FacingDirection.Up:
                    animator.Play("Idle_Up");
                    break;
            }
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
