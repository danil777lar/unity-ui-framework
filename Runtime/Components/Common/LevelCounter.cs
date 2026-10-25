using Larje.Core.Levels;
using Larje.Core.Localization;
using TMPro;
using UnityEngine;

namespace Larje.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LevelCounter : MonoBehaviour
    {
        [SerializeField] private int offset = 1;
        [SerializeField] private string prefixKey;
        [SerializeField] private string suffixKey;

        private ILevelService _levels;
        private ILocalizationService _localization;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ILevelService levels, ILocalizationService localization)
        {
            _levels = levels;
            _localization = localization;
            Refresh();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_levels == null)
            {
                return;
            }

            GetComponent<TMP_Text>().text = Localize(prefixKey) + (_levels.LevelNumber + offset) + Localize(suffixKey);
        }

        private string Localize(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return _localization != null ? _localization.Get(key) : key;
        }
    }
}
