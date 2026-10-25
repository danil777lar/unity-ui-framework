using System;
using System.Threading;
using Larje.Core.Ids;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public abstract class UIObject
    {
        private bool _opened;
        private bool _closed;
        private bool _hidden;
        private bool _focused;
        private LarjeId _id;
        private VisualElement _root;
        private UIStates _states;
        private IUIService _service;
        private CancellationTokenSource _lifetime;

        public LarjeId Id => _id;
        public string Key => _id != null ? _id.Key : GetType().Name;
        public VisualElement Root => _root;
        public UIStates States => _states;
        public bool IsOpened => _opened;
        public bool IsClosed => _closed;
        public bool IsHidden => _hidden;
        public bool IsFocused => _focused;
        public CancellationToken Lifetime => _lifetime != null ? _lifetime.Token : CancellationToken.None;
        protected IUIService Service => _service;

        public event Action<UIObject> Opened;
        public event Action<UIObject> Closing;
        public event Action<UIObject> Closed;

        public void Close()
        {
            if (_service != null && !_closed)
            {
                _service.Close(this);
            }
        }

        internal void Create(LarjeId id, VisualElement root, IUIService service)
        {
            _id = id;
            _root = root;
            _service = service;
            _states = new UIStates(root);
            _lifetime = new CancellationTokenSource();
            OnCreated();
        }

        internal void NotifyOpening(UIObjectArgs args)
        {
            OnOpen(args ?? UIObjectArgs.Empty);
        }

        internal void NotifyOpened()
        {
            _opened = true;
            OnOpened();
            Opened?.Invoke(this);
        }

        internal bool NotifyClosing()
        {
            if (_closed)
            {
                return false;
            }

            _closed = true;
            OnClose();
            Closing?.Invoke(this);
            return true;
        }

        internal void NotifyClosed()
        {
            _lifetime?.Cancel();
            _lifetime?.Dispose();
            _lifetime = null;
            OnClosed();
            Closed?.Invoke(this);
        }

        internal void NotifyHidden(bool hidden)
        {
            _hidden = hidden;
            if (hidden)
            {
                OnHide();
            }
            else
            {
                OnShow();
            }
        }

        internal void NotifyFocus(bool focused)
        {
            if (_focused == focused)
            {
                return;
            }

            _focused = focused;
            if (focused)
            {
                OnFocus();
            }
            else
            {
                OnUnfocus();
            }
        }

        internal bool HandleBack()
        {
            return OnBack();
        }

        protected virtual void OnCreated()
        {
        }

        protected virtual void OnOpen(UIObjectArgs args)
        {
        }

        protected virtual void OnOpened()
        {
        }

        protected virtual void OnClose()
        {
        }

        protected virtual void OnClosed()
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnFocus()
        {
        }

        protected virtual void OnUnfocus()
        {
        }

        protected virtual bool OnBack()
        {
            return false;
        }
    }
}
