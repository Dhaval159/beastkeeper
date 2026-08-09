using UnityEngine;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// A temporary NPC interaction component that logs a message to the console.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractableNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "Npc";
        [SerializeField, TextArea(2, 4)] private string interactionMessage = "Hello, Beast Keeper.";

        public void Interact()
        {
            Debug.Log($"[{npcName}]: {interactionMessage}");
        }
    }
}
