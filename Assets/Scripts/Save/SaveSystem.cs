using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MadKnight.Save
{
    /// <summary>
    /// Save System - Quản lý lưu/load game data
    /// Sử dụng JSON + Encryption (AES-256) để bảo vệ dữ liệu
    /// Auto Save: LocalLow/RedRat/autosave.dat (encrypted)
    /// Manual Saves: LocalLow/RedRat/save01.dat, save02.dat, ... (max 10 slots, encrypted)
    /// Version: 2.0 - Encrypted Save System
    /// </summary>
    public static class SaveSystem
    {
        // Encryption Key (32 bytes for AES-256)
        // QUAN TRỌNG: Thay đổi key này để bảo mật riêng cho game của bạn!
        // PHẢI đúng 32 ký tự (32 bytes) cho AES-256
        private static readonly byte[] ENCRYPTION_KEY = Encoding.UTF8.GetBytes("ISLA_Game_Super_Secret_Key_2025!");
        
        // Initialization Vector (16 bytes for AES)
        // PHẢI đúng 16 ký tự (16 bytes)
        private static readonly byte[] ENCRYPTION_IV = Encoding.UTF8.GetBytes("RedRat_IV16Bytes");
        private const string COMPANY_NAME = "RedRat";
        private const string AUTO_SAVE_FILE = "autosave.dat"; // Đổi từ .json sang .dat
        private const int MAX_SAVE_SLOTS = 10;
        
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
        
        #region Encryption/Decryption
        
        /// <summary>
        /// Mã hóa dữ liệu JSON thành byte array
        /// </summary>
        private static byte[] EncryptData(string jsonData)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = ENCRYPTION_KEY;
                    aes.IV = ENCRYPTION_IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] plainBytes = Encoding.UTF8.GetBytes(jsonData);
                            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                            csEncrypt.FlushFinalBlock();
                            return msEncrypt.ToArray();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Encryption failed: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Giải mã byte array thành JSON string
        /// </summary>
        private static string DecryptData(byte[] encryptedData)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = ENCRYPTION_KEY;
                    aes.IV = ENCRYPTION_IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Decryption failed: {e.Message}");
                return null;
            }
        }
        
        #endregion
        
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
                checkpointScene = "Level01",
                checkpointPosition = Vector3.zero,
                playTimeSeconds = 0f,
                
                // Initialize arrays
                inventoryItemIDs = new string[0],
                inventoryQuantities = new int[0],
                solvedPuzzleIDs = new string[0],
                unlockedDoorIDs = new string[0],
                collectedItemIDs = new string[0],
                completedLevelIDs = new string[0],
                unlockedLevelIDs = new string[] { "Level01" }, // First level unlocked
                completedDialogueIDs = new string[0],
                npcStateData = new string[0],
                discoveredClueIDs = new string[0],
                readNotesIDs = new string[0]
            };
            
            SaveAutoSave(newSave);
            Debug.Log($"[SaveSystem] Auto save created at: {AutoSavePath}");
        }
        
        /// <summary>
        /// Lưu auto save (tự động liên tục trong game)
        /// Path: LocalLow/RedRat/autosave.dat (encrypted)
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
                string json = JsonUtility.ToJson(data, false); // prettyPrint = false để giảm size
                
                // Encrypt JSON
                byte[] encryptedData = EncryptData(json);
                if (encryptedData == null)
                {
                    Debug.LogError("[SaveSystem] Failed to encrypt save data!");
                    return;
                }
                
                // Save encrypted data to .dat file
                File.WriteAllBytes(AutoSavePath, encryptedData);
                
                Debug.Log($"[SaveSystem] Auto save successful (encrypted): {AutoSavePath}");
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
                
                // Read encrypted data
                byte[] encryptedData = File.ReadAllBytes(AutoSavePath);
                
                // Decrypt data
                string json = DecryptData(encryptedData);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("[SaveSystem] Failed to decrypt save data!");
                    return null;
                }
                
                // Parse JSON
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                
                Debug.Log($"[SaveSystem] Auto save loaded (decrypted): {data.saveTime}");
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
        /// Path: LocalLow/RedRat/save01.dat, save02.dat, ... (encrypted)
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
                
                // Convert to JSON
                string json = JsonUtility.ToJson(data, false);
                
                // Encrypt
                byte[] encryptedData = EncryptData(json);
                if (encryptedData == null)
                {
                    Debug.LogError("[SaveSystem] Failed to encrypt save data!");
                    return;
                }
                
                // Save to .dat file
                string fileName = GetSlotFileName(slotNumber);
                string filePath = Path.Combine(SavePath, fileName);
                File.WriteAllBytes(filePath, encryptedData);
                
                Debug.Log($"[SaveSystem] Saved to slot {slotNumber} (encrypted): {filePath}");
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
                
                // Read encrypted data
                byte[] encryptedData = File.ReadAllBytes(filePath);
                
                // Decrypt
                string json = DecryptData(encryptedData);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("[SaveSystem] Failed to decrypt save data!");
                    return null;
                }
                
                // Parse JSON
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                
                Debug.Log($"[SaveSystem] Loaded from slot {slotNumber} (decrypted): {data.saveTime}");
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
                    // Read and decrypt
                    byte[] encryptedData = File.ReadAllBytes(filePath);
                    string json = DecryptData(encryptedData);
                    
                    if (!string.IsNullOrEmpty(json))
                    {
                        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                        
                        info.isEmpty = false;
                        info.saveTime = data.saveTime;
                        info.currentScene = data.currentScene;
                        info.playTimeSeconds = data.playTimeSeconds;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Failed to read slot {slotNumber}: {e.Message}");
                }
            }
            
            return info;
        }
        
        /// <summary>
        /// Get slot file name (save01.dat, save02.dat, ...)
        /// </summary>
        private static string GetSlotFileName(int slotNumber)
        {
            return $"save{slotNumber:D2}.dat"; // Changed from .json to .dat
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
        
        // Utility
        public string GetPlayTimeFormatted()
        {
            int hours = (int)(playTimeSeconds / 3600);
            int minutes = (int)((playTimeSeconds % 3600) / 60);
            return $"{hours}h {minutes}m";
        }
    }
    
    /// <summary>
    /// Player Save Data - Cho game giải đố + thu thập vật phẩm
    /// Dữ liệu được mã hóa AES-256 khi lưu vào file
    /// </summary>
    [System.Serializable]
    public class PlayerSaveData
    {
        // ===== META DATA =====
        public int saveVersion;           // Version của save file
        public string saveTime;           // Thời gian lưu "2024-01-15 14:30:00"
        public float playTimeSeconds;     // Tổng thời gian chơi (giây)
        public int slotNumber;            // 0 = autosave, 1-10 = manual slots
        
        // ===== SCENE & POSITION =====
        public string currentScene;       // Scene hiện tại "Level01"
        public Vector3 playerPosition;    // Vị trí player trong scene
        
        // ===== CHECKPOINT =====
        public string checkpointScene;    // Scene của checkpoint gần nhất
        public Vector3 checkpointPosition;// Vị trí checkpoint
        
        // ===== INVENTORY - THU THẬP VẬT PHẨM =====
        public string[] inventoryItemIDs;     // ID của vật phẩm (ví dụ: "key_red", "potion_health")
        public int[] inventoryQuantities;     // Số lượng tương ứng (ví dụ: 1, 3)
        
        // ===== PUZZLE PROGRESS - TIẾN TRÌNH GIẢI ĐỐ =====
        public string[] solvedPuzzleIDs;      // Danh sách puzzle đã giải (ví dụ: "puzzle_door_01", "puzzle_safe_02")
        public string[] unlockedDoorIDs;      // Danh sách cửa đã mở (ví dụ: "door_main_01")
        public string[] collectedItemIDs;     // Danh sách vật phẩm đã nhặt trong world (ví dụ: "item_coin_01", "item_key_basement")
        
        // ===== GAME PROGRESS =====
        public string[] completedLevelIDs;    // Danh sách level đã hoàn thành
        public string[] unlockedLevelIDs;     // Danh sách level đã mở khóa
        
        // ===== NPC & DIALOGUE =====
        public string[] completedDialogueIDs; // Danh sách hội thoại đã xem (để tránh lặp lại)
        public string[] npcStateData;         // Trạng thái NPC (ví dụ: "npc_guard_01:dead", "npc_shopkeeper:friendly")
        
        // ===== NOTES & CLUES - GHI CHÚ =====
        public string[] discoveredClueIDs;    // Danh sách manh mối đã phát hiện (cho puzzle)
        public string[] readNotesIDs;         // Danh sách ghi chú đã đọc
    }
}
