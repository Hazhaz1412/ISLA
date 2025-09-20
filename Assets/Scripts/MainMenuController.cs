using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Optional Fade")]
    [SerializeField] CanvasGroup fadeGroup; // đặt alpha=1 ở Awake nếu muốn fade-in
    [SerializeField] float fadeDuration = 0.35f;

    [Header("First Selected")]
    [SerializeField] Button firstSelected;  // gán PlayButton

    void Start()
    {
        if (fadeGroup) { fadeGroup.alpha = 1f; StartCoroutine(Fade(0f)); }
        if (firstSelected) firstSelected.Select();
        Application.targetFrameRate = 60;
    }

    public void OnPlay()
    {
        StartCoroutine(LoadGame("Level01"));
    }

    public void OnOptions()
    {
        // Bật panel options (setActive true) hoặc mở popup
        var panel = GameObject.Find("OptionsPanel");
        if (panel) panel.SetActive(true);
    }

    public void OnQuit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    IEnumerator LoadGame(string sceneName)
    {
        if (fadeGroup) yield return Fade(1f);
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }

    IEnumerator Fade(float target)
    {
        float start = fadeGroup.alpha, t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            fadeGroup.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        fadeGroup.alpha = target;
    }
}
