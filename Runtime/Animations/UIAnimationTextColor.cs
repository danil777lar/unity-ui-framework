using Larje.Core.Tweening;
using TMPro;
using UnityEngine;

namespace Larje.UI.Animations
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class UIAnimationTextColor : UIAnimationBase
    {
        [Header("Values")]
        [SerializeField] private Color from = Color.white;
        [SerializeField] private Color to = Color.white;
        [SerializeField] private bool setFromOnAwake = true;

        [Header("Timing")]
        [SerializeField] private float duration = 0.25f;

        private TMP_Text _text;

        protected override float OnEvent(bool forward)
        {
            TweenSettings settings = new TweenSettings(duration, Ease.Linear, 0f, true);
            Color target = forward ? to : from;
            return Play(settings, token => Tween.Color(_text.color, target, settings, x => _text.color = x, token));
        }

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (setFromOnAwake)
            {
                _text.color = from;
            }
        }
    }
}
