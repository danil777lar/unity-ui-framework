using Larje.Core.Tweening;
using UnityEngine;

namespace Larje.UI.Animations
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UIAnimationCanvasGroup : UIAnimationBase
    {
        [Header("Values")]
        [SerializeField] private float from;
        [SerializeField] private float to = 1f;
        [SerializeField] private bool setFromOnAwake = true;

        [Header("Timing")]
        [SerializeField] private float duration = 0.25f;

        private CanvasGroup _group;

        protected override float OnEvent(bool forward)
        {
            TweenSettings settings = new TweenSettings(duration, Ease.Linear, 0f, true);
            float target = forward ? to : from;
            return Play(settings, token => Tween.Float(_group.alpha, target, settings, x => _group.alpha = x, token));
        }

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            if (setFromOnAwake)
            {
                _group.alpha = from;
            }
        }
    }
}
