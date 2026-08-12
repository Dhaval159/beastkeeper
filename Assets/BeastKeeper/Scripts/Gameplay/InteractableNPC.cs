using UnityEngine;
using BeastKeeper.Core;
using BeastKeeper.Data;
using BeastKeeper.Systems;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// Interaction component for NPCs that triggers DialogueData assets via the DialogueSystem
    /// and publishes an NPCInteractionEvent so quests can react without hard references.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractableNPC : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcName = "Npc";
        [SerializeField] private string npcId;
        [SerializeField] private DialogueData dialogueData;

        public DialogueData DialogueData => dialogueData;

        public void Configure(string id, string name, DialogueData dialogue)
        {
            npcId = id;
            npcName = name;
            dialogueData = dialogue;
        }

        /// <summary>
        /// Stable id used for quest objectives ("talk:&lt;npcId&gt;"). Falls back to the name.
        /// </summary>
        public string NpcId => string.IsNullOrEmpty(npcId) ? npcName : npcId;

        public void Interact()
        {
            if (dialogueData == null)
            {
                Debug.LogWarning($"[InteractableNPC] NPC '{npcName}' has no DialogueData assigned.");
            }
            else if (ServiceLocator.TryGet<IDialogueSystem>(out var dialogueSystem))
            {
                dialogueSystem.StartDialogue(dialogueData);
            }
            else
            {
                Debug.LogError("[InteractableNPC] DialogueSystem service not found in ServiceLocator.");
            }

            EventBus.Raise(new NPCInteractionEvent { NpcId = NpcId });
        }
    }
}
