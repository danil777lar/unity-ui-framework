using System.Collections.Generic;
using Larje.Core;
using UnityEngine;

namespace Larje.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Larje/UI/UI Config")]
    public sealed class UIConfig : ScriptableObject, IConfigValidatable
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

        public void Validate(List<string> problems)
        {
            HashSet<UIScreenId> screenIds = new HashSet<UIScreenId>();
            foreach (UIScreen screen in screens)
            {
                if (screen == null)
                {
                    problems.Add("Screens list contains an empty entry");
                }
                else if (screen.Id == null)
                {
                    problems.Add($"Screen prefab '{screen.name}' has no UIScreenId");
                }
                else if (!screenIds.Add(screen.Id))
                {
                    problems.Add($"Screen id '{screen.Id.name}' is used by several prefabs");
                }
            }

            foreach (UIPopup popup in popups)
            {
                if (popup == null || popup.Id == null)
                {
                    problems.Add("Popup entry is empty or has no UIPopupId");
                }
            }

            foreach (UIToast toast in toasts)
            {
                if (toast == null || toast.Id == null)
                {
                    problems.Add("Toast entry is empty or has no UIToastId");
                }
            }

            if (startScreen != null && FindScreen(startScreen) == null)
            {
                problems.Add($"Start screen '{startScreen.name}' is not in the screens list");
            }
        }
    }
}
