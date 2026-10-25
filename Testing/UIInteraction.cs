using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI.Testing
{
    public static class UIInteraction
    {
        public static List<Button> Buttons()
        {
            List<Button> buttons = new List<Button>();
            foreach (UIDocument document in SceneObjects.All<UIDocument>())
            {
                if (document.isActiveAndEnabled && document.rootVisualElement != null)
                {
                    document.rootVisualElement.Query<Button>().ForEach(button =>
                    {
                        if (IsInteractable(button))
                        {
                            buttons.Add(button);
                        }
                    });
                }
            }

            return buttons;
        }

        public static Button Find(string nameOrText)
        {
            string wanted = nameOrText.ToLowerInvariant();
            foreach (Button button in Buttons())
            {
                if ((button.name ?? string.Empty).ToLowerInvariant().Contains(wanted) || (button.text ?? string.Empty).ToLowerInvariant().Contains(wanted))
                {
                    return button;
                }
            }

            return null;
        }

        public static void Click(Button button)
        {
            using (NavigationSubmitEvent evt = NavigationSubmitEvent.GetPooled())
            {
                evt.target = button;
                button.SendEvent(evt);
            }
        }

        public static string Describe(Button button)
        {
            return $"{button.name} \"{button.text}\"";
        }

        private static bool IsInteractable(Button button)
        {
            if (!button.enabledInHierarchy || button.worldBound.width <= 0f || button.worldBound.height <= 0f)
            {
                return false;
            }

            for (VisualElement current = button; current != null; current = current.parent)
            {
                if (current.resolvedStyle.display == DisplayStyle.None || current.resolvedStyle.visibility == Visibility.Hidden)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
