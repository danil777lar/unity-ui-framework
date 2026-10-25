using UnityEngine.UIElements;

namespace Larje.UI
{
    public static class UIKit
    {
        public const string ROOT_CLASS = "larje-ui-root";

        private static readonly string[] ThemeClasses = { "unity-button", "unity-label", "unity-text-element" };

        public static void Prepare(VisualElement root)
        {
            root.AddToClassList(ROOT_CLASS);
            root.style.position = Position.Absolute;
            root.style.left = 0f;
            root.style.top = 0f;
            root.style.right = 0f;
            root.style.bottom = 0f;
            root.pickingMode = PickingMode.Ignore;
            root.Query<VisualElement>().ForEach(StripTheme);
        }

        private static void StripTheme(VisualElement element)
        {
            foreach (string className in ThemeClasses)
            {
                element.RemoveFromClassList(className);
            }
        }
    }
}
