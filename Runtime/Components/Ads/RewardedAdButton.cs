using Larje.Core.Monetization;
using UnityEngine;
using UnityEngine.UI;

namespace Larje.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class RewardedAdButton : MonoBehaviour
    {
        [SerializeField] private GameObject unavailableLocker;

        private bool _checking = true;
        private Button _button;
        private IAdsService _ads;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IAdsService ads)
        {
            _ads = ads;
        }

        public void SetChecking(bool checking)
        {
            _checking = checking;
            if (!checking)
            {
                Apply(true);
            }
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Update()
        {
            if (_checking && _ads != null)
            {
                Apply(_ads.RewardedAvailable);
            }
        }

        private void Apply(bool available)
        {
            _button.interactable = available;
            if (unavailableLocker != null)
            {
                unavailableLocker.SetActive(!available);
            }
        }
    }
}
