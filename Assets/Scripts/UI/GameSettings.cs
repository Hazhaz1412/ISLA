using UnityEngine;

namespace MadKnight.UI
{
    /// <summary>
    /// Game Settings Data - Lưu trữ tất cả cài đặt của game
    /// Tự động save/load từ PlayerPrefs
    /// </summary>
    [System.Serializable]
    public class GameSettings
    {
        // Graphics
        public int qualityLevel = 2; // 0=Low, 1=Medium, 2=High
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool fullscreen = true;
        public bool vsync = false;
        public int targetFPS = 60; // 30, 60, 120, -1 (unlimited)
        public float brightness = 1f; // 0.5 - 1.5
        
        // Audio
        public float masterVolume = 0.8f; // 0 - 1
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public float ambienceVolume = 0.6f;
        
        // Controls
        public float mouseSensitivity = 1.0f; // 0.1 - 3.0
        public bool invertYAxis = false;
        public bool holdToRun = false; // false = toggle run
        
        // Gameplay
        public int difficulty = 1; // 0=Easy, 1=Normal, 2=Hard, 3=Nightmare
        public bool subtitles = true;
        public bool screenShake = true;
        public bool bloodEffects = true;
        
        // Keys for PlayerPrefs
        private const string KEY_QUALITY = "Settings_Quality";
        private const string KEY_RES_WIDTH = "Settings_ResWidth";
        private const string KEY_RES_HEIGHT = "Settings_ResHeight";
        private const string KEY_FULLSCREEN = "Settings_Fullscreen";
        private const string KEY_VSYNC = "Settings_VSync";
        private const string KEY_TARGET_FPS = "Settings_TargetFPS";
        private const string KEY_BRIGHTNESS = "Settings_Brightness";
        
        private const string KEY_MASTER_VOL = "Settings_MasterVolume";
        private const string KEY_MUSIC_VOL = "Settings_MusicVolume";
        private const string KEY_SFX_VOL = "Settings_SFXVolume";
        private const string KEY_AMBIENCE_VOL = "Settings_AmbienceVolume";
        
        private const string KEY_MOUSE_SENS = "Settings_MouseSensitivity";
        private const string KEY_INVERT_Y = "Settings_InvertY";
        private const string KEY_HOLD_TO_RUN = "Settings_HoldToRun";
        
        private const string KEY_DIFFICULTY = "Settings_Difficulty";
        private const string KEY_SUBTITLES = "Settings_Subtitles";
        private const string KEY_SCREEN_SHAKE = "Settings_ScreenShake";
        private const string KEY_BLOOD = "Settings_BloodEffects";
        
        /// <summary>
        /// Save tất cả settings vào PlayerPrefs
        /// </summary>
        public void Save()
        {
            // Graphics
            PlayerPrefs.SetInt(KEY_QUALITY, qualityLevel);
            PlayerPrefs.SetInt(KEY_RES_WIDTH, resolutionWidth);
            PlayerPrefs.SetInt(KEY_RES_HEIGHT, resolutionHeight);
            PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(KEY_VSYNC, vsync ? 1 : 0);
            PlayerPrefs.SetInt(KEY_TARGET_FPS, targetFPS);
            PlayerPrefs.SetFloat(KEY_BRIGHTNESS, brightness);
            
            // Audio
            PlayerPrefs.SetFloat(KEY_MASTER_VOL, masterVolume);
            PlayerPrefs.SetFloat(KEY_MUSIC_VOL, musicVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOL, sfxVolume);
            PlayerPrefs.SetFloat(KEY_AMBIENCE_VOL, ambienceVolume);
            
            // Controls
            PlayerPrefs.SetFloat(KEY_MOUSE_SENS, mouseSensitivity);
            PlayerPrefs.SetInt(KEY_INVERT_Y, invertYAxis ? 1 : 0);
            PlayerPrefs.SetInt(KEY_HOLD_TO_RUN, holdToRun ? 1 : 0);
            
            // Gameplay
            PlayerPrefs.SetInt(KEY_DIFFICULTY, difficulty);
            PlayerPrefs.SetInt(KEY_SUBTITLES, subtitles ? 1 : 0);
            PlayerPrefs.SetInt(KEY_SCREEN_SHAKE, screenShake ? 1 : 0);
            PlayerPrefs.SetInt(KEY_BLOOD, bloodEffects ? 1 : 0);
            
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load settings từ PlayerPrefs
        /// </summary>
        public static GameSettings Load()
        {
            GameSettings settings = new GameSettings();
            
            // Graphics
            settings.qualityLevel = PlayerPrefs.GetInt(KEY_QUALITY, 2);
            settings.resolutionWidth = PlayerPrefs.GetInt(KEY_RES_WIDTH, Screen.currentResolution.width);
            settings.resolutionHeight = PlayerPrefs.GetInt(KEY_RES_HEIGHT, Screen.currentResolution.height);
            settings.fullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
            settings.vsync = PlayerPrefs.GetInt(KEY_VSYNC, 0) == 1;
            settings.targetFPS = PlayerPrefs.GetInt(KEY_TARGET_FPS, 60);
            settings.brightness = PlayerPrefs.GetFloat(KEY_BRIGHTNESS, 1f);
            
            // Audio
            settings.masterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOL, 0.8f);
            settings.musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.7f);
            settings.sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOL, 0.8f);
            settings.ambienceVolume = PlayerPrefs.GetFloat(KEY_AMBIENCE_VOL, 0.6f);
            
            // Controls
            settings.mouseSensitivity = PlayerPrefs.GetFloat(KEY_MOUSE_SENS, 1.0f);
            settings.invertYAxis = PlayerPrefs.GetInt(KEY_INVERT_Y, 0) == 1;
            settings.holdToRun = PlayerPrefs.GetInt(KEY_HOLD_TO_RUN, 0) == 1;
            
            // Gameplay
            settings.difficulty = PlayerPrefs.GetInt(KEY_DIFFICULTY, 1);
            settings.subtitles = PlayerPrefs.GetInt(KEY_SUBTITLES, 1) == 1;
            settings.screenShake = PlayerPrefs.GetInt(KEY_SCREEN_SHAKE, 1) == 1;
            settings.bloodEffects = PlayerPrefs.GetInt(KEY_BLOOD, 1) == 1;
            
            return settings;
        }
        
        /// <summary>
        /// Lấy settings mặc định
        /// </summary>
        public static GameSettings GetDefault()
        {
            return new GameSettings();
        }
        
        /// <summary>
        /// Apply tất cả settings vào game
        /// </summary>
        public void ApplyAll()
        {
            // Graphics
            QualitySettings.SetQualityLevel(qualityLevel);
            Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreen);
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            Application.targetFrameRate = targetFPS;
            
            // Audio - cần AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(masterVolume);
                AudioManager.Instance.SetMusicVolume(musicVolume);
                AudioManager.Instance.SetSFXVolume(sfxVolume);
                AudioManager.Instance.SetAmbienceVolume(ambienceVolume);
            }
            
            // Brightness - TODO: Apply to post-processing
        }
        
        /// <summary>
        /// Reset về mặc định và xóa PlayerPrefs
        /// </summary>
        public static void ResetAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
