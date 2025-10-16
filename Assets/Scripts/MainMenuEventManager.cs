using MadKnight.Enums;
using MadKnight.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MadKnight
{
    public class MainMenuEventManager : MonoBehaviour
    {
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnContinue; 
        [SerializeField] private Button _btnQuit;
 

        private void Start()
        {
            _btnPlay.onClick.AddListener(BtnPlayClick);
            _btnContinue.onClick.AddListener(BtnContinueClick); 
            _btnQuit.onClick.AddListener(BtnQuitClick);
            
            // Kiểm tra và vô hiệu hóa nút Continue nếu không có file save
            CheckSaveFileAndUpdateUI();
        }
        
        /// <summary>
        /// Kiểm tra file save và cập nhật UI
        /// </summary>
        private void CheckSaveFileAndUpdateUI()
        {
            bool hasSaveFile = SaveSystem.HasAnySaveFile();
            
            // Vô hiệu hóa nút Continue nếu không có file save
            _btnContinue.interactable = hasSaveFile;
            
            if (!hasSaveFile)
            {
                Debug.Log("Không tìm thấy file save nào. Nút Continue đã bị vô hiệu hóa.");
            }
        }

        private static void BtnPlayClick()
        {
            // Kiểm tra xem có file save nào chưa
            if (!SaveSystem.HasAnySaveFile())
            {
                // Nếu chưa có file save, tạo thư mục và file auto save
                Debug.Log("Không tìm thấy file save. Đang tạo file auto save mới...");
                SaveSystem.CreateAutoSave();
            }
            
            // Load scene Level01
            SceneManager.LoadScene(nameof(SceneEnum.Level01));
        }

        private static void BtnContinueClick()
        {
            // Kiểm tra xem có file save không
            if (!SaveSystem.HasAnySaveFile())
            {
                Debug.LogWarning("Không có file save nào để tiếp tục!");
                return;
            }
            
            // Load auto save
            MadKnight.Save.PlayerSaveData saveData = SaveSystem.LoadAutoSave();
            
            if (saveData != null)
            {
                Debug.Log($"Đang load game... Scene: {saveData.currentScene}, Level: {saveData.playerLevel}");
                // Load scene từ save data
                SceneManager.LoadScene(saveData.currentScene);
            }
            else
            {
                Debug.LogError("Không thể load file save!");
            }
        }

        

        private static void BtnQuitClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}