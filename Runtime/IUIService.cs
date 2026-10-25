using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public interface IUIService
    {
        UIConfig Config { get; }
        VisualElement Root { get; }
        VisualElement OverlayLayer { get; }
        UIScreen CurrentScreen { get; }
        UIObject Focused { get; }

        event Action<UIObject> Opened;
        event Action<UIObject> Closed;
        event Action<UIObject> FocusChanged;

        UIScreen OpenScreen(UIScreenId id, UIObjectArgs args = null);
        UIScreen OpenScreen(string key, UIObjectArgs args = null);
        UIPopup OpenPopup(UIPopupId id, UIPopupCombination combination = UIPopupCombination.Close, UIObjectArgs args = null);
        UIPopup OpenPopup(string key, UIPopupCombination combination = UIPopupCombination.Close, UIObjectArgs args = null);
        UIToast ShowToast(UIToastId id, string text = null, Func<bool> keepOpen = null);
        UIToast ShowToast(string key, string text = null, Func<bool> keepOpen = null);
        void Close(UIObject target);
        bool Back();
        IReadOnlyList<UIObject> GetOpened();
    }
}
