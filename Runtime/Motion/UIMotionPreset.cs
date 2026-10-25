using System.Collections.Generic;
using Larje.Core.Tweening;
using UnityEngine;

namespace Larje.UI
{
    public sealed class UIMotionPreset
    {
        public const string CLASS_PREFIX = "anim--";

        private static readonly Dictionary<string, UIMotionPreset> Presets = new Dictionary<string, UIMotionPreset>
        {
            { "fade-in", Appear(0.2f, Ease.OutQuad, 0f, Vector2.zero, 1f, 0f) },
            { "popup-in", Appear(0.28f, Ease.OutBack, 0f, Vector2.zero, 0.85f, 0f) },
            { "pop-in", Appear(0.35f, Ease.OutBack, 0f, Vector2.zero, 0f, 0f) },
            { "toast-in", Appear(0.3f, Ease.OutBack, 0f, new Vector2(0f, -80f), 1f, 0f) },
            { "slide-up-in", Appear(0.3f, Ease.OutCubic, 0f, new Vector2(0f, 120f), 1f, 0f) },
            { "slide-down-in", Appear(0.3f, Ease.OutCubic, 0f, new Vector2(0f, -120f), 1f, 0f) },
            { "slide-left-in", Appear(0.3f, Ease.OutCubic, 0f, new Vector2(160f, 0f), 1f, 0f) },
            { "slide-right-in", Appear(0.3f, Ease.OutCubic, 0f, new Vector2(-160f, 0f), 1f, 0f) },
            { "press", new UIMotionPreset(UIMotionKind.Press, 0.07f, Ease.OutQuad, 0f, Vector2.zero, 0.93f, 0f) },
            { "hover-grow", new UIMotionPreset(UIMotionKind.Hover, 0.12f, Ease.OutQuad, 0f, Vector2.zero, 1.06f, 0f) },
            { "idle-pulse", new UIMotionPreset(UIMotionKind.Idle, 1.2f, Ease.InOutSine, 0f, Vector2.zero, 1.06f, 0f) },
            { "idle-wiggle", new UIMotionPreset(UIMotionKind.Idle, 0.6f, Ease.InOutSine, 0f, Vector2.zero, 1f, 5f) },
            { "idle-float", new UIMotionPreset(UIMotionKind.Idle, 2f, Ease.InOutSine, 0f, new Vector2(0f, -12f), 1f, 0f) },
            { "idle-spin", new UIMotionPreset(UIMotionKind.Spin, 2f, Ease.Linear, 0f, Vector2.zero, 1f, 360f) },
            { "coin-fly", new UIMotionPreset(UIMotionKind.Marker, 0f, Ease.Linear, 0f, Vector2.zero, 1f, 0f) }
        };

        public static IEnumerable<string> Names => Presets.Keys;
        public UIMotionKind Kind { get; }
        public float Duration { get; }
        public Ease Ease { get; }
        public float Delay { get; }
        public Vector2 Offset { get; }
        public float Scale { get; }
        public float Rotation { get; }
        public bool FadesIn => Kind == UIMotionKind.Appear;

        public UIMotionPreset(UIMotionKind kind, float duration, Ease ease, float delay, Vector2 offset, float scale, float rotation)
        {
            Kind = kind;
            Duration = duration;
            Ease = ease;
            Delay = delay;
            Offset = offset;
            Scale = scale;
            Rotation = rotation;
        }

        public static bool TryGet(string name, out UIMotionPreset preset)
        {
            return Presets.TryGetValue(name, out preset);
        }

        private static UIMotionPreset Appear(float duration, Ease ease, float delay, Vector2 offset, float scale, float rotation)
        {
            return new UIMotionPreset(UIMotionKind.Appear, duration, ease, delay, offset, scale, rotation);
        }
    }

    public enum UIMotionKind
    {
        Appear,
        Press,
        Hover,
        Idle,
        Spin,
        Marker
    }
}
