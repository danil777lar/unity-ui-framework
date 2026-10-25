using System;

namespace Larje.UI
{
    public sealed class UIToastArgs : UIObjectArgs
    {
        public readonly string Text;
        public readonly Func<bool> KeepOpen;

        public UIToastArgs(string text, Func<bool> keepOpen = null)
        {
            Text = text;
            KeepOpen = keepOpen;
        }
    }
}
