using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public static class UIGaps
    {
        public const string CLASS_PREFIX = "gapc--";

        public static void Apply(VisualElement root)
        {
            HashSet<VisualElement> containers = new HashSet<VisualElement>();
            root.Query<VisualElement>().ForEach(element =>
            {
                if (element.parent != null && TryParse(element, out bool _, out float _, out float _))
                {
                    containers.Add(element.parent);
                }
            });
            foreach (VisualElement container in containers)
            {
                ApplyContainer(container);
            }
        }

        private static void ApplyContainer(VisualElement container)
        {
            bool first = true;
            foreach (VisualElement child in container.Children())
            {
                if (TryParse(child, out bool row, out float gap, out float own) && !IsHidden(child))
                {
                    float margin = first ? own : own + gap;
                    if (row)
                    {
                        child.style.marginLeft = margin;
                    }
                    else
                    {
                        child.style.marginTop = margin;
                    }

                    first = false;
                }
            }
        }

        private static bool IsHidden(VisualElement element)
        {
            return element.style.display.keyword == StyleKeyword.Undefined && element.style.display.value == DisplayStyle.None;
        }

        private static bool TryParse(VisualElement element, out bool row, out float gap, out float own)
        {
            row = false;
            gap = 0f;
            own = 0f;
            foreach (string className in element.GetClasses())
            {
                if (className.StartsWith(CLASS_PREFIX))
                {
                    string[] parts = className.Substring(CLASS_PREFIX.Length).Split(new[] { "--" }, System.StringSplitOptions.None);
                    if (parts.Length == 3)
                    {
                        row = parts[0] == "left";
                        gap = Number(parts[1]);
                        own = Number(parts[2]);
                        return true;
                    }
                }
            }

            return false;
        }

        private static float Number(string text)
        {
            return float.Parse(text.Replace("n", "-").Replace("_", "."), CultureInfo.InvariantCulture);
        }
    }
}
