using System;
using UnityEngine;

namespace Larje.Core.UI
{
    public sealed class UIToastProcessor : UIProcessor
    {
        private UIToast _current;
        private readonly UIConfig _config;

        public UIToastProcessor(UIConfig config, IObjectInjector injector) : base(config.ToastsPriority, injector)
        {
            _config = config;
        }

        public UIToast Show(UIToastId id, string text, Func<bool> keepOpen = null)
        {
            UIToast prefab = _config.FindToast(id);
            if (prefab == null)
            {
                Debug.LogWarning($"UIToastProcessor: toast '{id}' not configured");
                return null;
            }

            if (_current != null)
            {
                _current.Close();
            }

            _current = Spawn(prefab, new UIToastArgs(text, keepOpen));
            return _current;
        }

        public override bool Back()
        {
            return _current != null && _current.Back();
        }

        protected override void OnClosed(UIObject uiObject)
        {
            if (_current == uiObject)
            {
                _current = null;
            }
        }
    }
}
