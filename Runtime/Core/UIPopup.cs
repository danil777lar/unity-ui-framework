using UnityEngine;
using UnityEngine.EventSystems;

namespace Larje.Core.UI
{
    public class UIPopup : UIObject
    {
        [Header("Popup")]
        [SerializeField] private UIPopupId id;
        [SerializeField] private bool closeByBackgroundClick;
        [SerializeField] private RectTransformEvents background;

        public UIPopupId Id => id;
        public override string IdName => id != null ? id.name : name;

        protected override void OnBeforeOpen(UIObjectArgs args)
        {
            if (closeByBackgroundClick && background != null)
            {
                background.PointerClick += OnBackgroundClick;
            }
        }

        protected override void OnBeforeClose()
        {
            if (background != null)
            {
                background.PointerClick -= OnBackgroundClick;
            }
        }

        private void OnBackgroundClick(PointerEventData eventData)
        {
            Close();
        }
    }
}
