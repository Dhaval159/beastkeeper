using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastKeeper.Data
{
    [Serializable]
    public class DialogueNode
    {
        [SerializeField] private string speakerName;
        [SerializeField, TextArea(2, 5)] private string text;
        [SerializeField] private List<DialogueChoice> choices;

        public string SpeakerName => speakerName;
        public string Text => text;
        public IReadOnlyList<DialogueChoice> Choices => choices;
    }

    [Serializable]
    public class DialogueChoice
    {
        [SerializeField] private string choiceText;
        [SerializeField] private int nextNodeIndex; // Branching node index reference

        public string ChoiceText => choiceText;
        public int NextNodeIndex => nextNodeIndex;
    }

    /// <summary>
    /// ScriptableObject representing dialogue conversations.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "Beast Keeper/Data/Dialogue")]
    public class DialogueData : EntityData
    {
        [SerializeField] private List<DialogueNode> nodes;

        public IReadOnlyList<DialogueNode> Nodes => nodes;
    }
}
