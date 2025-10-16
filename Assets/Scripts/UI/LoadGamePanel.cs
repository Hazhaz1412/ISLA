using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MadKnight.Save;
using UnityEngine.SceneManagement;

namespace MadKnight.UI
{
    /// <summary>
    /// Load Game Panel - Hiển thị 11 save slots (1 auto + 10 manual)
    /// </summary>
    public class LoadGamePanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject panelRoot;
        
        [Header("Save Slots")]
        [SerializeField] private SaveSlotUI autoSaveSlot;       // Slot đặc biệt cho auto save
        [SerializeField] private SaveSlotUI[] manualSlots;      // 10 slots cho manual saves
        
        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button deleteAllButton;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        
        private bool isVisible = false;
        private int pendingDeleteSlot = -1; // -1 = none, 0 = auto save, 1-10 = manual slots
        
        private void Awake()
        {
            // Get or create CanvasGroup
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Initial state - hidden
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
            
            // Setup event listeners
            SetupEventListeners();
        }
        
        private void Start()
        {
            // Force ẩn panel khi start (đảm bảo không đè lên main menu)
            Hide();
        }
        
        private void SetupEventListeners()
        {
            // Back button
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClick);
            }
            
            // Delete all button
            if (deleteAllButton != null)
            {
                deleteAllButton.onClick.AddListener(OnDeleteAllClick);
            }
            
            // Confirmation buttons
            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            }
            
            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(OnConfirmNo);
            }
            
            // Auto save slot
            if (autoSaveSlot != null)
            {
                autoSaveSlot.onLoadClicked += () => LoadFromAutoSave();
                autoSaveSlot.onDeleteClicked += () => ShowDeleteConfirm(0);
            }
            
            // Manual slots
            for (int i = 0; i < manualSlots.Length; i++)
            {
                if (manualSlots[i] != null)
                {
                    int slotIndex = i; // Capture for closure
                    manualSlots[i].onLoadClicked += () => LoadFromSlot(slotIndex + 1);
                    manualSlots[i].onDeleteClicked += () => ShowDeleteConfirm(slotIndex + 1);
                }
            }
        }
        
        #region Show/Hide Panel
        
        public void Show()
        {
            if (isVisible)
            {
                Debug.Log("[LoadGamePanel] Already visible, skipping...");
                return;
            }
            
            Debug.Log("[LoadGamePanel] Show() called - Starting...");
            
            try
            {
                isVisible = true;
                
                if (panelRoot != null)
                {
                    Debug.Log("[LoadGamePanel] Activating panel root...");
                    panelRoot.SetActive(true);
                }
                else
                {
                    Debug.LogError("[LoadGamePanel] Panel Root is NULL!");
                }
                
                // Refresh all slots
                Debug.Log("[LoadGamePanel] Starting RefreshAllSlots...");
                RefreshAllSlots();
                Debug.Log("[LoadGamePanel] RefreshAllSlots completed!");
                
                Debug.Log("[LoadGamePanel] Starting FadeIn coroutine...");
                StartCoroutine(FadeIn());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] CRITICAL ERROR in Show(): {e.Message}\n{e.StackTrace}");
            }
        }
        
        public void Hide()
        {
            if (!isVisible) return;
            
            Debug.Log("[LoadGamePanel] Hiding panel...");
            isVisible = false;
            StartCoroutine(FadeOut());
        }
        
        private IEnumerator FadeIn()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            
            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
        }
        
        private IEnumerator FadeOut()
        {
            canvasGroup.interactable = false;
            
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
        
        #endregion
        
        #region Refresh Slots
        
        private void RefreshAllSlots()
        {
            try
            {
                Debug.Log("[LoadGamePanel] RefreshAllSlots - Start");
                
                // Refresh auto save slot
                RefreshAutoSaveSlot();
                
                // Refresh manual slots
                if (manualSlots != null && manualSlots.Length > 0)
                {
                    Debug.Log($"[LoadGamePanel] Refreshing {manualSlots.Length} manual slots");
                    for (int i = 0; i < manualSlots.Length && i < 10; i++)
                    {
                        RefreshManualSlot(i + 1);
                    }
                }
                else
                {
                    Debug.LogWarning("[LoadGamePanel] Manual slots array is null or empty!");
                }
                
                Debug.Log("[LoadGamePanel] RefreshAllSlots - Complete");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] Error in RefreshAllSlots: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void RefreshAutoSaveSlot()
        {
            try
            {
                if (autoSaveSlot == null)
                {
                    Debug.LogWarning("[LoadGamePanel] Auto save slot is null!");
                    return;
                }
                
                Debug.Log("[LoadGamePanel] Refreshing auto save slot...");
                
                bool hasAutoSave = SaveSystem.HasAnySaveFile();
                Debug.Log($"[LoadGamePanel] Has auto save: {hasAutoSave}");
                
                if (hasAutoSave)
                {
                    MadKnight.Save.PlayerSaveData data = SaveSystem.LoadAutoSave();
                    if (data != null)
                    {
                        Debug.Log($"[LoadGamePanel] Auto save data loaded: {data.currentScene}");
                        autoSaveSlot.SetSlotData(
                            slotNumber: 0,
                            isEmpty: false,
                            saveTime: data.saveTime,
                            sceneName: data.currentScene,
                            playTime: data.playTimeSeconds,
                            level: data.playerLevel,
                            difficulty: data.difficulty
                        );
                    }
                    else
                    {
                        Debug.LogWarning("[LoadGamePanel] Auto save data is null");
                        autoSaveSlot.SetEmpty(0);
                    }
                }
                else
                {
                    Debug.Log("[LoadGamePanel] No auto save file found");
                    autoSaveSlot.SetEmpty(0);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] Error in RefreshAutoSaveSlot: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private void RefreshManualSlot(int slotNumber)
        {
            try
            {
                if (slotNumber < 1 || slotNumber > 10) return;
                
                int arrayIndex = slotNumber - 1;
                if (arrayIndex >= manualSlots.Length || manualSlots[arrayIndex] == null)
                {
                    Debug.LogWarning($"[LoadGamePanel] Manual slot {slotNumber} is null or out of range!");
                    return;
                }
                
                Debug.Log($"[LoadGamePanel] Refreshing manual slot {slotNumber}...");
                
                SaveSlotInfo info = SaveSystem.GetSlotInfo(slotNumber);
                
                if (info.isEmpty)
                {
                    Debug.Log($"[LoadGamePanel] Slot {slotNumber} is empty");
                    manualSlots[arrayIndex].SetEmpty(slotNumber);
                }
                else
                {
                    Debug.Log($"[LoadGamePanel] Slot {slotNumber} has data: {info.currentScene}");
                    manualSlots[arrayIndex].SetSlotData(
                        slotNumber: slotNumber,
                        isEmpty: false,
                        saveTime: info.saveTime,
                        sceneName: info.currentScene,
                        playTime: info.playTimeSeconds,
                        level: info.playerLevel,
                        difficulty: info.difficulty
                    );
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LoadGamePanel] Error refreshing slot {slotNumber}: {e.Message}\n{e.StackTrace}");
            }
        }
        
        #endregion
        
        #region Load/Delete Actions
        
        private void LoadFromAutoSave()
        {
            Debug.Log("[LoadGamePanel] Loading auto save...");
            
            MadKnight.Save.PlayerSaveData data = SaveSystem.LoadAutoSave();
            if (data != null)
            {
                // Load scene
                SceneManager.LoadScene(data.currentScene);
            }
            else
            {
                Debug.LogError("Failed to load auto save!");
            }
        }
        
        private void LoadFromSlot(int slotNumber)
        {
            Debug.Log($"[LoadGamePanel] Loading from slot {slotNumber}...");
            
            if (!SaveSystem.IsSlotUsed(slotNumber))
            {
                Debug.LogWarning($"Slot {slotNumber} is empty!");
                return;
            }
            
            PlayerSaveData data = SaveSystem.LoadFromSlot(slotNumber);
            if (data != null)
            {
                // Copy to auto save để tiếp tục chơi
                SaveSystem.CopySlotToAutoSave(slotNumber);
                
                // Load scene
                SceneManager.LoadScene(data.currentScene);
            }
            else
            {
                Debug.LogError($"Failed to load from slot {slotNumber}!");
            }
        }
        
        private void ShowDeleteConfirm(int slotNumber)
        {
            pendingDeleteSlot = slotNumber;
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(true);
                
                string slotName = slotNumber == 0 ? "Auto Save" : $"Slot {slotNumber}";
                if (confirmText != null)
                {
                    confirmText.text = $"Delete {slotName}?\nThis action cannot be undone.";
                }
            }
        }
        
        private void OnConfirmYes()
        {
            if (pendingDeleteSlot == 0)
            {
                // Delete auto save
                SaveSystem.DeleteAutoSave();
                RefreshAutoSaveSlot();
            }
            else if (pendingDeleteSlot >= 1 && pendingDeleteSlot <= 10)
            {
                // Delete manual slot
                SaveSystem.DeleteSlot(pendingDeleteSlot);
                RefreshManualSlot(pendingDeleteSlot);
            }
            
            pendingDeleteSlot = -1;
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }
        
        private void OnConfirmNo()
        {
            pendingDeleteSlot = -1;
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }
        
        private void OnDeleteAllClick()
        {
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(true);
                
                if (confirmText != null)
                {
                    confirmText.text = "Delete ALL saves?\nThis will delete auto save and all 10 slots!\nThis action cannot be undone.";
                }
                
                pendingDeleteSlot = -99; // Special code for delete all
            }
        }
        
        private void DeleteAllSaves()
        {
            SaveSystem.DeleteAllSaves();
            RefreshAllSlots();
            
            Debug.Log("[LoadGamePanel] All saves deleted!");
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnBackClick()
        {
            Hide();
        }
        
        #endregion
        
        #region Confirmation Dialog (Modified)
        
        private void OnConfirmYesModified()
        {
            if (pendingDeleteSlot == -99)
            {
                // Delete all
                DeleteAllSaves();
            }
            else if (pendingDeleteSlot == 0)
            {
                // Delete auto save
                SaveSystem.DeleteAutoSave();
                RefreshAutoSaveSlot();
            }
            else if (pendingDeleteSlot >= 1 && pendingDeleteSlot <= 10)
            {
                // Delete manual slot
                SaveSystem.DeleteSlot(pendingDeleteSlot);
                RefreshManualSlot(pendingDeleteSlot);
            }
            
            pendingDeleteSlot = -1;
            
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }
        
        #endregion
    }
}
