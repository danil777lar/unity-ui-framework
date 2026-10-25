using System;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public abstract class UIToast : UIObject
    {
        public const string MESSAGE_NAME = "Message";

        private Func<bool> _keepOpen;

        public UIToastId ToastId => Id as UIToastId;
        public Func<bool> KeepOpen => _keepOpen;
        public virtual float Duration => -1f;
        public virtual bool CloseOnTap => true;

        protected override void OnCreated()
        {
            Root.RegisterCallback<PointerDownEvent>(OnPointerDown);
            OnToastCreated();
        }

        protected override void OnOpen(UIObjectArgs args)
        {
            UIToastArgs toastArgs = args as UIToastArgs;
            if (toastArgs != null)
            {
                _keepOpen = toastArgs.KeepOpen;
                Label message = Root.Q<Label>(MESSAGE_NAME);
                if (message != null && toastArgs.Text != null)
                {
                    message.text = toastArgs.Text;
                }
            }

            OnToastOpen(args);
        }

        protected virtual void OnToastCreated()
        {
        }

        protected virtual void OnToastOpen(UIObjectArgs args)
        {
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (CloseOnTap)
            {
                Close();
            }
        }
    }

    public abstract class UIToast<TView> : UIToast where TView : UIView, new()
    {
        private TView _view;

        protected TView View => _view;

        protected override void OnToastCreated()
        {
            _view = new TView();
            _view.Bind(Root, States);
            OnViewReady();
        }

        protected virtual void OnViewReady()
        {
        }
    }

    public sealed class DefaultToast : UIToast
    {
    }
}
