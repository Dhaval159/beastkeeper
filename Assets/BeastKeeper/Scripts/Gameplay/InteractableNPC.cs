using UnityEngine;
using BeastKeeper.Core;
using BeastKeeper.Data;
using BeastKeeper.Systems;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// Interaction component for NPCs that triggers DialogueData assets via the DialogueSystem.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractableNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "Npc";
        [SerializeField] private DialogueData dialogueData;

        public void Interact()
        {
            if (dialogueData == null)
            {
                Debug.LogWarning($"[InteractableNPC] NPC '{npcName}' has no DialogueData assigned.");
                return;
            }

            if (ServiceLocator.TryGet<IDialogueSystem>(out var dialogueSystem))
            {
                dialogueSystem.StartDialogue(dialogueData);
            }
            else
            {
                Debug.LogError("[InteractableNPC] DialogueSystem service not found in ServiceLocator.");
            }
        }
    }
}
