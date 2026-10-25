using System;
using System.Collections.Generic;

namespace Larje.UI
{
    public sealed class UIControllerRegistry : IUIControllerFactory
    {
        private readonly Dictionary<string, Func<UIObject>> _factories = new Dictionary<string, Func<UIObject>>();

        public void Register(string key, Func<UIObject> create)
        {
            _factories[key] = create;
        }

        public UIObject Create(string key)
        {
            return _factories.TryGetValue(key, out Func<UIObject> create) ? create() : null;
        }
    }
}
