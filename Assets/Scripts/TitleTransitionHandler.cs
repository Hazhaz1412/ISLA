using UnityEngine;
using DG.Tweening;
using MadKnight.Enums;
using UnityEngine.SceneManagement;

namespace MadKnight
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TitleTransitionHandler : MonoBehaviour
    {
        [SerializeField] private float _transitionDuration;
        [SerializeField] private float _fadeOutDelay;

        private CanvasGroup _cvGroup;

        private Tween _fadeInTween;
        private Tween _fadeOutTween;
        private float _fadeOutDelayTimer;

        private void OnDestroy()
        {
            _fadeInTween.Kill();
            _fadeOutTween.Kill();
        }

        private void Awake()
        {
            _cvGroup = GetComponent<CanvasGroup>();
            _fadeInTween = _cvGroup.DOFade(1, _transitionDuration).Pause().SetAutoKill(false);
            _fadeOutTween = _cvGroup.DOFade(0, _transitionDuration).Pause().SetAutoKill(false);
            _fadeOutDelayTimer = _fadeOutDelay;
        }

        private void Start()
        {
            _cvGroup.alpha = 0;
            _fadeInTween.Play();
        }

        private void Update()
        {
            if (_fadeOutTween.IsComplete())
            {
                SceneManager.LoadScene(nameof(SceneEnum.MainMenu2));
                return;
            }

            if (_fadeInTween.IsPlaying() && _fadeOutTween.IsPlaying())
            {
                return;
            }

            _fadeOutDelayTimer -= Time.deltaTime;

            if (_fadeOutDelayTimer <= 0 || Input.anyKeyDown)
            {
                _fadeOutTween.Play();
            }
        }
    }
}