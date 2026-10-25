using System;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public abstract class UIView
    {
        private VisualElement _root;
        private UIStates _states;

        public VisualElement Root => _root;
        public UIStates States => _states;

        public void Bind(VisualElement root)
        {
            Bind(root, new UIStates(root));
        }

        public void Bind(VisualElement root, UIStates states)
        {
            _root = root;
            _states = states;
            OnBind();
        }

        protected abstract void OnBind();

        protected T Find<T>(string path) where T : VisualElement
        {
            T element = UIQuery.Path<T>(_root, path);
            if (element == null)
            {
                throw new InvalidOperationException($"{GetType().Name}: element '{path}' ({typeof(T).Name}) not found. Re-export the mockup (design.py ui-export).");
            }

            return element;
        }
    }
}
