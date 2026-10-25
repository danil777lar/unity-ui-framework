using System.Threading;
using Larje.Core.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI
{
    internal sealed class UIMotionState
    {
        private bool _hasBase;
        private bool _appearing;
        private float _appearProgress = 1f;
        private float _baseRotate;
        private float _baseOpacity = 1f;
        private Vector2 _baseTranslate;
        private Vector3 _baseScale = Vector3.one;
        private UIMotionPreset _appear;
        private readonly VisualElement _element;
        private readonly TweenScope _pressScope;
        private readonly TweenScope _hoverScope;

        public float PressScale { get; set; } = 1f;
        public float HoverScale { get; set; } = 1f;
        public float IdleScale { get; set; } = 1f;
        public float IdleRotate { get; set; }
        public Vector2 IdleOffset { get; set; }

        public VisualElement Element => _element;
        public TweenScope PressScope => _pressScope;
        public TweenScope HoverScope => _hoverScope;

        public UIMotionState(VisualElement element, CancellationToken owner)
        {
            _element = element;
            _pressScope = new TweenScope(owner);
            _hoverScope = new TweenScope(owner);
        }

        public void BeginAppear(UIMotionPreset preset, float progress)
        {
            _appear = preset;
            _appearing = true;
            _appearProgress = progress;
            Apply();
        }

        public void SetAppearProgress(float progress)
        {
            _appearProgress = progress;
            Apply();
        }

        public void EndAppear(float progress)
        {
            _appearProgress = progress;
            _appearing = progress < 1f;
            Apply();
        }

        public void Apply()
        {
            CaptureBase();
            float appearScale = 1f;
            float appearRotate = 0f;
            float appearOpacity = 1f;
            Vector2 appearOffset = Vector2.zero;
            if (_appearing && _appear != null)
            {
                float progress = _appearProgress;
                appearScale = Mathf.LerpUnclamped(_appear.Scale, 1f, progress);
                appearRotate = Mathf.LerpUnclamped(_appear.Rotation, 0f, progress);
                appearOffset = Vector2.LerpUnclamped(_appear.Offset, Vector2.zero, progress);
                appearOpacity = _appear.FadesIn ? Mathf.Clamp01(progress) : 1f;
            }

            float scale = appearScale * PressScale * HoverScale * IdleScale;
            float rotate = appearRotate + IdleRotate;
            Vector2 offset = appearOffset + IdleOffset;
            bool transformed = !Mathf.Approximately(scale, 1f) || !Mathf.Approximately(rotate, 0f) || offset.sqrMagnitude > 0.0001f;
            if (transformed)
            {
                _element.style.scale = new StyleScale(new Scale(new Vector3(_baseScale.x * scale, _baseScale.y * scale, 1f)));
                _element.style.rotate = new StyleRotate(new Rotate(new Angle(_baseRotate + rotate, AngleUnit.Degree)));
                _element.style.translate = new StyleTranslate(new Translate(new Length(_baseTranslate.x + offset.x), new Length(_baseTranslate.y + offset.y)));
            }
            else
            {
                _element.style.scale = StyleKeyword.Null;
                _element.style.rotate = StyleKeyword.Null;
                _element.style.translate = StyleKeyword.Null;
            }

            if (_appearing && _appear != null && _appear.FadesIn)
            {
                _element.style.opacity = _baseOpacity * appearOpacity;
            }
            else
            {
                _element.style.opacity = StyleKeyword.Null;
            }

            if (!transformed && !_appearing)
            {
                _hasBase = false;
            }
        }

        private void CaptureBase()
        {
            if (_hasBase)
            {
                return;
            }

            _hasBase = true;
            IResolvedStyle resolved = _element.resolvedStyle;
            _baseScale = resolved.scale.value;
            _baseRotate = resolved.rotate.angle.ToDegrees();
            _baseTranslate = new Vector2(resolved.translate.x, resolved.translate.y);
            _baseOpacity = resolved.opacity;
        }
    }
}
