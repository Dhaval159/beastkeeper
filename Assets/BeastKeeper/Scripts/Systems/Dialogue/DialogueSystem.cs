using System;
using UnityEngine;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Runtime implementation of the dialogue system, managing state and notifications.
    /// </summary>
    public class DialogueSystem : MonoBehaviour, IDialogueSystem
    {
        public event Action<DialogueNode> OnNodeDisplayed;
        public event Action OnDialogueEnded;

        private DialogueData currentDialogue;
        private int currentNodeIndex = -1;
        private bool isActive = false;

        public bool IsDialogueActive => isActive;

        private void Awake()
        {
            ServiceLocator.Register<IDialogueSystem>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IDialogueSystem>();
        }

        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.Nodes == null || dialogue.Nodes.Count == 0)
            {
                Debug.LogWarning("[DialogueSystem] Tried to start dialogue with empty dialogue data.");
                return;
            }

            currentDialogue = dialogue;
            currentNodeIndex = 0;
            isActive = true;
            
            DisplayCurrentNode();
        }

        public void ChooseOption(int choiceIndex)
        {
            if (!isActive || currentDialogue == null) return;

            var currentNode = currentDialogue.Nodes[currentNodeIndex];
            
            // If the current node doesn't have choices, choiceIndex is treated as an advance command
            if (currentNode.Choices == null || currentNode.Choices.Count == 0)
            {
                AdvanceDialogue();
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= currentNode.Choices.Count)
            {
                Debug.LogWarning($"[DialogueSystem] Choice index {choiceIndex} is out of bounds for the current node.");
                return;
            }

            var choice = currentNode.Choices[choiceIndex];
            int nextIndex = choice.NextNodeIndex;

            if (nextIndex >= 0 && nextIndex < currentDialogue.Nodes.Count)
            {
                currentNodeIndex = nextIndex;
                DisplayCurrentNode();
            }
            else
            {
                // Ending path
                EndDialogue();
            }
        }

        public void EndDialogue()
        {
            if (!isActive) return;

            string dialogueId = currentDialogue != null ? currentDialogue.IdOrAssetName : string.Empty;

            isActive = false;
            currentDialogue = null;
            currentNodeIndex = -1;

            OnDialogueEnded?.Invoke();
            if (!string.IsNullOrEmpty(dialogueId))
            {
                EventBus.Raise(new DialogueCompletedEvent { DialogueId = dialogueId });
            }
        }

        private void AdvanceDialogue()
        {
            int nextIndex = currentNodeIndex + 1;
            if (nextIndex >= 0 && nextIndex < currentDialogue.Nodes.Count)
            {
                currentNodeIndex = nextIndex;
                DisplayCurrentNode();
            }
            else
            {
                EndDialogue();
            }
        }

        private void DisplayCurrentNode()
        {
            if (currentNodeIndex >= 0 && currentNodeIndex < currentDialogue.Nodes.Count)
            {
                OnNodeDisplayed?.Invoke(currentDialogue.Nodes[currentNodeIndex]);
            }
        }
    }
}
