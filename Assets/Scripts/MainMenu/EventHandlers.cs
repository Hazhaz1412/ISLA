using MadKnight.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MadKnight.MainMenu
{
    public class EventHandlers : MonoBehaviour
    {
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnOption;
        [SerializeField] private Button _btnQuit;

        [SerializeField] private GameObject _panelOption;

        private void Start()
        {
            _btnPlay.onClick.AddListener(BtnPlayClick);
            _btnContinue.onClick.AddListener(BtnContinueClick);
            _btnOption.onClick.AddListener(BtnOptionClick);
            _btnQuit.onClick.AddListener(BtnQuitClick);
        }

        private static void BtnPlayClick()
        {
            SceneManager.LoadScene(nameof(SceneEnum.Level01));
        }

        private static void BtnContinueClick()
        {
            Debug.LogWarning("This button have not been implemented");
        }

        private void BtnOptionClick()
        {
            _panelOption.SetActive(!_panelOption.activeSelf);
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