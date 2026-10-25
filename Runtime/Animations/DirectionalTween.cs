using System;
using Larje.Core.Tweening;

namespace Larje.UI.Animations
{
    [Serializable]
    public sealed class DirectionalTween
    {
        public float ForwardDuration = 0.25f;
        public Ease ForwardEase = Ease.OutBack;
        public float BackwardDuration = 0.25f;
        public Ease BackwardEase = Ease.InBack;

        public TweenSettings Get(bool forward)
        {
            return forward
                ? new TweenSettings(ForwardDuration, ForwardEase, 0f, true)
                : new TweenSettings(BackwardDuration, BackwardEase, 0f, true);
        }
    }
}
