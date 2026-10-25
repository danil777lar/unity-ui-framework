using System.Text;
using Larje.Core.Debugging;

namespace Larje.UI.Debugging
{
    public sealed class UIDebugCommands : IDebugCommandProvider
    {
        private readonly IUIService _ui;

        public UIDebugCommands(IUIService ui)
        {
            _ui = ui;
        }

        [DebugCommand("UI", "Open screen")]
        public string OpenScreen(UIScreenId screen)
        {
            return _ui.OpenScreen(screen) != null ? "Opened " + screen.Key : "Screen not configured";
        }

        [DebugCommand("UI", "Open popup")]
        public string OpenPopup(UIPopupId popup, UIPopupCombination combination = UIPopupCombination.Close)
        {
            return _ui.OpenPopup(popup, combination) != null ? "Opened " + popup.Key : "Popup not configured";
        }

        [DebugCommand("UI", "Show toast")]
        public string ShowToast(UIToastId toast, string text)
        {
            return _ui.ShowToast(toast, text) != null ? "Shown" : "Toast not configured";
        }

        [DebugCommand("UI", "Open screen, popup or toast by key")]
        public string Open(string key)
        {
            if (_ui.OpenScreen(key) != null)
            {
                return "Opened screen " + key;
            }

            if (_ui.OpenPopup(key) != null)
            {
                return "Opened popup " + key;
            }

            return _ui.ShowToast(key) != null ? "Shown toast " + key : "Nothing with key " + key;
        }

        [DebugCommand("UI", "Press back")]
        public bool Back()
        {
            return _ui.Back();
        }

        [DebugCommand("UI", "List opened UI objects")]
        public string ListOpened()
        {
            StringBuilder builder = new StringBuilder();
            foreach (UIObject uiObject in _ui.GetOpened())
            {
                builder.Append(uiObject.Key);
                if (uiObject == _ui.Focused)
                {
                    builder.Append(" [focused]");
                }

                if (uiObject.IsHidden)
                {
                    builder.Append(" [hidden]");
                }

                builder.Append('\n');
            }

            return builder.Length > 0 ? builder.ToString() : "Nothing opened";
        }
    }
}
