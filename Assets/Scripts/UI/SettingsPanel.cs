using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace MadKnight.UI
{
    /// <summary>
    /// Settings Panel - Quản lý cài đặt game đầy đủ
    /// Graphics, Audio, Controls, Gameplay settings
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject settingsRoot;
        
        [Header("Tab System")]
        [SerializeField] private Button graphicsTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button controlsTabButton;
        [SerializeField] private Color selectedTabColor = Color.white;
        [SerializeField] private Color normalTabColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        
        [Header("Tab Panels")]
        [SerializeField] private GameObject graphicsPanel;
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject controlsPanel;
        
        [Header("Graphics Settings")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private TMP_Dropdown fpsLimitDropdown;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private TextMeshProUGUI brightnessValueText;
        
        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private Slider ambienceVolumeSlider;
        [SerializeField] private TextMeshProUGUI ambienceVolumeText;
        
        [Header("Controls Settings")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI mouseSensitivityText;
        [SerializeField] private Toggle invertYAxisToggle;
        
        [Header("Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;
        
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        
        private Resolution[] resolutions;
        private int currentTab = 0; // 0=Graphics, 1=Audio, 2=Controls
        private bool isVisible = false;
        
        // Settings data
        private GameSettings currentSettings;
        
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
            
            if (settingsRoot != null)
            {
                settingsRoot.SetActive(false);
            }
            
            // Load current settings
            currentSettings = GameSettings.Load();
            
            // Setup UI (with null checks)
            SetupResolutions();
            SetupFPSLimits();
            SetupQualityLevels();
            SetupEventListeners();
            
            // Apply loaded settings to UI
            ApplySettingsToUI();
            
            // Show Graphics tab by default
            SwitchToTab(0);
            
            // Validate setup
            ValidateSetup();
        }
        
        private void Start()
        {
            // Force ẩn panel khi start (đảm bảo không đè lên main menu)
            Hide();
        }
        
        private void SetupResolutions()
        {
            if (resolutionDropdown == null)
            {
                Debug.LogWarning("Resolution Dropdown not assigned in SettingsPanel!");
                return;
            }
            
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            
            List<string> options = new List<string>();
            int currentResolutionIndex = 0;
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height + 
                               " @ " + resolutions[i].refreshRateRatio.value.ToString("F0") + "Hz";
                options.Add(option);
                
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
        
        private void SetupFPSLimits()
        {
            if (fpsLimitDropdown == null)
            {
                Debug.LogWarning("FPS Limit Dropdown not assigned in SettingsPanel!");
                return;
            }
            
            fpsLimitDropdown.ClearOptions();
            List<string> options = new List<string> { "30 FPS", "60 FPS", "120 FPS", "Unlimited" };
            fpsLimitDropdown.AddOptions(options);
        }
        
        private void SetupQualityLevels()
        {
            if (qualityDropdown == null)
            {
                Debug.LogWarning("Quality Dropdown not assigned in SettingsPanel!");
                return;
            }
            
            qualityDropdown.ClearOptions();
            List<string> options = new List<string>();
            
            string[] qualityNames = QualitySettings.names;
            foreach (string name in qualityNames)
            {
                options.Add(name);
            }
            
            qualityDropdown.AddOptions(options);
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
        }
        
        private void SetupEventListeners()
        {
            // Tab buttons
            graphicsTabButton?.onClick.AddListener(() => SwitchToTab(0));
            audioTabButton?.onClick.AddListener(() => SwitchToTab(1));
            controlsTabButton?.onClick.AddListener(() => SwitchToTab(2));
            
            // Graphics
            qualityDropdown?.onValueChanged.AddListener(OnQualityChanged);
            resolutionDropdown?.onValueChanged.AddListener(OnResolutionChanged);
            fullscreenToggle?.onValueChanged.AddListener(OnFullscreenChanged);
            vsyncToggle?.onValueChanged.AddListener(OnVSyncChanged);
            fpsLimitDropdown?.onValueChanged.AddListener(OnFPSLimitChanged);
            brightnessSlider?.onValueChanged.AddListener(OnBrightnessChanged);
            
            // Audio
            masterVolumeSlider?.onValueChanged.AddListener(OnMasterVolumeChanged);
            musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
            ambienceVolumeSlider?.onValueChanged.AddListener(OnAmbienceVolumeChanged);
            
            // Controls
            mouseSensitivitySlider?.onValueChanged.AddListener(OnMouseSensitivityChanged);
            invertYAxisToggle?.onValueChanged.AddListener(OnInvertYAxisChanged);
            
            // Action buttons
            applyButton?.onClick.AddListener(OnApplyClicked);
            resetButton?.onClick.AddListener(OnResetClicked);
            backButton?.onClick.AddListener(OnBackClicked);
        }
        
        private void ApplySettingsToUI()
        {
            // Graphics
            if (qualityDropdown != null)
                qualityDropdown.value = currentSettings.qualityLevel;
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = currentSettings.fullscreen;
            if (vsyncToggle != null)
                vsyncToggle.isOn = currentSettings.vsync;
            if (fpsLimitDropdown != null)
                fpsLimitDropdown.value = GetFPSDropdownIndex(currentSettings.targetFPS);
            if (brightnessSlider != null)
                brightnessSlider.value = currentSettings.brightness;
            
            // Audio
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = currentSettings.masterVolume;
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = currentSettings.musicVolume;
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = currentSettings.sfxVolume;
            if (ambienceVolumeSlider != null)
                ambienceVolumeSlider.value = currentSettings.ambienceVolume;
            
            // Controls
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = currentSettings.mouseSensitivity;
            if (invertYAxisToggle != null)
                invertYAxisToggle.isOn = currentSettings.invertYAxis;
        }
        
        private void SwitchToTab(int tabIndex)
        {
            currentTab = tabIndex;
            
            // Hide all panels
            if (graphicsPanel != null) graphicsPanel.SetActive(false);
            if (audioPanel != null) audioPanel.SetActive(false);
            if (controlsPanel != null) controlsPanel.SetActive(false);
            
            // Reset all tab colors
            SetTabColor(graphicsTabButton, normalTabColor);
            SetTabColor(audioTabButton, normalTabColor);
            SetTabColor(controlsTabButton, normalTabColor);
            
            // Show selected panel and highlight tab
            switch (tabIndex)
            {
                case 0:
                    if (graphicsPanel != null) graphicsPanel.SetActive(true);
                    SetTabColor(graphicsTabButton, selectedTabColor);
                    break;
                case 1:
                    if (audioPanel != null) audioPanel.SetActive(true);
                    SetTabColor(audioTabButton, selectedTabColor);
                    break;
                case 2:
                    if (controlsPanel != null) controlsPanel.SetActive(true);
                    SetTabColor(controlsTabButton, selectedTabColor);
                    break;
            }
        }
        
        private void SetTabColor(Button button, Color color)
        {
            if (button == null) return;
            
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = color;
            }
        }
        
        #region Graphics Callbacks
        
        private void OnQualityChanged(int index)
        {
            currentSettings.qualityLevel = index;
            QualitySettings.SetQualityLevel(index);
        }
        
        private void OnResolutionChanged(int index)
        {
            Resolution resolution = resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
        
        private void OnFullscreenChanged(bool value)
        {
            currentSettings.fullscreen = value;
            Screen.fullScreen = value;
        }
        
        private void OnVSyncChanged(bool value)
        {
            currentSettings.vsync = value;
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
        
        private void OnFPSLimitChanged(int index)
        {
            int targetFPS = GetFPSFromDropdownIndex(index);
            currentSettings.targetFPS = targetFPS;
            Application.targetFrameRate = targetFPS;
        }
        
        private void OnBrightnessChanged(float value)
        {
            currentSettings.brightness = value;
            if (brightnessValueText != null)
            {
                brightnessValueText.text = Mathf.RoundToInt(value * 100) + "%";
            }
            // Apply brightness to post-processing or camera
            ApplyBrightness(value);
        }
        
        #endregion
        
        #region Audio Callbacks
        
        private void OnMasterVolumeChanged(float value)
        {
            currentSettings.masterVolume = value;
            if (masterVolumeText != null)
            {
                masterVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
            }
            // Apply to AudioMixer
            AudioManager.Instance?.SetMasterVolume(value);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            currentSettings.musicVolume = value;
            if (musicVolumeText != null)
            {
                musicVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
            }
            AudioManager.Instance?.SetMusicVolume(value);
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            currentSettings.sfxVolume = value;
            if (sfxVolumeText != null)
            {
                sfxVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
            }
            AudioManager.Instance?.SetSFXVolume(value);
        }
        
        private void OnAmbienceVolumeChanged(float value)
        {
            currentSettings.ambienceVolume = value;
            if (ambienceVolumeText != null)
            {
                ambienceVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
            }
            AudioManager.Instance?.SetAmbienceVolume(value);
        }
        
        #endregion
        
        #region Controls Callbacks
        
        private void OnMouseSensitivityChanged(float value)
        {
            currentSettings.mouseSensitivity = value;
            if (mouseSensitivityText != null)
            {
                mouseSensitivityText.text = value.ToString("F1");
            }
        }
        
        private void OnInvertYAxisChanged(bool value)
        {
            currentSettings.invertYAxis = value;
        }
        
        #endregion
        
        #region Button Callbacks
        
        private void OnApplyClicked()
        {
            currentSettings.Save();
            Debug.Log("Settings saved!");
        }
        
        private void OnResetClicked()
        {
            currentSettings = GameSettings.GetDefault();
            ApplySettingsToUI();
            currentSettings.ApplyAll();
            Debug.Log("Settings reset to default!");
        }
        
        private void OnBackClicked()
        {
            Hide();
        }
        
        #endregion
        
        #region Helper Methods
        
        private int GetFPSDropdownIndex(int targetFPS)
        {
            switch (targetFPS)
            {
                case 30: return 0;
                case 60: return 1;
                case 120: return 2;
                default: return 3; // Unlimited
            }
        }
        
        private int GetFPSFromDropdownIndex(int index)
        {
            switch (index)
            {
                case 0: return 30;
                case 1: return 60;
                case 2: return 120;
                default: return -1; // Unlimited
            }
        }
        
        private void ApplyBrightness(float value)
        {
            // TODO: Apply to post-processing volume or camera
            // Example with camera:
            // Camera.main.GetComponent<PostProcessing>().brightness = value;
        }
        
        #endregion
        
        #region Show/Hide
        
        public void Show()
        {
            Debug.Log("[SettingsPanel] Show() called");
            
            if (isVisible)
            {
                Debug.Log("[SettingsPanel] Already visible, skipping");
                return;
            }
            
            Debug.Log("[SettingsPanel] Starting fade in...");
            isVisible = true;
            if (settingsRoot != null)
            {
                settingsRoot.SetActive(true);
            }
            
            StartCoroutine(FadeIn());
        }
        
        public void Hide()
        {
            if (!isVisible) return;
            
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
            
            if (settingsRoot != null)
            {
                settingsRoot.SetActive(false);
            }
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validate setup và log warnings nếu thiếu references
        /// </summary>
        private void ValidateSetup()
        {
            int missingCount = 0;
            
            // Tab buttons
            if (graphicsTabButton == null) { Debug.LogWarning("[SettingsPanel] Graphics Tab Button not assigned!"); missingCount++; }
            if (audioTabButton == null) { Debug.LogWarning("[SettingsPanel] Audio Tab Button not assigned!"); missingCount++; }
            if (controlsTabButton == null) { Debug.LogWarning("[SettingsPanel] Controls Tab Button not assigned!"); missingCount++; }
            
            // Tab panels
            if (graphicsPanel == null) { Debug.LogWarning("[SettingsPanel] Graphics Panel not assigned!"); missingCount++; }
            if (audioPanel == null) { Debug.LogWarning("[SettingsPanel] Audio Panel not assigned!"); missingCount++; }
            if (controlsPanel == null) { Debug.LogWarning("[SettingsPanel] Controls Panel not assigned!"); missingCount++; }
            
            // Graphics settings
            if (qualityDropdown == null) { Debug.LogWarning("[SettingsPanel] Quality Dropdown not assigned!"); missingCount++; }
            if (resolutionDropdown == null) { Debug.LogWarning("[SettingsPanel] Resolution Dropdown not assigned!"); missingCount++; }
            if (fullscreenToggle == null) { Debug.LogWarning("[SettingsPanel] Fullscreen Toggle not assigned!"); missingCount++; }
            if (vsyncToggle == null) { Debug.LogWarning("[SettingsPanel] VSync Toggle not assigned!"); missingCount++; }
            if (fpsLimitDropdown == null) { Debug.LogWarning("[SettingsPanel] FPS Limit Dropdown not assigned!"); missingCount++; }
            if (brightnessSlider == null) { Debug.LogWarning("[SettingsPanel] Brightness Slider not assigned!"); missingCount++; }
            
            // Audio settings
            if (masterVolumeSlider == null) { Debug.LogWarning("[SettingsPanel] Master Volume Slider not assigned!"); missingCount++; }
            if (musicVolumeSlider == null) { Debug.LogWarning("[SettingsPanel] Music Volume Slider not assigned!"); missingCount++; }
            if (sfxVolumeSlider == null) { Debug.LogWarning("[SettingsPanel] SFX Volume Slider not assigned!"); missingCount++; }
            if (ambienceVolumeSlider == null) { Debug.LogWarning("[SettingsPanel] Ambience Volume Slider not assigned!"); missingCount++; }
            
            // Controls settings
            if (mouseSensitivitySlider == null) { Debug.LogWarning("[SettingsPanel] Mouse Sensitivity Slider not assigned!"); missingCount++; }
            if (invertYAxisToggle == null) { Debug.LogWarning("[SettingsPanel] Invert Y Axis Toggle not assigned!"); missingCount++; }
            
            // Buttons
            if (applyButton == null) { Debug.LogWarning("[SettingsPanel] Apply Button not assigned!"); missingCount++; }
            if (resetButton == null) { Debug.LogWarning("[SettingsPanel] Reset Button not assigned!"); missingCount++; }
            if (backButton == null) { Debug.LogWarning("[SettingsPanel] Back Button not assigned!"); missingCount++; }
            
            if (missingCount > 0)
            {
                Debug.LogError($"[SettingsPanel] {missingCount} references are missing! Check Inspector and assign them.");
                Debug.LogError("[SettingsPanel] See SETTINGS_PANEL_GUIDE.md for setup instructions.");
            }
            else
            {
                Debug.Log("[SettingsPanel] All references assigned correctly! ✅");
            }
        }
        
        #endregion
    }
}
