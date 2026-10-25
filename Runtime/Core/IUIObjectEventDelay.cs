namespace Larje.UI
{
    public interface IUIObjectEventDelay
    {
        float OnOpen();
        float OnClose();
        float OnShow();
        float OnHide();
        float OnFocus();
        float OnUnfocus();
    }
}
