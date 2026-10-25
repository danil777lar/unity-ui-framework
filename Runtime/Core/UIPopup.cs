using UnityEngine.UIElements;

namespace Larje.UI
{
    public abstract class UIPopup : UIObject
    {
        public const string BACKDROP_NAME = "Backdrop";
        public const string CLOSE_BUTTON_NAME = "CloseButton";

        public UIPopupId PopupId => Id as UIPopupId;
        public virtual bool CloseOnBackdrop => true;
        public virtual bool CloseOnBack => true;

        protected override void OnCreated()
        {
            VisualElement backdrop = Root.Q(BACKDROP_NAME);
            if (backdrop != null)
            {
                backdrop.RegisterCallback<ClickEvent>(OnBackdropClick);
            }

            Button closeButton = Root.Q<Button>(CLOSE_BUTTON_NAME);
            if (closeButton != null)
            {
                closeButton.clicked += Close;
            }

            OnPopupCreated();
        }

        protected override bool OnBack()
        {
            if (CloseOnBack)
            {
                Close();
                return true;
            }

            return false;
        }

        protected virtual void OnPopupCreated()
        {
        }

        private void OnBackdropClick(ClickEvent evt)
        {
            if (CloseOnBackdrop && evt.target == evt.currentTarget)
            {
                Close();
            }
        }
    }

    public abstract class UIPopup<TView> : UIPopup where TView : UIView, new()
    {
        private TView _view;

        protected TView View => _view;

        protected override void OnPopupCreated()
        {
            _view = new TView();
            _view.Bind(Root, States);
            OnViewReady();
        }

        protected virtual void OnViewReady()
        {
        }
    }

    public sealed class DefaultPopup : UIPopup
    {
    }
}
