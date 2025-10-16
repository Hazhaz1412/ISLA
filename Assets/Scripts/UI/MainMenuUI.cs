using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using MadKnight.Save;

namespace MadKnight.UI
{
    /// <summary>
    /// Quản lý UI của Main Menu với hiệu ứng fade in
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private CanvasGroup menuButtonsGroup;
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private LoadGamePanel loadGamePanel;
        [SerializeField] private CanvasGroup creditsPanel;
        
        [Header("Menu Buttons")]
        [SerializeField] private MenuButton newGameButton;
        [SerializeField] private MenuButton continueButton;
        [SerializeField] private MenuButton settingsButton;
        [SerializeField] private MenuButton creditsButton;
        [SerializeField] private MenuButton exitButton;
        
        [Header("Animation Settings")]
        [SerializeField] private float titleFadeInDuration = 2f;
        [SerializeField] private float subtitleFadeInDuration = 1.5f;
        [SerializeField] private float buttonsFadeInDuration = 1f;
        [SerializeField] private float delayBetweenElements = 0.5f;
        
        [Header("Background")]
        [SerializeField] private AudioSource backgroundMusic;
        [SerializeField] private float musicFadeInDuration = 3f;
        
        private void Start()
        {
            // Khởi tạo alpha = 0 cho tất cả
            SetAlpha(titleText, 0);
            SetAlpha(subtitleText, 0);
            if (menuButtonsGroup != null) menuButtonsGroup.alpha = 0;
            if (creditsPanel != null) creditsPanel.alpha = 0;
            
            // Kiểm tra save file và disable Continue button nếu cần
            CheckSaveFileStatus();
            
            // Bắt đầu animation fade in
            StartCoroutine(FadeInSequence());
            
            // Fade in background music
            if (backgroundMusic != null)
            {
                StartCoroutine(FadeInMusic());
            }
        }
        
        private void CheckSaveFileStatus()
        {
            bool hasSaveFile = SaveSystem.HasAnySaveFile();
            
            Debug.Log($"[MainMenuUI] Checking save file status: {hasSaveFile}");
            
            if (continueButton != null)
            {
                continueButton.SetInteractable(hasSaveFile);
            }
        }
        
        private IEnumerator FadeInSequence()
        {
            // Fade in Title
            yield return StartCoroutine(FadeInText(titleText, titleFadeInDuration));
            
            yield return new WaitForSeconds(delayBetweenElements);
            
            // Fade in Subtitle
            yield return StartCoroutine(FadeInText(subtitleText, subtitleFadeInDuration));
            
            yield return new WaitForSeconds(delayBetweenElements);
            
            // Fade in Buttons
            if (menuButtonsGroup != null)
            {
                yield return StartCoroutine(FadeInCanvasGroup(menuButtonsGroup, buttonsFadeInDuration));
            }
        }
        
        private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
        {
            if (text == null) yield break;
            
            float elapsed = 0f;
            Color color = text.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                color.a = alpha;
                text.color = color;
                yield return null;
            }
            
            color.a = 1f;
            text.color = color;
        }
        
        private IEnumerator FadeInCanvasGroup(CanvasGroup group, float duration)
        {
            if (group == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            
            group.alpha = 1f;
        }
        
        private IEnumerator FadeInMusic()
        {
            float elapsed = 0f;
            float targetVolume = backgroundMusic.volume;
            backgroundMusic.volume = 0f;
            backgroundMusic.Play();
            
            while (elapsed < musicFadeInDuration)
            {
                elapsed += Time.deltaTime;
                backgroundMusic.volume = Mathf.Lerp(0f, targetVolume, elapsed / musicFadeInDuration);
                yield return null;
            }
            
            backgroundMusic.volume = targetVolume;
        }
        
        private void SetAlpha(TextMeshProUGUI text, float alpha)
        {
            if (text == null) return;
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
        
        // Menu Button Actions
        public void OnNewGameClick()
        {
            Debug.Log("[MainMenuUI] New Game clicked!");
            
            // Tạo auto save mới (luôn tạo mới khi New Game)
            SaveSystem.CreateAutoSave();
            Debug.Log("[MainMenuUI] Auto save created, loading Level01...");
            
            // Fade out và load scene
            StartCoroutine(LoadSceneWithFade("Level01"));
        }
        
        public void OnContinueClick()
        {
            Debug.Log("[MainMenuUI] Continue clicked!");
            
            try
            {
                if (!SaveSystem.HasAnySaveFile())
                {
                    Debug.LogWarning("[MainMenuUI] Không có file save!");
                    return;
                }
                
                Debug.Log("[MainMenuUI] Has save file, showing Load Game Panel...");
                
                // Hiển thị Load Game Panel với 11 slots
                if (loadGamePanel != null)
                {
                    Debug.Log("[MainMenuUI] LoadGamePanel reference found, calling Show()...");
                    loadGamePanel.Show();
                    Debug.Log("[MainMenuUI] Show() called successfully!");
                }
                else
                {
                    Debug.LogError("[MainMenuUI] Load Game Panel is NULL!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainMenuUI] ERROR in OnContinueClick: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public void OnSettingsClick()
        {
            Debug.Log("Settings button clicked!");
            
            if (settingsPanel == null)
            {
                Debug.LogError("Settings Panel is not assigned in MainMenuUI!");
                return;
            }
            
            Debug.Log("Showing Settings Panel...");
            settingsPanel.Show();
        }
        
        public void OnCreditsClick()
        {
            if (creditsPanel != null)
            {
                StartCoroutine(TogglePanel(creditsPanel));
            }
        }
        
        public void OnExitClick()
        {
            StartCoroutine(ExitGame());
        }
        
        private IEnumerator TogglePanel(CanvasGroup panel)
        {
            bool isActive = panel.alpha > 0.5f;
            float targetAlpha = isActive ? 0f : 1f;
            float startAlpha = panel.alpha;
            float duration = 0.3f;
            float elapsed = 0f;
            
            panel.interactable = !isActive;
            panel.blocksRaycasts = !isActive;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            
            panel.alpha = targetAlpha;
        }
        
        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            // Fade out tất cả
            float duration = 1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                
                SetAlpha(titleText, alpha);
                SetAlpha(subtitleText, alpha);
                if (menuButtonsGroup != null) menuButtonsGroup.alpha = alpha;
                
                yield return null;
            }
            
            // Load scene
            SceneManager.LoadScene(sceneName);
        }
        
        private IEnumerator ExitGame()
        {
            // Fade out
            yield return StartCoroutine(LoadSceneWithFade(""));
            
            // Quit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
