using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Larje.Core.UI.Animations
{
    public sealed class UIEventsRoot : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        ISelectHandler, IDeselectHandler, IBeginDragHandler, IEndDragHandler
    {
        private bool _interactable = true;
        private Selectable _selectable;
        private List<UIAnimationBase> _animations;

        private List<UIAnimationBase> Animations => _animations ??= new List<UIAnimationBase>(GetComponentsInChildren<UIAnimationBase>(true));

        public void OnPointerDown(PointerEventData eventData)
        {
            Send(UIAnimationEvent.PointerDown, true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Send(UIAnimationEvent.PointerUp, false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Send(UIAnimationEvent.PointerEnter, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Send(UIAnimationEvent.PointerExit, false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Send(UIAnimationEvent.PointerClick, true);
        }

        public void OnSelect(BaseEventData eventData)
        {
            Send(UIAnimationEvent.SelectableSelect, true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Send(UIAnimationEvent.SelectableDeselect, false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Send(UIAnimationEvent.DraggableBeginDrag, true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Send(UIAnimationEvent.DraggableEndDrag, false);
        }

        private void OnEnable()
        {
            Send(UIAnimationEvent.GameObjectEnable, true);
        }

        private void OnDisable()
        {
            Send(UIAnimationEvent.GameObjectDisable, false);
        }

        private void Start()
        {
            _selectable = GetComponent<Selectable>();
            if (_selectable != null)
            {
                _interactable = _selectable.interactable;
                SendInteractable();
            }
        }

        private void Update()
        {
            if (_selectable != null && _interactable != _selectable.interactable)
            {
                _interactable = _selectable.interactable;
                SendInteractable();
            }
        }

        private void SendInteractable()
        {
            Send(_interactable ? UIAnimationEvent.SelectableInteractableOn : UIAnimationEvent.SelectableInteractableOff, _interactable);
        }

        private void Send(UIAnimationEvent animationEvent, bool forward)
        {
            foreach (UIAnimationBase animation in Animations)
            {
                animation.Process(animationEvent, forward);
            }
        }
    }
}
