using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Larje.Core.UI
{
    public interface IUIService
    {
        UIConfig Config { get; }
        UIObject Focused { get; }
        UIScreenProcessor Screens { get; }
        UIPopupProcessor Popups { get; }
        UIToastProcessor Toasts { get; }

        event Action<UIObject> FocusChanged;

        UIScreen OpenScreen(UIScreenId id, UIObjectArgs args = null);
        UIPopup OpenPopup(UIPopupId id, UIPopupCombination combination = UIPopupCombination.Close, UIObjectArgs args = null);
        UIToast ShowToast(UIToastId id, string text, Func<bool> keepOpen = null);
        bool Back();
        void AddProcessor(UIProcessor processor);
        T GetProcessor<T>() where T : UIProcessor;
        IReadOnlyList<UIObject> GetOpened();
        void SetWorldCamera(Camera camera);
        void SetVirtualScreen(Func<PointerEventData, VirtualScreen.Hit> resolve);
    }
}
