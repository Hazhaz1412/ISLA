using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MadKnight.UI
{
    /// <summary>
    /// UI component cho 1 save slot
    /// Hiển thị thông tin save và handle Load/Delete
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject emptyState;          // Hiển thị khi slot trống
        [SerializeField] private GameObject filledState;         // Hiển thị khi có save
        
        [Header("Filled State UI")]
        [SerializeField] private TextMeshProUGUI slotNumberText; // "Auto Save" hoặc "Slot 1"
        [SerializeField] private TextMeshProUGUI saveTimeText;   // "2024-01-15 14:30:00"
        [SerializeField] private TextMeshProUGUI sceneNameText;  // "Level01"
        [SerializeField] private TextMeshProUGUI playTimeText;   // "20h 34m"
        [SerializeField] private TextMeshProUGUI levelText;      // "Level 5"
        [SerializeField] private TextMeshProUGUI difficultyText; // "Normal"
        [SerializeField] private Image thumbnail;                // Screenshot (optional)
        
        [Header("Buttons")]
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        
        [Header("Colors")]
        [SerializeField] private Color autoSaveColor = new Color(1f, 0.8f, 0.2f); // Gold
        [SerializeField] private Color manualSaveColor = Color.white;
        [SerializeField] private Color emptyColor = new Color(0.5f, 0.5f, 0.5f);
        
        // Events
        public Action onLoadClicked;
        public Action onDeleteClicked;
        
        private int currentSlotNumber = -1;
        private bool isEmpty = true;
        
        private void Awake()
        {
            // Setup button listeners
            if (loadButton != null)
            {
                loadButton.onClick.AddListener(OnLoadClick);
            }
            
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(OnDeleteClick);
            }
            
            // Default to empty
            SetEmpty(0);
        }
        
        /// <summary>
        /// Set slot data khi có save
        /// </summary>
        public void SetSlotData(int slotNumber, bool isEmpty, string saveTime, string sceneName, 
                                float playTime, int level, int difficulty)
        {
            currentSlotNumber = slotNumber;
            this.isEmpty = isEmpty;
            
            // Show/hide states
            if (emptyState != null) emptyState.SetActive(isEmpty);
            if (filledState != null) filledState.SetActive(!isEmpty);
            
            if (!isEmpty)
            {
                // Slot number
                if (slotNumberText != null)
                {
                    slotNumberText.text = slotNumber == 0 ? "AUTO SAVE" : $"SLOT {slotNumber}";
                    slotNumberText.color = slotNumber == 0 ? autoSaveColor : manualSaveColor;
                }
                
                // Save time
                if (saveTimeText != null)
                {
                    saveTimeText.text = saveTime;
                }
                
                // Scene name
                if (sceneNameText != null)
                {
                    sceneNameText.text = sceneName;
                }
                
                // Play time
                if (playTimeText != null)
                {
                    playTimeText.text = FormatPlayTime(playTime);
                }
                
                // Level
                if (levelText != null)
                {
                    levelText.text = $"Level {level}";
                }
                
                // Difficulty
                if (difficultyText != null)
                {
                    difficultyText.text = GetDifficultyName(difficulty);
                }
                
                // Enable buttons
                if (loadButton != null) loadButton.interactable = true;
                if (deleteButton != null) deleteButton.interactable = true;
            }
        }
        
        /// <summary>
        /// Set slot empty
        /// </summary>
        public void SetEmpty(int slotNumber)
        {
            currentSlotNumber = slotNumber;
            isEmpty = true;
            
            if (emptyState != null) emptyState.SetActive(true);
            if (filledState != null) filledState.SetActive(false);
            
            // Disable buttons
            if (loadButton != null) loadButton.interactable = false;
            if (deleteButton != null) deleteButton.interactable = false;
        }
        
        private void OnLoadClick()
        {
            if (isEmpty) return;
            
            Debug.Log($"[SaveSlotUI] Load clicked for slot {currentSlotNumber}");
            onLoadClicked?.Invoke();
        }
        
        private void OnDeleteClick()
        {
            if (isEmpty) return;
            
            Debug.Log($"[SaveSlotUI] Delete clicked for slot {currentSlotNumber}");
            onDeleteClicked?.Invoke();
        }
        
        private string FormatPlayTime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            
            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }
            else
            {
                return $"{minutes}m";
            }
        }
        
        private string GetDifficultyName(int difficulty)
        {
            switch (difficulty)
            {
                case 0: return "Easy";
                case 1: return "Normal";
                case 2: return "Hard";
                case 3: return "Nightmare";
                default: return "Unknown";
            }
        }
    }
}
