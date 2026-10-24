using TMPro;
using UnityEngine;

namespace Larje.Core.UI.Animations
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class UIAnimationTextBold : UIAnimationBase
    {
        [SerializeField] private bool from;
        [SerializeField] private bool to = true;
        [SerializeField] private bool setFromOnAwake = true;

        private TMP_Text _text;

        protected override float OnEvent(bool forward)
        {
            Apply(forward ? to : from);
            return 0f;
        }

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (setFromOnAwake)
            {
                Apply(from);
            }
        }

        private void Apply(bool bold)
        {
            if (bold)
            {
                _text.fontStyle |= FontStyles.Bold;
            }
            else
            {
                _text.fontStyle &= ~FontStyles.Bold;
            }
        }
    }
}
