using UnityEngine;
using System;
using System.IO;

namespace MadKnight.Save
{
    /// <summary>
    /// Save System - Quản lý lưu/load game data
    /// Sử dụng JSON + File System
    /// Auto Save: LocalLow/RedRat/autosave.json
    /// Manual Saves: LocalLow/RedRat/save01.json, save02.json, ... (max 10 slots)
    /// Version: 1.1 - Force recompile
    /// </summary>
    public static class SaveSystem
    {
        private const string COMPANY_NAME = "RedRat";
        private const string AUTO_SAVE_FILE = "autosave.json";
        private const int MAX_SAVE_SLOTS = 10;
        
        // Custom save path: LocalLow/RedRat/ (thay vì dùng Application.persistentDataPath)
        private static string SavePath
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                // Windows: C:\Users\[Name]\AppData\LocalLow\RedRat\
                string localLowPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "Low";
                return System.IO.Path.Combine(localLowPath, COMPANY_NAME);
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                // Mac: ~/Library/Application Support/RedRat/
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Library/Application Support", COMPANY_NAME);
#else
                // Linux/Other: ~/.config/unity3d/RedRat/
                return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ".config/unity3d", COMPANY_NAME);
#endif
            }
        }
        
        private static string AutoSavePath => Path.Combine(SavePath, AUTO_SAVE_FILE);
        
        #region Auto Save
        
        /// <summary>
        /// Tạo auto save mới (khi bắt đầu New Game)
        /// </summary>
        public static void CreateAutoSave()
        {
            PlayerSaveData newSave = new PlayerSaveData
            {
                saveVersion = 1,
                saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                currentScene = "Level01",
                playerPosition = Vector3.zero,
                playerHealth = 100f,
                playerStamina = 100f,
                checkpointScene = "Level01",
                checkpointPosition = Vector3.zero,
                playTimeSeconds = 0f,
                difficulty = 1 // Normal
            };
            
            SaveAutoSave(newSave);
            Debug.Log($"[SaveSystem] Auto save created at: {AutoSavePath}");
        }
        
        /// <summary>
        /// Lưu auto save (tự động liên tục trong game)
        /// Path: LocalLow/RedRat/autosave.json
        /// </summary>
        public static void SaveAutoSave(PlayerSaveData data)
        {
            try
            {
                // Ensure save folder exists
                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                    Debug.Log($"[SaveSystem] Created save directory: {SavePath}");
                }
                
                // Update save time
                data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // Convert to JSON
                string json = JsonUtility.ToJson(data, true);
                
                // Save to autosave.json
                File.WriteAllText(AutoSavePath, json);
                
                Debug.Log($"[SaveSystem] Auto save successful: {AutoSavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to auto save: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load auto save
        /// </summary>
        public static PlayerSaveData LoadAutoSave()
        {
            try
            {
                if (!File.Exists(AutoSavePath))
                {
                    Debug.LogWarning("[SaveSystem] No auto save file found!");
                    return null;
                }
                
                string json = File.ReadAllText(AutoSavePath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                
                Debug.Log($"[SaveSystem] Auto save loaded: {data.saveTime}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load auto save: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Kiểm tra có auto save không
        /// </summary>
        public static bool HasAnySaveFile()
        {
            return File.Exists(AutoSavePath);
        }
        
        /// <summary>
        /// Xóa auto save
        /// </summary>
        public static void DeleteAutoSave()
        {
            try
            {
                if (File.Exists(AutoSavePath))
                {
                    File.Delete(AutoSavePath);
                    Debug.Log("[SaveSystem] Auto save deleted!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete auto save: {e.Message}");
            }
        }
        
        #endregion
        
        #region Manual Save Slots (save01 - save10)
        
        /// <summary>
        /// Lưu vào slot (1-10)
        /// Path: LocalLow/RedRat/save01.json, save02.json, ...
        /// </summary>
        public static void SaveToSlot(PlayerSaveData data, int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
            {
                Debug.LogError($"[SaveSystem] Invalid slot number: {slotNumber}. Must be 1-{MAX_SAVE_SLOTS}");
                return;
            }
            
            try
            {
                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                }
                
                data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                data.slotNumber = slotNumber;
                
                string json = JsonUtility.ToJson(data, true);
                string fileName = GetSlotFileName(slotNumber);
                string filePath = Path.Combine(SavePath, fileName);
                
                File.WriteAllText(filePath, json);
                
                Debug.Log($"[SaveSystem] Saved to slot {slotNumber}: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save to slot {slotNumber}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load từ slot (1-10)
        /// </summary>
        public static PlayerSaveData LoadFromSlot(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
            {
                Debug.LogError($"[SaveSystem] Invalid slot number: {slotNumber}");
                return null;
            }
            
            try
            {
                string fileName = GetSlotFileName(slotNumber);
                string filePath = Path.Combine(SavePath, fileName);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveSystem] Slot {slotNumber} is empty");
                    return null;
                }
                
                string json = File.ReadAllText(filePath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                
                Debug.Log($"[SaveSystem] Loaded from slot {slotNumber}: {data.saveTime}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load from slot {slotNumber}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Kiểm tra slot có save không
        /// </summary>
        public static bool IsSlotUsed(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS) return false;
            
            string fileName = GetSlotFileName(slotNumber);
            string filePath = Path.Combine(SavePath, fileName);
            return File.Exists(filePath);
        }
        
        /// <summary>
        /// Xóa save slot
        /// </summary>
        public static void DeleteSlot(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
            {
                Debug.LogError($"[SaveSystem] Invalid slot number: {slotNumber}");
                return;
            }
            
            try
            {
                string fileName = GetSlotFileName(slotNumber);
                string filePath = Path.Combine(SavePath, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveSystem] Slot {slotNumber} deleted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete slot {slotNumber}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Lấy thông tin của tất cả save slots
        /// </summary>
        public static SaveSlotInfo[] GetAllSlots()
        {
            SaveSlotInfo[] slots = new SaveSlotInfo[MAX_SAVE_SLOTS];
            
            for (int i = 1; i <= MAX_SAVE_SLOTS; i++)
            {
                slots[i - 1] = GetSlotInfo(i);
            }
            
            return slots;
        }
        
        /// <summary>
        /// Lấy thông tin của 1 slot
        /// </summary>
        public static SaveSlotInfo GetSlotInfo(int slotNumber)
        {
            SaveSlotInfo info = new SaveSlotInfo
            {
                slotNumber = slotNumber,
                isEmpty = true
            };
            
            if (slotNumber < 1 || slotNumber > MAX_SAVE_SLOTS)
            {
                return info;
            }
            
            string fileName = GetSlotFileName(slotNumber);
            string filePath = Path.Combine(SavePath, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                    
                    info.isEmpty = false;
                    info.saveTime = data.saveTime;
                    info.currentScene = data.currentScene;
                    info.playTimeSeconds = data.playTimeSeconds;
                    info.playerLevel = data.playerLevel;
                    info.difficulty = data.difficulty;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Failed to read slot {slotNumber}: {e.Message}");
                }
            }
            
            return info;
        }
        
        /// <summary>
        /// Get slot file name (save01.json, save02.json, ...)
        /// </summary>
        private static string GetSlotFileName(int slotNumber)
        {
            return $"save{slotNumber:D2}.json"; // D2 = 2 digits: 01, 02, 03, ...
        }
        
        #endregion
        
        #region Manual Save/Load (Legacy - Deprecated)
        
        /// <summary>
        /// [DEPRECATED] Dùng SaveToSlot() thay thế
        /// </summary>
        public static void SaveGame(PlayerSaveData data, string saveName)
        {
            Debug.LogWarning("[SaveSystem] SaveGame(string) is deprecated. Use SaveToSlot(int) instead.");
            
            try
            {
                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                }
                
                data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = JsonUtility.ToJson(data, true);
                
                string filePath = Path.Combine(SavePath, $"{saveName}.json");
                File.WriteAllText(filePath, json);
                
                Debug.Log($"[SaveSystem] Game saved: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save game: {e.Message}");
            }
        }
        
        /// <summary>
        /// [DEPRECATED] Dùng LoadFromSlot() thay thế
        /// </summary>
        public static PlayerSaveData LoadGame(string saveName)
        {
            Debug.LogWarning("[SaveSystem] LoadGame(string) is deprecated. Use LoadFromSlot(int) instead.");
            
            try
            {
                string filePath = Path.Combine(SavePath, $"{saveName}.json");
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveSystem] Save file not found: {saveName}");
                    return null;
                }
                
                string json = File.ReadAllText(filePath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                
                Debug.Log($"[SaveSystem] Game loaded: {saveName}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load game: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// [DEPRECATED] Dùng DeleteSlot() thay thế
        /// </summary>
        public static void DeleteSave(string saveName)
        {
            Debug.LogWarning("[SaveSystem] DeleteSave(string) is deprecated. Use DeleteSlot(int) instead.");
            
            try
            {
                string filePath = Path.Combine(SavePath, $"{saveName}.json");
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[SaveSystem] Save deleted: {saveName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete save: {e.Message}");
            }
        }
        
        /// <summary>
        /// Lấy danh sách tất cả save files (bao gồm cả custom saves)
        /// </summary>
        public static string[] GetAllSaveFiles()
        {
            try
            {
                if (!Directory.Exists(SavePath))
                {
                    return new string[0];
                }
                
                string[] files = Directory.GetFiles(SavePath, "*.json");
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = Path.GetFileNameWithoutExtension(files[i]);
                }
                
                return files;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to get save files: {e.Message}");
                return new string[0];
            }
        }
        
        #endregion
        
        #region Checkpoint System
        
        /// <summary>
        /// Lưu checkpoint (quick save tại vị trí checkpoint)
        /// </summary>
        public static void SaveCheckpoint(string sceneName, Vector3 position)
        {
            PlayerSaveData currentData = LoadAutoSave();
            
            if (currentData == null)
            {
                Debug.LogWarning("[SaveSystem] No save data to update checkpoint!");
                return;
            }
            
            currentData.checkpointScene = sceneName;
            currentData.checkpointPosition = position;
            
            SaveAutoSave(currentData);
            Debug.Log($"[SaveSystem] Checkpoint saved: {sceneName} at {position}");
        }
        
        /// <summary>
        /// Load từ checkpoint gần nhất
        /// </summary>
        public static PlayerSaveData LoadFromCheckpoint()
        {
            PlayerSaveData data = LoadAutoSave();
            
            if (data != null)
            {
                // Restore to checkpoint position
                data.currentScene = data.checkpointScene;
                data.playerPosition = data.checkpointPosition;
            }
            
            return data;
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Xóa tất cả save files (reset game)
        /// Bao gồm autosave và tất cả save slots
        /// </summary>
        public static void DeleteAllSaves()
        {
            try
            {
                if (Directory.Exists(SavePath))
                {
                    Directory.Delete(SavePath, true);
                    Debug.Log("[SaveSystem] All saves deleted!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to delete all saves: {e.Message}");
            }
        }
        
        /// <summary>
        /// Get save folder path
        /// </summary>
        public static string GetSavePath()
        {
            return SavePath;
        }
        
        /// <summary>
        /// Copy auto save vào slot
        /// </summary>
        public static void CopyAutoSaveToSlot(int slotNumber)
        {
            PlayerSaveData autoSave = LoadAutoSave();
            if (autoSave != null)
            {
                SaveToSlot(autoSave, slotNumber);
                Debug.Log($"[SaveSystem] Auto save copied to slot {slotNumber}");
            }
            else
            {
                Debug.LogWarning("[SaveSystem] No auto save to copy!");
            }
        }
        
        /// <summary>
        /// Copy slot vào auto save
        /// </summary>
        public static void CopySlotToAutoSave(int slotNumber)
        {
            PlayerSaveData slotSave = LoadFromSlot(slotNumber);
            if (slotSave != null)
            {
                SaveAutoSave(slotSave);
                Debug.Log($"[SaveSystem] Slot {slotNumber} copied to auto save");
            }
            else
            {
                Debug.LogWarning($"[SaveSystem] Slot {slotNumber} is empty!");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Thông tin save slot (để hiển thị UI)
    /// </summary>
    [System.Serializable]
    public class SaveSlotInfo
    {
        public int slotNumber;        // 1-10
        public bool isEmpty;          // Slot trống hay không
        public string saveTime;       // "2024-01-15 14:30:00"
        public string currentScene;   // "Level01"
        public float playTimeSeconds; // 1234.5
        public int playerLevel;       // 1
        public int difficulty;        // 0-3
        
        // Utility
        public string GetPlayTimeFormatted()
        {
            int hours = (int)(playTimeSeconds / 3600);
            int minutes = (int)((playTimeSeconds % 3600) / 60);
            return $"{hours}h {minutes}m";
        }
        
        public string GetDifficultyName()
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
    
    /// <summary>
    /// Player Save Data - Tất cả dữ liệu cần lưu
    /// </summary>
    [System.Serializable]
    public class PlayerSaveData
    {
        // Meta data
        public int saveVersion;
        public string saveTime;
        public float playTimeSeconds;
        public int slotNumber;        // 0 = autosave, 1-10 = manual slots
        
        // Scene & Position
        public string currentScene;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        
        // Player Stats
        public float playerHealth;
        public float playerStamina;
        public int playerLevel;
        public int playerExperience;
        
        // Checkpoint
        public string checkpointScene;
        public Vector3 checkpointPosition;
        
        // Inventory (sẽ mở rộng sau)
        public string[] inventoryItems;
        public int[] inventoryQuantities;
        
        // Game Progress
        public bool[] levelsUnlocked;
        public bool[] achievementsUnlocked;
        public int difficulty; // 0=Easy, 1=Normal, 2=Hard, 3=Nightmare
        
        // Settings (sync với GameSettings)
        public int qualityLevel;
        public bool fullscreen;
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
    }
}
