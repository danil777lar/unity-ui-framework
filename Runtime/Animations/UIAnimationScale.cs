using Larje.Core.Tweening;
using UnityEngine;

namespace Larje.UI.Animations
{
    public sealed class UIAnimationScale : UIAnimationBase
    {
        [Header("Values")]
        [SerializeField] private Vector3 from = Vector3.one * 0.5f;
        [SerializeField] private Vector3 to = Vector3.one;
        [SerializeField] private bool setFromOnAwake = true;

        [Header("Timing")]
        [SerializeField] private DirectionalTween timing = new DirectionalTween();

        protected override float OnEvent(bool forward)
        {
            TweenSettings settings = timing.Get(forward);
            Vector3 target = forward ? to : from;
            return Play(settings, token => Tween.Vector3(transform.localScale, target, settings, x => transform.localScale = x, token));
        }

        private void Awake()
        {
            if (setFromOnAwake)
            {
                transform.localScale = from;
            }
        }
    }
}
