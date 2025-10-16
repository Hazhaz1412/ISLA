using UnityEngine;
using MadKnight.Save;

namespace MadKnight
{
    /// <summary>
    /// Ví dụ về cách sử dụng SaveSystem trong game
    /// Gắn script này vào một GameObject trong scene để quản lý việc save/load
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private MadKnight.Save.PlayerSaveData currentSaveData;
        
        private void Start()
        {
            // Load auto save khi bắt đầu game
            LoadGameData();
        }
        
        /// <summary>
        /// Load dữ liệu game
        /// </summary>
        private void LoadGameData()
        {
            currentSaveData = SaveSystem.LoadAutoSave();
            
            if (currentSaveData != null)
            {
                Debug.Log($"Đã load game data: Level {currentSaveData.playerLevel}, Scene: {currentSaveData.currentScene}");
                // Áp dụng dữ liệu vào game
                // Ví dụ: Set vị trí player, health, etc.
            }
            else
            {
                Debug.Log("Không có dữ liệu save, sử dụng giá trị mặc định");
            }
        }
        
        /// <summary>
        /// Lưu dữ liệu game (gọi hàm này khi cần auto save)
        /// </summary>
        public void SaveGameData()
        {
            if (currentSaveData == null)
            {
                currentSaveData = new PlayerSaveData();
            }
            
            // Cập nhật dữ liệu hiện tại
            currentSaveData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentSaveData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Lấy thông tin từ player (cần tùy chỉnh theo game của bạn)
            // Ví dụ:
            // GameObject player = GameObject.FindGameObjectWithTag("Player");
            // currentSaveData.playerPosition = player.transform.position;
            // currentSaveData.playerRotation = player.transform.rotation;
            // currentSaveData.playerHealth = playerController.health;
            
            // Lưu auto save
            SaveSystem.SaveAutoSave(currentSaveData);
        }
        
        /// <summary>
        /// Gọi hàm này để auto save theo interval (ví dụ mỗi 30 giây)
        /// </summary>
        private void AutoSave()
        {
            InvokeRepeating(nameof(SaveGameData), 30f, 30f); // Auto save mỗi 30 giây
        }
    }
}
