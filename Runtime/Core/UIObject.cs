using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Larje.UI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class UIObject : MonoBehaviour
    {
        [Header("Behaviour")]
        [SerializeField] private bool useDeviceBackButton = true;
        [SerializeField] private bool blockNextBackInvokes = true;
        [SerializeField] private bool focusTarget = true;

        private bool _opened;
        private bool _closed;
        private bool _hidden;
        private bool _focused;
        private Canvas _canvas;
        private readonly List<IUIObjectEventDelay> _delays = new List<IUIObjectEventDelay>();

        public bool Opened => _opened;
        public bool Closed => _closed;
        public bool Hidden => _hidden;
        public bool Focused => _focused;
        public bool FocusTarget => focusTarget;
        public Canvas Canvas => _canvas != null ? _canvas : _canvas = GetComponent<Canvas>();
        public abstract string IdName { get; }

        public event Action BeforeOpen;
        public event Action AfterOpen;
        public event Action BeforeClose;
        public event Action AfterClose;
        public event Action BeforeShow;
        public event Action AfterShow;
        public event Action BeforeHide;
        public event Action AfterHide;
        public event Action BeforeFocus;
        public event Action AfterFocus;
        public event Action BeforeUnfocus;
        public event Action AfterUnfocus;

        public void Open(UIObjectArgs args)
        {
            if (_opened)
            {
                return;
            }

            _opened = true;
            _delays.Clear();
            _delays.AddRange(GetComponentsInChildren<IUIObjectEventDelay>(true));

            OnBeforeOpen(args);
            BeforeOpen?.Invoke();
            RunAfter(GetDelay(x => x.OnOpen()), () =>
            {
                OnAfterOpen(args);
                AfterOpen?.Invoke();
            });
        }

        public void Close()
        {
            if (_closed)
            {
                return;
            }

            _closed = true;
            OnBeforeClose();
            BeforeClose?.Invoke();
            RunAfter(_hidden ? 0f : GetDelay(x => x.OnClose()), () =>
            {
                OnAfterClose();
                AfterClose?.Invoke();
                Destroy(gameObject);
            });
        }

        public void Show()
        {
            if (!_hidden || _closed)
            {
                return;
            }

            _hidden = false;
            gameObject.SetActive(true);
            OnBeforeShow();
            BeforeShow?.Invoke();
            RunAfter(GetDelay(x => x.OnShow()), () =>
            {
                OnAfterShow();
                AfterShow?.Invoke();
            });
        }

        public void Hide()
        {
            if (_hidden || _closed)
            {
                return;
            }

            _hidden = true;
            OnBeforeHide();
            BeforeHide?.Invoke();
            RunAfter(GetDelay(x => x.OnHide()), () =>
            {
                OnAfterHide();
                AfterHide?.Invoke();
                if (_hidden)
                {
                    gameObject.SetActive(false);
                }
            });
        }

        public void Focus()
        {
            if (_focused || !focusTarget || _closed)
            {
                return;
            }

            _focused = true;
            OnBeforeFocus();
            BeforeFocus?.Invoke();
            RunAfter(GetDelay(x => x.OnFocus()), () =>
            {
                OnAfterFocus();
                AfterFocus?.Invoke();
            });
        }

        public void Unfocus()
        {
            if (!_focused || !focusTarget || _closed)
            {
                return;
            }

            _focused = false;
            OnBeforeUnfocus();
            BeforeUnfocus?.Invoke();
            RunAfter(GetDelay(x => x.OnUnfocus()), () =>
            {
                OnAfterUnfocus();
                AfterUnfocus?.Invoke();
            });
        }

        public void SetSortingOrder(int order)
        {
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = order;
        }

        public bool Back(bool onlyOverride = false)
        {
            return useDeviceBackButton && OnBack(onlyOverride) && blockNextBackInvokes;
        }

        protected virtual void OnBeforeOpen(UIObjectArgs args)
        {
        }

        protected virtual void OnAfterOpen(UIObjectArgs args)
        {
        }

        protected virtual void OnBeforeClose()
        {
        }

        protected virtual void OnAfterClose()
        {
        }

        protected virtual void OnBeforeShow()
        {
        }

        protected virtual void OnAfterShow()
        {
        }

        protected virtual void OnBeforeHide()
        {
        }

        protected virtual void OnAfterHide()
        {
        }

        protected virtual void OnBeforeFocus()
        {
        }

        protected virtual void OnAfterFocus()
        {
        }

        protected virtual void OnBeforeUnfocus()
        {
        }

        protected virtual void OnAfterUnfocus()
        {
        }

        protected virtual bool OnBack(bool onlyOverride)
        {
            if (_opened && !_closed && !onlyOverride)
            {
                Close();
                return true;
            }

            return false;
        }

        private float GetDelay(Func<IUIObjectEventDelay, float> selector)
        {
            float delay = 0f;
            foreach (IUIObjectEventDelay eventDelay in _delays)
            {
                delay = Mathf.Max(delay, selector(eventDelay));
            }

            return delay;
        }

        private void RunAfter(float delay, Action action)
        {
            if (delay <= 0f)
            {
                action();
                return;
            }

            RunAfterAsync(delay, action, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid RunAfterAsync(float delay, Action action, CancellationToken cancellationToken)
        {
            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
            if (!cancelled)
            {
                action();
            }
        }
    }
}
