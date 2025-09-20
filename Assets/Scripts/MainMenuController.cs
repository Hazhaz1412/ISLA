using System.Collections;
using MadKnight.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MadKnight
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _fadeGroup; // đặt alpha=1 ở Awake nếu muốn fade-in

        [SerializeField] private float _fadeDuration = 0.35f;

        [SerializeField] private Button _btnPlay; // gán PlayButton
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnOption;
        [SerializeField] private Button _btnQuit;

        [SerializeField] private GameObject _panelOption;

        private void Start()
        {
            Application.targetFrameRate = 60;

            // if (_fadeGroup)
            // {
            //     _fadeGroup.alpha = 1f;
            //     StartCoroutine(Fade(0f));
            // }

            _btnPlay.Select();
            _btnPlay.onClick.AddListener(BtnPlayClick);
            _btnContinue.onClick.AddListener(() =>
            {
                Debug.LogWarning("This button have not been implemented");
            });
            _btnOption.onClick.AddListener(BtnOptionClick);
            _btnQuit.onClick.AddListener(BtnQuitClick);
        }

        private void BtnPlayClick()
        {
            StartCoroutine(LoadGame(SceneEnum.Level01));
        }

        private void BtnOptionClick()
        {
            // Bật panel options (setActive true) hoặc mở popup
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

        private IEnumerator LoadGame(SceneEnum scene)
        {
            if (_fadeGroup) yield return Fade(1f);
            var op = SceneManager.LoadSceneAsync(scene.ToString());
            yield return new WaitUntil(() => op is { isDone: true });
        }

        private IEnumerator Fade(float target)
        {
            float start = _fadeGroup.alpha, t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _fadeDuration;
                _fadeGroup.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            _fadeGroup.alpha = target;
        }
    }
}