using System;
using System.Collections.Generic;
using Larje.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Larje.UI
{
    public abstract class UIProcessor
    {
        private Transform _holder;
        private Camera _camera;
        private Func<PointerEventData, VirtualScreen.Hit> _virtualScreen;
        private readonly List<UIObject> _opened = new List<UIObject>();
        private readonly int _priority;
        private readonly IObjectInjector _injector;

        public int Priority => _priority;
        public IReadOnlyList<UIObject> OpenedObjects => _opened;

        public event Action OpenedChanged;
        public event Action ShownChanged;

        protected UIProcessor(int priority, IObjectInjector injector)
        {
            _priority = priority;
            _injector = injector;
        }

        public void Initialize(Transform root)
        {
            _holder = new GameObject(GetType().Name).transform;
            _holder.SetParent(root, false);
            OnInitialized();
        }

        public void SetWorldCamera(Camera camera)
        {
            _camera = camera;
            _opened.ForEach(ApplyCamera);
        }

        public void SetVirtualScreen(Func<PointerEventData, VirtualScreen.Hit> resolve)
        {
            _virtualScreen = resolve;
            _opened.ForEach(ApplyVirtualScreen);
        }

        public int ApplySortingOrders(int maxOrder, int offset)
        {
            int index = 0;
            for (int i = _opened.Count - 1; i >= 0; i--)
            {
                if (_opened[i] != null)
                {
                    index++;
                    _opened[i].SetSortingOrder(maxOrder - index - offset);
                }
            }

            return _opened.Count;
        }

        public UIObject ApplyFocus(bool canFocus)
        {
            UIObject focused = null;
            for (int i = _opened.Count - 1; i >= 0; i--)
            {
                UIObject uiObject = _opened[i];
                if (uiObject != null && uiObject.FocusTarget && !uiObject.Hidden)
                {
                    if (focused != null || !canFocus)
                    {
                        uiObject.Unfocus();
                    }
                    else
                    {
                        uiObject.Focus();
                        focused = uiObject;
                    }
                }
            }

            return focused;
        }

        public abstract bool Back();

        protected virtual void OnInitialized()
        {
        }

        protected T Spawn<T>(T prefab, UIObjectArgs args) where T : UIObject
        {
            GameObject instance = _injector.Instantiate(prefab.gameObject, _holder);
            T uiObject = instance.GetComponent<T>();
            Register(uiObject);
            uiObject.Open(args ?? UIObjectArgs.Empty);
            return uiObject;
        }

        protected virtual void OnClosed(UIObject uiObject)
        {
        }

        private void Register(UIObject uiObject)
        {
            _opened.Add(uiObject);
            ApplyCamera(uiObject);
            ApplyVirtualScreen(uiObject);
            uiObject.BeforeShow += NotifyShownChanged;
            uiObject.AfterHide += NotifyShownChanged;
            uiObject.AfterClose += () => Unregister(uiObject);
            OpenedChanged?.Invoke();
            ShownChanged?.Invoke();
        }

        private void Unregister(UIObject uiObject)
        {
            if (_opened.Remove(uiObject))
            {
                OnClosed(uiObject);
                OpenedChanged?.Invoke();
                ShownChanged?.Invoke();
            }
        }

        private void NotifyShownChanged()
        {
            ShownChanged?.Invoke();
        }

        private void ApplyCamera(UIObject uiObject)
        {
            uiObject.Canvas.renderMode = _camera != null ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
            uiObject.Canvas.worldCamera = _camera;
        }

        private void ApplyVirtualScreen(UIObject uiObject)
        {
            if (_virtualScreen == null)
            {
                return;
            }

            if (!uiObject.Canvas.TryGetComponent(out VirtualScreen screen))
            {
                screen = uiObject.Canvas.gameObject.AddComponent<VirtualScreen>();
            }

            screen.Initialize(_virtualScreen);
        }
    }
}
