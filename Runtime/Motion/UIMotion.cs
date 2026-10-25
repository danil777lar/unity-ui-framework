using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public sealed class UIMotion
    {
        public const string PRESSED_CLASS = "is-pressed";

        private readonly float _speed;
        private readonly Dictionary<VisualElement, UIMotionState> _states = new Dictionary<VisualElement, UIMotionState>();

        public UIMotion(float speed = 1f)
        {
            _speed = speed > 0f ? speed : 1f;
        }

        public static List<KeyValuePair<VisualElement, UIMotionPreset>> Find(VisualElement root, UIMotionKind kind)
        {
            List<KeyValuePair<VisualElement, UIMotionPreset>> result = new List<KeyValuePair<VisualElement, UIMotionPreset>>();
            root.Query<VisualElement>().ForEach(element =>
            {
                foreach (string className in element.GetClasses())
                {
                    if (className.StartsWith(UIMotionPreset.CLASS_PREFIX) &&
                        UIMotionPreset.TryGet(className.Substring(UIMotionPreset.CLASS_PREFIX.Length), out UIMotionPreset preset) &&
                        preset.Kind == kind)
                    {
                        result.Add(new KeyValuePair<VisualElement, UIMotionPreset>(element, preset));
                    }
                }
            });
            return result;
        }

        public void Attach(VisualElement root, CancellationToken token)
        {
            foreach (KeyValuePair<VisualElement, UIMotionPreset> pair in Find(root, UIMotionKind.Press))
            {
                UIMotionState state = State(pair.Key, token);
                UIMotionPreset preset = pair.Value;
                pair.Key.RegisterCallback<PointerDownEvent>(evt => Press(state, preset, true), TrickleDown.TrickleDown);
                pair.Key.RegisterCallback<PointerUpEvent>(evt => Press(state, preset, false), TrickleDown.TrickleDown);
                pair.Key.RegisterCallback<PointerLeaveEvent>(evt => Press(state, preset, false));
                pair.Key.RegisterCallback<PointerCancelEvent>(evt => Press(state, preset, false));
            }

            foreach (KeyValuePair<VisualElement, UIMotionPreset> pair in Find(root, UIMotionKind.Hover))
            {
                UIMotionState state = State(pair.Key, token);
                UIMotionPreset preset = pair.Value;
                pair.Key.RegisterCallback<PointerEnterEvent>(evt => Hover(state, preset, true));
                pair.Key.RegisterCallback<PointerLeaveEvent>(evt => Hover(state, preset, false));
            }
        }

        public async UniTask PlayIn(VisualElement root, CancellationToken token)
        {
            root.style.opacity = 0f;
            await UniTask.NextFrame(token);
            List<KeyValuePair<VisualElement, UIMotionPreset>> appear = Find(root, UIMotionKind.Appear);
            foreach (KeyValuePair<VisualElement, UIMotionPreset> pair in appear)
            {
                State(pair.Key, token).BeginAppear(pair.Value, 0f);
            }

            root.style.opacity = StyleKeyword.Null;
            List<UniTask> tasks = new List<UniTask>();
            foreach (KeyValuePair<VisualElement, UIMotionPreset> pair in appear)
            {
                UIMotionState state = State(pair.Key, token);
                TweenSettings settings = new TweenSettings(pair.Value.Duration / _speed, pair.Value.Ease, pair.Value.Delay / _speed, true);
                tasks.Add(Animate(state, 0f, 1f, settings, token));
            }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask PlayOut(VisualElement root, CancellationToken token)
        {
            List<UniTask> tasks = new List<UniTask>();
            foreach (KeyValuePair<VisualElement, UIMotionPreset> pair in Find(root, UIMotionKind.Appear))
            {
                UIMotionState state = State(pair.Key, token);
                state.BeginAppear(pair.Value, 1f);
                TweenSettings settings = new TweenSettings(pair.Value.Duration * 0.6f / _speed, Ease.InQuad, 0f, true);
                tasks.Add(Animate(state, 1f, 0f, settings, token));
            }

            await UniTask.WhenAll(tasks);
        }

        public void StartIdle(VisualElement root, CancellationToken token)
        {
            foreach (KeyValuePair<VisualElement, UIMotionPreset> pair in Find(root, UIMotionKind.Idle))
            {
                IdleLoop(State(pair.Key, token), pair.Value, token).Forget();
            }

            foreach (KeyValuePair<VisualElement, UIMotionPreset> pair in Find(root, UIMotionKind.Spin))
            {
                SpinLoop(State(pair.Key, token), pair.Value, token).Forget();
            }
        }

        public void Release(VisualElement root)
        {
            root.Query<VisualElement>().ForEach(element => _states.Remove(element));
        }

        private UIMotionState State(VisualElement element, CancellationToken token)
        {
            if (!_states.TryGetValue(element, out UIMotionState state))
            {
                state = new UIMotionState(element, token);
                _states.Add(element, state);
            }

            return state;
        }

        private async UniTask Animate(UIMotionState state, float from, float to, TweenSettings settings, CancellationToken token)
        {
            bool cancelled = await Tween.Float(from, to, settings, state.SetAppearProgress, token).SuppressCancellationThrow();
            if (!cancelled)
            {
                state.EndAppear(to);
            }
        }

        private void Press(UIMotionState state, UIMotionPreset preset, bool down)
        {
            if (state.Element.ClassListContains("btn"))
            {
                state.Element.EnableInClassList(PRESSED_CLASS, down);
            }

            float target = down ? preset.Scale : 1f;
            TweenSettings settings = down
                ? new TweenSettings(preset.Duration / _speed, preset.Ease, 0f, true)
                : new TweenSettings(0.18f / _speed, Ease.OutBack, 0f, true);
            Tween.Float(state.PressScale, target, settings, x => SetPress(state, x), state.PressScope.Restart()).SuppressCancellationThrow().Forget();
        }

        private void Hover(UIMotionState state, UIMotionPreset preset, bool inside)
        {
            TweenSettings settings = new TweenSettings(preset.Duration / _speed, preset.Ease, 0f, true);
            Tween.Float(state.HoverScale, inside ? preset.Scale : 1f, settings, x => SetHover(state, x), state.HoverScope.Restart()).SuppressCancellationThrow().Forget();
        }

        private void SetPress(UIMotionState state, float scale)
        {
            state.PressScale = scale;
            state.Apply();
        }

        private void SetHover(UIMotionState state, float scale)
        {
            state.HoverScale = scale;
            state.Apply();
        }

        private async UniTaskVoid IdleLoop(UIMotionState state, UIMotionPreset preset, CancellationToken token)
        {
            float half = preset.Duration / _speed * 0.5f;
            TweenSettings settings = new TweenSettings(half, preset.Ease, 0f, true);
            bool wiggle = !Mathf.Approximately(preset.Rotation, 0f);
            while (!token.IsCancellationRequested)
            {
                if (wiggle)
                {
                    await Tween.Delay(1.6f / _speed, true, token).SuppressCancellationThrow();
                    await Tween.Float(0f, preset.Rotation, settings, x => SetIdle(state, 1f, x, Vector2.zero), token).SuppressCancellationThrow();
                    await Tween.Float(preset.Rotation, -preset.Rotation, settings, x => SetIdle(state, 1f, x, Vector2.zero), token).SuppressCancellationThrow();
                    await Tween.Float(-preset.Rotation, 0f, settings, x => SetIdle(state, 1f, x, Vector2.zero), token).SuppressCancellationThrow();
                }
                else
                {
                    await Tween.Float(0f, 1f, settings, x => SetIdle(state, Mathf.Lerp(1f, preset.Scale, x), 0f, preset.Offset * x), token).SuppressCancellationThrow();
                    await Tween.Float(1f, 0f, settings, x => SetIdle(state, Mathf.Lerp(1f, preset.Scale, x), 0f, preset.Offset * x), token).SuppressCancellationThrow();
                }
            }
        }

        private async UniTaskVoid SpinLoop(UIMotionState state, UIMotionPreset preset, CancellationToken token)
        {
            TweenSettings settings = new TweenSettings(preset.Duration / _speed, Ease.Linear, 0f, true);
            while (!token.IsCancellationRequested)
            {
                await Tween.Float(0f, preset.Rotation, settings, x => SetIdle(state, 1f, x, Vector2.zero), token).SuppressCancellationThrow();
            }
        }

        private void SetIdle(UIMotionState state, float scale, float rotate, Vector2 offset)
        {
            state.IdleScale = scale;
            state.IdleRotate = rotate;
            state.IdleOffset = offset;
            state.Apply();
        }
    }
}
