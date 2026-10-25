using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core;
using Larje.Core.Ids;
using Larje.Core.Input;
using Object = UnityEngine.Object;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public sealed class UIService : IUIService, IInitializableService, IDisposable
    {
        private const string ROOT_NAME = "[UI]";

        private GameObject _host;
        private UIDocument _document;
        private VisualElement _screensLayer;
        private VisualElement _popupsLayer;
        private VisualElement _toastsLayer;
        private VisualElement _overlayLayer;
        private UIObject _focused;
        private readonly UIConfig _config;
        private readonly IInputService _input;
        private readonly IUIControllerFactory _factory;
        private readonly UIMotion _motion;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly List<UIScreen> _screens = new List<UIScreen>();
        private readonly List<UIPopup> _popups = new List<UIPopup>();
        private readonly List<UIToast> _toasts = new List<UIToast>();

        public UIConfig Config => _config;
        public VisualElement Root => _document != null ? _document.rootVisualElement : null;
        public VisualElement OverlayLayer => _overlayLayer;
        public UIScreen CurrentScreen => _screens.Count > 0 ? _screens[_screens.Count - 1] : null;
        public UIObject Focused => _focused;

        public event Action<UIObject> Opened;
        public event Action<UIObject> Closed;
        public event Action<UIObject> FocusChanged;

        public UIService(UIConfig config, IInputService input, IUIControllerFactory factory)
        {
            _config = config;
            _input = input;
            _factory = factory;
            _motion = new UIMotion(config.MotionSpeed);
        }

        public void Initialize()
        {
            _host = new GameObject(ROOT_NAME);
            _host.SetActive(false);
            _document = _host.AddComponent<UIDocument>();
            _document.panelSettings = _config.PanelSettings;
            _host.SetActive(true);
            if (_config.PersistAcrossScenes)
            {
                Object.DontDestroyOnLoad(_host);
            }

            VisualElement root = _document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            _screensLayer = CreateLayer(root, "Screens");
            _popupsLayer = CreateLayer(root, "Popups");
            _toastsLayer = CreateLayer(root, "Toasts");
            _overlayLayer = CreateLayer(root, "Overlay");
            ServiceLoop.Run(Tick, _lifetime.Token, PlayerLoopTiming.Update, true);
            if (_config.StartScreen != null)
            {
                OpenScreen(_config.StartScreen);
            }
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            if (_host != null)
            {
                Object.Destroy(_host);
            }
        }

        public UIScreen OpenScreen(UIScreenId id, UIObjectArgs args = null)
        {
            UIScreen screen = Create<UIScreen>(id, () => new DefaultScreen());
            if (screen == null)
            {
                return null;
            }

            if (screen.ClearHistoryOnOpen)
            {
                foreach (UIScreen opened in _screens.ToArray())
                {
                    Close(opened);
                }
            }
            else if (CurrentScreen != null)
            {
                SetHidden(CurrentScreen, true);
            }

            _screens.Add(screen);
            Show(screen, _screensLayer, args);
            return screen;
        }

        public UIScreen OpenScreen(string key, UIObjectArgs args = null)
        {
            return OpenScreen(RequireId<UIScreenId>(key), args);
        }

        public UIPopup OpenPopup(UIPopupId id, UIPopupCombination combination = UIPopupCombination.Close, UIObjectArgs args = null)
        {
            UIPopup popup = Create<UIPopup>(id, () => new DefaultPopup());
            if (popup == null)
            {
                return null;
            }

            UIPopup top = _popups.Count > 0 ? _popups[_popups.Count - 1] : null;
            if (top != null && combination == UIPopupCombination.Close)
            {
                Close(top);
            }
            else if (top != null && combination == UIPopupCombination.Hide)
            {
                SetHidden(top, true);
            }

            _popups.Add(popup);
            Show(popup, _popupsLayer, args);
            return popup;
        }

        public UIPopup OpenPopup(string key, UIPopupCombination combination = UIPopupCombination.Close, UIObjectArgs args = null)
        {
            return OpenPopup(RequireId<UIPopupId>(key), combination, args);
        }

        public UIToast ShowToast(UIToastId id, string text = null, Func<bool> keepOpen = null)
        {
            UIToast toast = Create<UIToast>(id, () => new DefaultToast());
            if (toast == null)
            {
                return null;
            }

            _toasts.Add(toast);
            Show(toast, _toastsLayer, new UIToastArgs(text, keepOpen));
            AutoClose(toast).Forget();
            return toast;
        }

        public UIToast ShowToast(string key, string text = null, Func<bool> keepOpen = null)
        {
            return ShowToast(RequireId<UIToastId>(key), text, keepOpen);
        }

        public void Close(UIObject target)
        {
            if (target == null || !target.NotifyClosing())
            {
                return;
            }

            bool wasCurrentScreen = target == CurrentScreen;
            _screens.Remove(target as UIScreen);
            _popups.Remove(target as UIPopup);
            _toasts.Remove(target as UIToast);
            if (wasCurrentScreen && CurrentScreen != null && CurrentScreen.IsHidden)
            {
                SetHidden(CurrentScreen, false);
            }

            if (target is UIPopup && _popups.Count > 0 && _popups[_popups.Count - 1].IsHidden)
            {
                SetHidden(_popups[_popups.Count - 1], false);
            }

            UpdateFocus();
            Remove(target).Forget();
        }

        public bool Back()
        {
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                if (_popups[i].HandleBack())
                {
                    return true;
                }
            }

            UIScreen current = CurrentScreen;
            if (current == null)
            {
                return false;
            }

            if (current.HandleBack())
            {
                return true;
            }

            if (_screens.Count > 1)
            {
                Close(current);
                return true;
            }

            return false;
        }

        public IReadOnlyList<UIObject> GetOpened()
        {
            List<UIObject> opened = new List<UIObject>();
            opened.AddRange(_screens);
            opened.AddRange(_popups);
            opened.AddRange(_toasts);
            return opened;
        }

        private VisualElement CreateLayer(VisualElement root, string layerName)
        {
            VisualElement layer = new VisualElement { name = layerName, pickingMode = PickingMode.Ignore };
            layer.style.position = Position.Absolute;
            layer.style.left = 0f;
            layer.style.top = 0f;
            layer.style.right = 0f;
            layer.style.bottom = 0f;
            root.Add(layer);
            return layer;
        }

        private T Create<T>(LarjeId id, Func<T> fallback) where T : UIObject
        {
            VisualTreeAsset layout = id != null ? _config.FindLayout(id) : null;
            if (layout == null)
            {
                Debug.LogWarning($"UIService: '{(id != null ? id.Key : "null")}' has no layout in UIConfig");
                return null;
            }

            UIObject created = _factory != null ? _factory.Create(id.Key) : null;
            if (created != null && !(created is T))
            {
                Debug.LogError($"UIService: controller for '{id.Key}' is {created.GetType().Name}, expected {typeof(T).Name}");
                return null;
            }

            T controller = created as T ?? fallback();
            VisualElement root = layout.Instantiate();
            root.name = id.Key;
            UIKit.Prepare(root);
            controller.Create(id, root, this);
            return controller;
        }

        private void Show(UIObject target, VisualElement layer, UIObjectArgs args)
        {
            layer.Add(target.Root);
            UISafeArea.Attach(target.Root);
            _motion.Attach(target.Root, target.Lifetime);
            target.NotifyOpening(args);
            Opened?.Invoke(target);
            UpdateFocus();
            PlayIn(target).Forget();
        }

        private async UniTaskVoid PlayIn(UIObject target)
        {
            bool cancelled = await _motion.PlayIn(target.Root, target.Lifetime).SuppressCancellationThrow();
            if (!cancelled && !target.IsClosed)
            {
                target.NotifyOpened();
                _motion.StartIdle(target.Root, target.Lifetime);
            }
        }

        private async UniTaskVoid Remove(UIObject target)
        {
            await _motion.PlayOut(target.Root, _lifetime.Token).SuppressCancellationThrow();
            _motion.Release(target.Root);
            target.Root.RemoveFromHierarchy();
            target.NotifyClosed();
            Closed?.Invoke(target);
        }

        private void SetHidden(UIObject target, bool hidden)
        {
            if (target.IsHidden == hidden)
            {
                return;
            }

            target.NotifyHidden(hidden);
            if (hidden)
            {
                HideAsync(target).Forget();
            }
            else
            {
                target.Root.style.display = DisplayStyle.Flex;
                _motion.PlayIn(target.Root, target.Lifetime).SuppressCancellationThrow().Forget();
            }

            UpdateFocus();
        }

        private async UniTaskVoid HideAsync(UIObject target)
        {
            bool cancelled = await _motion.PlayOut(target.Root, target.Lifetime).SuppressCancellationThrow();
            if (!cancelled && target.IsHidden)
            {
                target.Root.style.display = DisplayStyle.None;
            }
        }

        private async UniTaskVoid AutoClose(UIToast toast)
        {
            float duration = toast.Duration > 0f ? toast.Duration : _config.ToastDuration;
            CancellationToken token = toast.Lifetime;
            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(duration), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            if (!cancelled && toast.KeepOpen != null)
            {
                cancelled = await UniTask.WaitWhile(toast.KeepOpen, PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            }

            if (!cancelled)
            {
                Close(toast);
            }
        }

        private void UpdateFocus()
        {
            UIObject focused = null;
            for (int i = _popups.Count - 1; i >= 0 && focused == null; i--)
            {
                if (!_popups[i].IsHidden && !_popups[i].IsClosed)
                {
                    focused = _popups[i];
                }
            }

            if (focused == null && CurrentScreen != null && !CurrentScreen.IsHidden)
            {
                focused = CurrentScreen;
            }

            if (_focused == focused)
            {
                return;
            }

            _focused?.NotifyFocus(false);
            _focused = focused;
            _focused?.NotifyFocus(true);
            FocusChanged?.Invoke(_focused);
        }

        private T RequireId<T>(string key) where T : LarjeId
        {
            T id = _config.FindId<T>(key);
            if (id == null)
            {
                Debug.LogWarning($"UIService: no {typeof(T).Name} with key '{key}' in UIConfig");
            }

            return id;
        }

        private void Tick(float deltaTime)
        {
            if (_config.UseDeviceBackButton && _input != null && _input.Back != null && _input.Back.WasPerformedThisFrame())
            {
                Back();
            }
        }
    }
}
