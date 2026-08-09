using System;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Contract for running and branching dialogues in-game.
    /// </summary>
    public interface IDialogueSystem : IGameService
    {
        event Action<DialogueNode> OnNodeDisplayed;
        event Action OnDialogueEnded;

        void StartDialogue(DialogueData dialogue);
        void ChooseOption(int choiceIndex);
        void EndDialogue();
        bool IsDialogueActive { get; }
    }
}
