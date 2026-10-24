using Larje.Core.Tweening;
using UnityEngine;

namespace Larje.Core.UI.Animations
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIAnimationPosition : UIAnimationBase
    {
        [Header("Values")]
        [SerializeField] private Vector2 offsetFrom = new Vector2(0f, -200f);
        [SerializeField] private Vector2 offsetTo = Vector2.zero;
        [SerializeField] private bool setFromOnAwake = true;

        [Header("Timing")]
        [SerializeField] private DirectionalTween timing = new DirectionalTween();

        private Vector2 _from;
        private Vector2 _to;
        private RectTransform _rect;

        protected override float OnEvent(bool forward)
        {
            TweenSettings settings = timing.Get(forward);
            Vector2 target = forward ? _to : _from;
            return Play(settings, token => Tween.Vector2(_rect.anchoredPosition, target, settings, x => _rect.anchoredPosition = x, token));
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _from = _rect.anchoredPosition + offsetFrom;
            _to = _rect.anchoredPosition + offsetTo;
            if (setFromOnAwake)
            {
                _rect.anchoredPosition = _from;
            }
        }
    }
}
