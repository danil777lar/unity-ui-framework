using Larje.Core.Haptics;
using UnityEngine;

namespace Larje.Core.UI.Animations
{
    public sealed class UIAnimationHaptic : UIAnimationBase
    {
        [SerializeField] private HapticId haptic;

        private IHapticService _haptics;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IHapticService haptics)
        {
            _haptics = haptics;
        }

        protected override float OnEvent(bool forward)
        {
            if (forward && _haptics != null)
            {
                _haptics.Play(haptic);
            }

            return 0f;
        }
    }
}
