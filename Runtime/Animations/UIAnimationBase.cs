using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core.Tweening;
using UnityEngine;

namespace Larje.Core.UI.Animations
{
    public abstract class UIAnimationBase : MonoBehaviour, IUIObjectEventDelay
    {
        [SerializeField] private List<UIAnimationEvent> events = new List<UIAnimationEvent>
        {
            UIAnimationEvent.RootOpen,
            UIAnimationEvent.RootClose
        };

        private TweenScope _scope;

        public float OnOpen()
        {
            return Process(UIAnimationEvent.RootOpen, true);
        }

        public float OnClose()
        {
            return Process(UIAnimationEvent.RootClose, false);
        }

        public float OnShow()
        {
            return Process(UIAnimationEvent.RootShow, true);
        }

        public float OnHide()
        {
            return Process(UIAnimationEvent.RootHide, false);
        }

        public float OnFocus()
        {
            return Process(UIAnimationEvent.RootFocus, true);
        }

        public float OnUnfocus()
        {
            return Process(UIAnimationEvent.RootUnfocus, false);
        }

        public float Process(UIAnimationEvent animationEvent, bool forward)
        {
            return events.Contains(animationEvent) ? OnEvent(forward) : 0f;
        }

        protected abstract float OnEvent(bool forward);

        protected float Play(TweenSettings settings, Func<CancellationToken, UniTask> tween)
        {
            _scope ??= new TweenScope(this.GetCancellationTokenOnDestroy());
            tween(_scope.Restart()).SuppressCancellationThrow().Forget();
            return settings.Duration + settings.Delay;
        }

        protected virtual void OnDisable()
        {
            _scope?.Kill();
        }
    }
}
