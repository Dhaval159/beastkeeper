using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BeastKeeper.Core;
using BeastKeeper.Systems;
using BeastKeeper.Data;
using System.Collections;
using System.Collections.Generic;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// UI Manager that displays dialogue text, speaker names, continue indicators, and branching choices.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        private IDialogueSystem dialogueSystem;
        private List<Button> activeButtons = new List<Button>();
        private bool hasChoices = false;

        private void Start()
        {
            // Register callbacks when service becomes available
            if (ServiceLocator.TryGet<IDialogueSystem>(out dialogueSystem))
            {
                SubscribeToEvents();
            }
            else
            {
                StartCoroutine(DeferredInit());
            }

            // Start closed
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private IEnumerator DeferredInit()
        {
            while (dialogueSystem == null)
            {
                if (ServiceLocator.TryGet<IDialogueSystem>(out dialogueSystem))
                {
                    SubscribeToEvents();
                    break;
                }
                yield return null;
            }
        }

        private void SubscribeToEvents()
        {
            dialogueSystem.OnNodeDisplayed += DisplayNode;
            dialogueSystem.OnDialogueEnded += HideDialogue;
        }

        private void OnDestroy()
        {
            if (dialogueSystem != null)
            {
                dialogueSystem.OnNodeDisplayed -= DisplayNode;
                dialogueSystem.OnDialogueEnded -= HideDialogue;
            }
        }

        private void Update()
        {
            if (dialogueSystem != null && dialogueSystem.IsDialogueActive)
            {
                // Continue on pressing E, Space or Enter
                // But only if we are not currently displaying branching choices!
                if (!hasChoices)
                {
                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    {
                        OnContinuePressed();
                    }
                }
            }
        }

        private void DisplayNode(DialogueNode node)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            if (speakerNameText != null) speakerNameText.text = node.SpeakerName;
            if (dialogueText != null) dialogueText.text = node.Text;

            // Clear previous choices
            ClearChoices();

            // Display Choices if any
            if (node.Choices != null && node.Choices.Count > 0)
            {
                hasChoices = true;
                if (continueIndicator != null) continueIndicator.SetActive(false);
                if (choicesContainer != null) choicesContainer.gameObject.SetActive(true);

                for (int i = 0; i < node.Choices.Count; i++)
                {
                    int index = i;
                    var choice = node.Choices[i];
                    
                    if (choiceButtonPrefab != null && choicesContainer != null)
                    {
                        GameObject buttonGo = Instantiate(choiceButtonPrefab, choicesContainer);
                        Button btn = buttonGo.GetComponent<Button>();
                        TMP_Text btnText = buttonGo.GetComponentInChildren<TMP_Text>();

                        if (btnText != null) btnText.text = choice.ChoiceText;
                        if (btn != null)
                        {
                            btn.onClick.AddListener(() => OnChoiceSelected(index));
                            activeButtons.Add(btn);
                        }
                    }
                }
            }
            else
            {
                hasChoices = false;
                if (continueIndicator != null) continueIndicator.SetActive(true);
                if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
            }
        }

        private void OnChoiceSelected(int index)
        {
            if (dialogueSystem != null)
            {
                dialogueSystem.ChooseOption(index);
            }
        }

        public void OnContinuePressed()
        {
            if (dialogueSystem != null && !hasChoices)
            {
                dialogueSystem.ChooseOption(-1);
            }
        }

        private void HideDialogue()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            ClearChoices();
        }

        private void ClearChoices()
        {
            foreach (var btn in activeButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }
            activeButtons.Clear();
            hasChoices = false;
        }
    }
}
