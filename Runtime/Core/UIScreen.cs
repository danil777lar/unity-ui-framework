namespace Larje.UI
{
    public abstract class UIScreen : UIObject
    {
        public UIScreenId ScreenId => Id as UIScreenId;
        public virtual bool ClearHistoryOnOpen => false;
    }

    public abstract class UIScreen<TView> : UIScreen where TView : UIView, new()
    {
        private TView _view;

        protected TView View => _view;

        protected override void OnCreated()
        {
            _view = new TView();
            _view.Bind(Root, States);
            OnViewReady();
        }

        protected virtual void OnViewReady()
        {
        }
    }

    public sealed class DefaultScreen : UIScreen
    {
    }
}
