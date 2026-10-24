using Larje.Core.Monetization;
using UnityEngine;

namespace Larje.Core.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class BannerSpace : MonoBehaviour
    {
        [SerializeField] private Mode mode = Mode.Height;

        private float _defaultOffset;
        private RectTransform _rect;
        private IAdsService _ads;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(IAdsService ads)
        {
            _ads = ads;
            _ads.BannerShown += Apply;
            _ads.BannerHidden += Apply;
            Apply();
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _defaultOffset = _rect.offsetMin.y;
        }

        private void OnDestroy()
        {
            if (_ads != null)
            {
                _ads.BannerShown -= Apply;
                _ads.BannerHidden -= Apply;
            }
        }

        private void Apply()
        {
            float height = _ads.BannerShowing ? _ads.BannerHeight : 0f;
            if (mode == Mode.Height)
            {
                _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, height);
            }
            else
            {
                _rect.offsetMin = new Vector2(_rect.offsetMin.x, _defaultOffset + height);
            }
        }

        public enum Mode
        {
            Height,
            BottomOffset
        }
    }
}
