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
                builder.Append(uiObject.IdName);
                if (uiObject == _ui.Focused)
                {
                    builder.Append(" [focused]");
                }

                if (uiObject.Hidden)
                {
                    builder.Append(" [hidden]");
                }

                builder.Append('\n');
            }

            return builder.Length > 0 ? builder.ToString() : "Nothing opened";
        }
    }
}
