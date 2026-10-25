using System;
using System.Collections.Generic;
using System.Threading;
using Larje.Core;
using Larje.Core.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Larje.UI
{
    public sealed class UIService : IUIService, IInitializableService, IDisposable
    {
        private const string ROOT_NAME = "[UI]";

        private Transform _root;
        private UIObject _focused;
        private readonly UIConfig _config;
        private readonly IInputService _input;
        private readonly UIScreenProcessor _screens;
        private readonly UIPopupProcessor _popups;
        private readonly UIToastProcessor _toasts;
        private readonly List<UIProcessor> _processors = new List<UIProcessor>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public UIConfig Config => _config;
        public UIObject Focused => _focused;
        public UIScreenProcessor Screens => _screens;
        public UIPopupProcessor Popups => _popups;
        public UIToastProcessor Toasts => _toasts;

        public event Action<UIObject> FocusChanged;

        public UIService(UIConfig config, IInputService input, IObjectInjector injector)
        {
            _config = config;
            _input = input;
            _screens = new UIScreenProcessor(config, injector);
            _popups = new UIPopupProcessor(config, injector);
            _toasts = new UIToastProcessor(config, injector);
        }

        public void Initialize()
        {
            GameObject root = new GameObject(ROOT_NAME);
            if (_config.PersistAcrossScenes)
            {
                Object.DontDestroyOnLoad(root);
            }

            _root = root.transform;
            AddProcessor(_screens);
            AddProcessor(_popups);
            AddProcessor(_toasts);
            ServiceLoop.Run(Tick, _lifetime.Token, Cysharp.Threading.Tasks.PlayerLoopTiming.Update, true);
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            if (_root != null)
            {
                Object.Destroy(_root.gameObject);
            }
        }

        public UIScreen OpenScreen(UIScreenId id, UIObjectArgs args = null)
        {
            return _screens.Open(id, args);
        }

        public UIPopup OpenPopup(UIPopupId id, UIPopupCombination combination = UIPopupCombination.Close, UIObjectArgs args = null)
        {
            return _popups.Open(id, combination, args);
        }

        public UIToast ShowToast(UIToastId id, string text, Func<bool> keepOpen = null)
        {
            return _toasts.Show(id, text, keepOpen);
        }

        public bool Back()
        {
            foreach (UIProcessor processor in GetByPriority())
            {
                if (processor.Back())
                {
                    return true;
                }
            }

            return false;
        }

        public void AddProcessor(UIProcessor processor)
        {
            if (_processors.Contains(processor))
            {
                return;
            }

            _processors.Add(processor);
            processor.OpenedChanged += ApplySortingOrders;
            processor.ShownChanged += ApplyFocus;
            processor.Initialize(_root);
        }

        public T GetProcessor<T>() where T : UIProcessor
        {
            foreach (UIProcessor processor in _processors)
            {
                if (processor is T typed)
                {
                    return typed;
                }
            }

            return null;
        }

        public IReadOnlyList<UIObject> GetOpened()
        {
            List<UIObject> opened = new List<UIObject>();
            foreach (UIProcessor processor in GetByPriority())
            {
                opened.AddRange(processor.OpenedObjects);
            }

            return opened;
        }

        public void SetWorldCamera(Camera camera)
        {
            _processors.ForEach(x => x.SetWorldCamera(camera));
        }

        public void SetVirtualScreen(Func<PointerEventData, VirtualScreen.Hit> resolve)
        {
            _processors.ForEach(x => x.SetVirtualScreen(resolve));
        }

        private void Tick(float deltaTime)
        {
            if (_config.UseDeviceBackButton && _input.Back != null && _input.Back.WasPerformedThisFrame())
            {
                Back();
            }
        }

        private void ApplySortingOrders()
        {
            int offset = 0;
            foreach (UIProcessor processor in GetByPriority())
            {
                offset += processor.ApplySortingOrders(_config.MaxSortingOrder, offset);
            }
        }

        private void ApplyFocus()
        {
            UIObject focused = null;
            foreach (UIProcessor processor in GetByPriority())
            {
                UIObject candidate = processor.ApplyFocus(focused == null);
                if (candidate != null)
                {
                    focused = candidate;
                }
            }

            if (_focused != focused)
            {
                _focused = focused;
                FocusChanged?.Invoke(focused);
            }
        }

        private List<UIProcessor> GetByPriority()
        {
            List<UIProcessor> sorted = new List<UIProcessor>(_processors);
            sorted.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            return sorted;
        }
    }
}
