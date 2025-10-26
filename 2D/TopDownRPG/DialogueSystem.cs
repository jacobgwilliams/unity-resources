using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityTemplates.TopDownRPG
{
    [CreateAssetMenu(fileName = "DialogueData", menuName = "Unity Templates/TopDownRPG/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [Header("Dialogue Info")]
        public string dialogueName;
        public List<DialogueNode> nodes = new List<DialogueNode>();
        
        [System.Serializable]
        public class DialogueNode
        {
            public string nodeId;
            public string speakerName;
            [TextArea(3, 5)]
            public string dialogueText;
            public Sprite speakerPortrait;
            public List<DialogueChoice> choices = new List<DialogueChoice>();
            public bool isEndNode = false;
            public string nextNodeId;
            
            [Header("Events")]
            public string onEnterEvent;
            public string onExitEvent;
        }
        
        [System.Serializable]
        public class DialogueChoice
        {
            public string choiceText;
            public string nextNodeId;
            public bool isAvailable = true;
            public string conditionEvent;
        }
    }
    
    public class DialogueSystem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialogueUI;
        [SerializeField] private UnityEngine.UI.Text speakerNameText;
        [SerializeField] private UnityEngine.UI.Text dialogueText;
        [SerializeField] private UnityEngine.UI.Image speakerPortrait;
        [SerializeField] private Transform choicesParent;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private UnityEngine.UI.Button continueButton;
        
        [Header("Settings")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool autoAdvance = false;
        [SerializeField] private float autoAdvanceDelay = 2f;
        
        // Private fields
        private DialogueData currentDialogue;
        private DialogueData.DialogueNode currentNode;
        private bool isDialogueActive = false;
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private Dictionary<string, DialogueData.DialogueNode> nodeDictionary = new Dictionary<string, DialogueData.DialogueNode>();
        
        // Events
        public event Action<DialogueData> OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<string> OnDialogueEvent;
        public event Action<DialogueData.DialogueNode> OnNodeChanged;
        
        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueDialogue);
            }
        }
        
        private void Update()
        {
            if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    SkipTyping();
                }
                else
                {
                    ContinueDialogue();
                }
            }
        }
        
        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null || dialogue.nodes.Count == 0) return;
            
            currentDialogue = dialogue;
            BuildNodeDictionary();
            
            // Start with first node
            if (dialogue.nodes.Count > 0)
            {
                SetCurrentNode(dialogue.nodes[0]);
            }
            
            isDialogueActive = true;
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(true);
            }
            
            OnDialogueStarted?.Invoke(dialogue);
        }
        
        public void EndDialogue()
        {
            isDialogueActive = false;
            isTyping = false;
            
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            if (dialogueUI != null)
            {
                dialogueUI.SetActive(false);
            }
            
            ClearChoices();
            OnDialogueEnded?.Invoke();
        }
        
        private void BuildNodeDictionary()
        {
            nodeDictionary.Clear();
            foreach (var node in currentDialogue.nodes)
            {
                nodeDictionary[node.nodeId] = node;
            }
        }
        
        private void SetCurrentNode(DialogueData.DialogueNode node)
        {
            currentNode = node;
            
            // Update UI
            if (speakerNameText != null)
            {
                speakerNameText.text = node.speakerName;
            }
            
            if (speakerPortrait != null && node.speakerPortrait != null)
            {
                speakerPortrait.sprite = node.speakerPortrait;
            }
            
            // Clear previous choices
            ClearChoices();
            
            // Show choices or continue button
            if (node.choices.Count > 0)
            {
                ShowChoices(node.choices);
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(!node.isEndNode);
                }
                StartTyping(node.dialogueText);
            }
            
            // Trigger events
            if (!string.IsNullOrEmpty(node.onEnterEvent))
            {
                OnDialogueEvent?.Invoke(node.onEnterEvent);
            }
            
            OnNodeChanged?.Invoke(node);
        }
        
        private void StartTyping(string text)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            typingCoroutine = StartCoroutine(TypeText(text));
        }
        
        private System.Collections.IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";
            
            foreach (char letter in text)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }
            
            isTyping = false;
            
            if (autoAdvance && currentNode.choices.Count == 0 && !currentNode.isEndNode)
            {
                yield return new WaitForSeconds(autoAdvanceDelay);
                ContinueDialogue();
            }
        }
        
        private void SkipTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            dialogueText.text = currentNode.dialogueText;
            isTyping = false;
        }
        
        private void ShowChoices(List<DialogueData.DialogueChoice> choices)
        {
            if (choicesParent == null || choiceButtonPrefab == null) return;
            
            foreach (var choice in choices)
            {
                if (!choice.isAvailable) continue;
                
                GameObject choiceButton = Instantiate(choiceButtonPrefab, choicesParent);
                var button = choiceButton.GetComponent<UnityEngine.UI.Button>();
                var text = choiceButton.GetComponentInChildren<UnityEngine.UI.Text>();
                
                if (text != null)
                {
                    text.text = choice.choiceText;
                }
                
                if (button != null)
                {
                    button.onClick.AddListener(() => SelectChoice(choice));
                }
            }
        }
        
        private void SelectChoice(DialogueData.DialogueChoice choice)
        {
            if (string.IsNullOrEmpty(choice.nextNodeId))
            {
                EndDialogue();
                return;
            }
            
            if (nodeDictionary.ContainsKey(choice.nextNodeId))
            {
                SetCurrentNode(nodeDictionary[choice.nextNodeId]);
            }
            else
            {
                Debug.LogWarning($"Dialogue node '{choice.nextNodeId}' not found!");
                EndDialogue();
            }
        }
        
        private void ContinueDialogue()
        {
            if (isTyping) return;
            
            if (currentNode.isEndNode)
            {
                EndDialogue();
                return;
            }
            
            if (!string.IsNullOrEmpty(currentNode.nextNodeId))
            {
                if (nodeDictionary.ContainsKey(currentNode.nextNodeId))
                {
                    SetCurrentNode(nodeDictionary[currentNode.nextNodeId]);
                }
                else
                {
                    Debug.LogWarning($"Dialogue node '{currentNode.nextNodeId}' not found!");
                    EndDialogue();
                }
            }
            else
            {
                EndDialogue();
            }
        }
        
        private void ClearChoices()
        {
            if (choicesParent == null) return;
            
            foreach (Transform child in choicesParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        public bool IsDialogueActive => isDialogueActive;
        public DialogueData.DialogueNode CurrentNode => currentNode;
    }
}