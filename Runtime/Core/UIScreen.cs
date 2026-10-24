using UnityEngine;

namespace Larje.Core.UI
{
    public class UIScreen : UIObject
    {
        [Header("Screen")]
        [SerializeField] private UIScreenId id;
        [SerializeField] private bool clearHistoryOnOpen;

        public UIScreenId Id => id;
        public bool ClearHistoryOnOpen => clearHistoryOnOpen;
        public override string IdName => id != null ? id.name : name;
    }
}
