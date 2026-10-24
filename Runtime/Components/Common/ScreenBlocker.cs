using UnityEngine;

namespace Larje.Core.UI
{
    public sealed class ScreenBlocker : MonoBehaviour
    {
        private UIObject _owner;

        private void Awake()
        {
            _owner = GetComponentInParent<UIObject>();
            if (_owner == null)
            {
                return;
            }

            _owner.BeforeOpen += Activate;
            _owner.BeforeShow += Activate;
            _owner.BeforeClose += Activate;
            _owner.AfterOpen += Deactivate;
            _owner.AfterShow += Deactivate;
        }

        private void OnDestroy()
        {
            if (_owner == null)
            {
                return;
            }

            _owner.BeforeOpen -= Activate;
            _owner.BeforeShow -= Activate;
            _owner.BeforeClose -= Activate;
            _owner.AfterOpen -= Deactivate;
            _owner.AfterShow -= Deactivate;
        }

        private void Activate()
        {
            gameObject.SetActive(true);
        }

        private void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}
