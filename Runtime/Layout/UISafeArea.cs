using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public static class UISafeArea
    {
        public const string SAFE_CLASS = "safe";

        public static void Attach(VisualElement root)
        {
            root.Query<VisualElement>(className: SAFE_CLASS).ForEach(element =>
            {
                Insets design = null;
                element.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    design ??= Insets.From(element.resolvedStyle);
                    Apply(element, design);
                });
            });
        }

        public static Insets Device(IPanel panel)
        {
            Rect safe = Screen.safeArea;
            if (panel == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return new Insets(0f, 0f, 0f, 0f);
            }

            Vector2 size = panel.visualTree.layout.size;
            float scaleX = size.x / Screen.width;
            float scaleY = size.y / Screen.height;
            return new Insets(safe.xMin * scaleX, (Screen.height - safe.yMax) * scaleY, (Screen.width - safe.xMax) * scaleX, safe.yMin * scaleY);
        }

        private static void Apply(VisualElement element, Insets design)
        {
            Insets device = Device(element.panel);
            float left = Mathf.Max(design.Left, device.Left);
            float top = Mathf.Max(design.Top, device.Top);
            float right = Mathf.Max(design.Right, device.Right);
            float bottom = Mathf.Max(design.Bottom, device.Bottom);
            if (!Mathf.Approximately(element.resolvedStyle.left, left) || !Mathf.Approximately(element.resolvedStyle.top, top) ||
                !Mathf.Approximately(element.resolvedStyle.right, right) || !Mathf.Approximately(element.resolvedStyle.bottom, bottom))
            {
                element.style.left = left;
                element.style.top = top;
                element.style.right = right;
                element.style.bottom = bottom;
            }
        }

        public sealed class Insets
        {
            public float Left { get; }
            public float Top { get; }
            public float Right { get; }
            public float Bottom { get; }

            public Insets(float left, float top, float right, float bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public static Insets From(IResolvedStyle style)
            {
                return new Insets(Clean(style.left), Clean(style.top), Clean(style.right), Clean(style.bottom));
            }

            private static float Clean(float number)
            {
                return float.IsNaN(number) ? 0f : number;
            }
        }
    }
}
