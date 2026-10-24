using System.Collections.Generic;
using UnityEngine;

namespace Larje.Core.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Larje/Core/UI/UI Config")]
    public sealed class UIConfig : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private bool useDeviceBackButton = true;
        [SerializeField] private int maxSortingOrder = 1000;
        [SerializeField] private bool persistAcrossScenes = true;

        [Header("Screens")]
        [SerializeField] private UIScreenId startScreen;
        [SerializeField] private int screensPriority;
        [SerializeField] private List<UIScreen> screens = new List<UIScreen>();

        [Header("Popups")]
        [SerializeField] private int popupsPriority = 10;
        [SerializeField] private List<UIPopup> popups = new List<UIPopup>();

        [Header("Toasts")]
        [SerializeField] private int toastsPriority = 20;
        [SerializeField] private List<UIToast> toasts = new List<UIToast>();

        public bool UseDeviceBackButton => useDeviceBackButton;
        public int MaxSortingOrder => maxSortingOrder;
        public bool PersistAcrossScenes => persistAcrossScenes;
        public UIScreenId StartScreen => startScreen;
        public int ScreensPriority => screensPriority;
        public IReadOnlyList<UIScreen> Screens => screens;
        public int PopupsPriority => popupsPriority;
        public IReadOnlyList<UIPopup> Popups => popups;
        public int ToastsPriority => toastsPriority;
        public IReadOnlyList<UIToast> Toasts => toasts;

        public UIScreen FindScreen(UIScreenId id)
        {
            return screens.Find(x => x != null && x.Id == id);
        }

        public UIPopup FindPopup(UIPopupId id)
        {
            return popups.Find(x => x != null && x.Id == id);
        }

        public UIToast FindToast(UIToastId id)
        {
            return toasts.Find(x => x != null && x.Id == id);
        }
    }
}
