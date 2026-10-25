using UnityEngine;
using UnityEngine.UI;

namespace Larje.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class UIBackButton : MonoBehaviour
    {
        private Button _button;
        private UIObject _owner;
        private IUIService _ui;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IUIService ui)
        {
            _ui = ui;
        }

        private void Awake()
        {
            _owner = GetComponentInParent<UIObject>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            if (_ui == null || (_owner != null && _ui.Focused != _owner))
            {
                return;
            }

            _button.interactable = false;
            _ui.Back();
        }
    }
}
