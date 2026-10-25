using System;
using System.Collections.Generic;
using Larje.Core;
using Larje.Core.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Larje/UI/UI Config")]
    public sealed class UIConfig : ScriptableObject, IConfigValidatable
    {
        [Header("Panel")]
        [SerializeField] private PanelSettings panelSettings;
        [SerializeField] private bool persistAcrossScenes = true;
        [SerializeField] private bool useDeviceBackButton = true;

        [Header("Views")]
        [SerializeField] private UIScreenId startScreen;
        [SerializeField] private List<UIViewEntry<UIScreenId>> screens = new List<UIViewEntry<UIScreenId>>();
        [SerializeField] private List<UIViewEntry<UIPopupId>> popups = new List<UIViewEntry<UIPopupId>>();
        [SerializeField] private List<UIViewEntry<UIToastId>> toasts = new List<UIViewEntry<UIToastId>>();

        [Header("Motion")]
        [SerializeField] private float motionSpeed = 1f;
        [SerializeField] private float toastDuration = 2f;

        [Header("Sync")]
        [SerializeField] private string viewsFolder = "Assets/_Project/UI";
        [SerializeField] private string idsFolder = "Assets/_Project/Configs/Ids";

        public PanelSettings PanelSettings => panelSettings;
        public bool PersistAcrossScenes => persistAcrossScenes;
        public bool UseDeviceBackButton => useDeviceBackButton;
        public UIScreenId StartScreen => startScreen;
        public IReadOnlyList<UIViewEntry<UIScreenId>> Screens => screens;
        public IReadOnlyList<UIViewEntry<UIPopupId>> Popups => popups;
        public IReadOnlyList<UIViewEntry<UIToastId>> Toasts => toasts;
        public float MotionSpeed => motionSpeed;
        public float ToastDuration => toastDuration;
        public string ViewsFolder => viewsFolder;
        public string IdsFolder => idsFolder;

        public VisualTreeAsset FindLayout(LarjeId id)
        {
            return Find(screens, id) ?? Find(popups, id) ?? Find(toasts, id);
        }

        public T FindId<T>(string key) where T : LarjeId
        {
            foreach (UIViewEntry<UIScreenId> entry in screens)
            {
                if (entry.Id is T typed && typed.Key == key)
                {
                    return typed;
                }
            }

            foreach (UIViewEntry<UIPopupId> entry in popups)
            {
                if (entry.Id is T typed && typed.Key == key)
                {
                    return typed;
                }
            }

            foreach (UIViewEntry<UIToastId> entry in toasts)
            {
                if (entry.Id is T typed && typed.Key == key)
                {
                    return typed;
                }
            }

            return null;
        }

        public void Validate(List<string> problems)
        {
            if (panelSettings == null)
            {
                problems.Add("PanelSettings is not assigned");
            }

            Check(screens, "screen", problems);
            Check(popups, "popup", problems);
            Check(toasts, "toast", problems);
        }

        private VisualTreeAsset Find<T>(List<UIViewEntry<T>> entries, LarjeId id) where T : LarjeId
        {
            foreach (UIViewEntry<T> entry in entries)
            {
                if (entry.Id == id)
                {
                    return entry.Layout;
                }
            }

            return null;
        }

        private void Check<T>(List<UIViewEntry<T>> entries, string kind, List<string> problems) where T : LarjeId
        {
            HashSet<T> seen = new HashSet<T>();
            foreach (UIViewEntry<T> entry in entries)
            {
                if (entry.Id == null || entry.Layout == null)
                {
                    problems.Add($"{kind} entry without id or layout");
                }
                else if (!seen.Add(entry.Id))
                {
                    problems.Add($"{kind} {entry.Id.Key} is listed twice");
                }
            }
        }
    }

    [Serializable]
    public sealed class UIViewEntry<T> where T : LarjeId
    {
        [SerializeField] private T id;
        [SerializeField] private VisualTreeAsset layout;

        public T Id => id;
        public VisualTreeAsset Layout => layout;

        public UIViewEntry(T id, VisualTreeAsset layout)
        {
            this.id = id;
            this.layout = layout;
        }
    }
}
