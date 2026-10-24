using Larje.Core.Audio;
using UnityEngine;

namespace Larje.Core.UI.Animations
{
    public sealed class UIAnimationSound : UIAnimationBase
    {
        [SerializeField] private SoundId forwardSound;
        [SerializeField] private SoundId backwardSound;

        private ISoundService _sounds;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ISoundService sounds)
        {
            _sounds = sounds;
        }

        protected override float OnEvent(bool forward)
        {
            SoundId sound = forward ? forwardSound : backwardSound;
            if (_sounds != null && sound != null)
            {
                _sounds.Play(sound);
            }

            return 0f;
        }
    }
}
