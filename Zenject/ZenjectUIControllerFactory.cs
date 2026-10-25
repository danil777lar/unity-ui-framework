using System.Collections.Generic;
using Zenject;

namespace Larje.UI.Zenject
{
    public sealed class ZenjectUIControllerFactory : IUIControllerFactory
    {
        private readonly DiContainer _container;
        private readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();

        public ZenjectUIControllerFactory(DiContainer container, [InjectOptional] List<UIControllerBinding> bindings)
        {
            _container = container;
            if (bindings != null)
            {
                foreach (UIControllerBinding binding in bindings)
                {
                    _types[binding.Key] = binding.Type;
                }
            }
        }

        public UIObject Create(string key)
        {
            return _types.TryGetValue(key, out System.Type type) ? (UIObject)_container.Instantiate(type) : null;
        }
    }
}
