using System;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Core.UI
{
    public sealed class UIPopupProcessor : UIProcessor
    {
        private readonly UIConfig _config;
        private readonly List<UIPopup> _stack = new List<UIPopup>();

        public event Action<UIPopupId> Opened;
        public event Action<UIPopupId> Closed;

        public UIPopupProcessor(UIConfig config, IObjectInjector injector) : base(config.PopupsPriority, injector)
        {
            _config = config;
        }

        public UIPopup Open(UIPopupId id, UIPopupCombination combination = UIPopupCombination.Close, UIObjectArgs args = null)
        {
            UIPopup prefab = _config.FindPopup(id);
            if (prefab == null)
            {
                Debug.LogWarning($"UIPopupProcessor: popup '{id}' not configured");
                return null;
            }

            if (_stack.Count > 0)
            {
                UIPopup last = _stack[_stack.Count - 1];
                if (combination == UIPopupCombination.Close)
                {
                    last.Close();
                }
                else if (combination == UIPopupCombination.Hide)
                {
                    last.Hide();
                }
            }

            UIPopup instance = Spawn(prefab, args);
            _stack.Add(instance);
            Opened?.Invoke(id);
            return instance;
        }

        public override bool Back()
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i].Back())
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnClosed(UIObject uiObject)
        {
            UIPopup popup = (UIPopup)uiObject;
            bool wasTop = _stack.Count > 0 && _stack[_stack.Count - 1] == popup;
            _stack.Remove(popup);
            if (wasTop && _stack.Count > 0)
            {
                _stack[_stack.Count - 1].Show();
            }

            Closed?.Invoke(popup.Id);
        }
    }
}
