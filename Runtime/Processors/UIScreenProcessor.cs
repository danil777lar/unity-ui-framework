using System;
using System.Collections.Generic;
using Larje.Core;
using UnityEngine;

namespace Larje.UI
{
    public sealed class UIScreenProcessor : UIProcessor
    {
        private UIScreenId _last;
        private readonly UIConfig _config;
        private readonly List<UIScreen> _history = new List<UIScreen>();

        public UIScreenId Last => _last;
        public UIScreen Current => _history.Count > 0 ? _history[_history.Count - 1] : null;

        public event Action<UIScreenId> Opened;
        public event Action<UIScreenId> Closed;

        public UIScreenProcessor(UIConfig config, IObjectInjector injector) : base(config.ScreensPriority, injector)
        {
            _config = config;
        }

        public UIScreen Open(UIScreenId id, UIObjectArgs args = null)
        {
            UIScreen prefab = _config.FindScreen(id);
            if (prefab == null)
            {
                Debug.LogWarning($"UIScreenProcessor: screen '{id}' not configured");
                return null;
            }

            if (prefab.ClearHistoryOnOpen)
            {
                foreach (UIScreen screen in _history.ToArray())
                {
                    screen.Close();
                }
            }
            else if (Current != null)
            {
                Current.Hide();
            }

            _last = id;
            UIScreen instance = Spawn(prefab, args);
            _history.Add(instance);
            Opened?.Invoke(id);
            return instance;
        }

        public override bool Back()
        {
            for (int i = _history.Count - 1; i >= 0; i--)
            {
                if (_history[i].Back(i == 0))
                {
                    if (i > 0)
                    {
                        _history[i - 1].Show();
                    }

                    return true;
                }
            }

            return false;
        }

        protected override void OnInitialized()
        {
            if (_config.StartScreen != null)
            {
                Open(_config.StartScreen);
            }
        }

        protected override void OnClosed(UIObject uiObject)
        {
            UIScreen screen = (UIScreen)uiObject;
            _history.Remove(screen);
            Closed?.Invoke(screen.Id);
        }
    }
}
