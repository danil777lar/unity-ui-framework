using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Larje.Core.UI
{
    [RequireComponent(typeof(Slider))]
    public sealed class SliderBlink : MonoBehaviour
    {
        [Header("Blink")]
        [SerializeField] private Vector3 addScale = Vector3.one * 0.1f;
        [SerializeField] private float blinkDuration = 0.5f;
        [SerializeField, Range(0f, 1f)] private float blinkBelow = 0.3f;

        [Header("Curves")]
        [SerializeField] private AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve frequencyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Colors")]
        [SerializeField] private Color minColor = Color.red;
        [SerializeField] private Color maxColor = Color.green;
        [SerializeField] private Color blinkColor = Color.white;

        private bool _blinking;
        private float _blink;
        private Vector3 _defaultScale;
        private Slider _slider;
        private Image _fill;

        private float Percent => Mathf.InverseLerp(_slider.minValue, _slider.maxValue, _slider.value);

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _fill = _slider.fillRect != null ? _slider.fillRect.GetComponent<Image>() : null;
            _defaultScale = transform.localScale;
        }

        private void OnDisable()
        {
            _blinking = false;
            _blink = 0f;
            transform.localScale = _defaultScale;
        }

        private void Update()
        {
            if (_fill != null)
            {
                Color baseColor = Color.Lerp(minColor, maxColor, colorCurve.Evaluate(Percent));
                _fill.color = Color.Lerp(baseColor, blinkColor, _blink);
            }

            if (!_blinking && Percent <= blinkBelow)
            {
                _blinking = true;
                BlinkLoopAsync(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow().Forget();
            }
        }

        private async UniTask BlinkLoopAsync(CancellationToken cancellationToken)
        {
            while (_blinking && Percent <= blinkBelow)
            {
                float duration = Mathf.Max(0.05f, frequencyCurve.Evaluate(Percent) * blinkDuration);
                TweenSettings settings = new TweenSettings(duration, Ease.InOutQuad, 0f, true);
                await Tween.Progress(settings, t =>
                {
                    _blink = blinkCurve.Evaluate(t);
                    transform.localScale = _defaultScale + addScale * _blink;
                }, cancellationToken);
                await Tween.Delay(duration, true, cancellationToken);
            }

            _blinking = false;
            _blink = 0f;
            transform.localScale = _defaultScale;
        }
    }
}
