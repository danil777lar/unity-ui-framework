using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Larje.UI.Testing
{
    public static class UIPanelCapture
    {
        public static IEnumerator Capture(int width, int height, Action<Texture2D> done)
        {
            List<PanelSettings> settings = SceneObjects.All<UIDocument>()
                .Where(x => x.isActiveAndEnabled && x.panelSettings != null)
                .Select(x => x.panelSettings)
                .Distinct()
                .OrderBy(x => x.sortingOrder)
                .ToList();
            if (settings.Count == 0)
            {
                done(null);
                yield break;
            }

            List<PanelState> states = settings.Select(x => new PanelState(x, width, height)).ToList();
            for (int i = 0; i < 3; i++)
            {
                yield return null;
            }

            Color32[] result = new Color32[width * height];
            foreach (PanelState state in states)
            {
                Blend(result, state.Read());
                state.Restore();
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(result);
            texture.Apply();
            done(texture);
        }

        private static void Blend(Color32[] target, Color32[] layer)
        {
            for (int i = 0; i < target.Length; i++)
            {
                float alpha = layer[i].a / 255f;
                Color32 below = target[i];
                target[i] = new Color32(
                    (byte)Mathf.RoundToInt(Mathf.Lerp(below.r, layer[i].r, alpha)),
                    (byte)Mathf.RoundToInt(Mathf.Lerp(below.g, layer[i].g, alpha)),
                    (byte)Mathf.RoundToInt(Mathf.Lerp(below.b, layer[i].b, alpha)),
                    (byte)Mathf.Max(below.a, layer[i].a));
            }
        }

        private sealed class PanelState
        {
            private readonly bool _clearColor;
            private readonly Color _clearValue;
            private readonly PanelSettings _settings;
            private readonly RenderTexture _previous;
            private readonly RenderTexture _texture;

            public PanelState(PanelSettings settings, int width, int height)
            {
                _settings = settings;
                _previous = settings.targetTexture;
                _clearColor = settings.clearColor;
                _clearValue = settings.colorClearValue;
                _texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                _texture.Create();
                settings.targetTexture = _texture;
                settings.clearColor = true;
                settings.colorClearValue = Color.clear;
            }

            public Color32[] Read()
            {
                RenderTexture active = RenderTexture.active;
                RenderTexture.active = _texture;
                Texture2D readback = new Texture2D(_texture.width, _texture.height, TextureFormat.RGBA32, false);
                readback.ReadPixels(new Rect(0, 0, _texture.width, _texture.height), 0, 0);
                readback.Apply();
                RenderTexture.active = active;
                Color32[] pixels = readback.GetPixels32();
                Object.Destroy(readback);
                return pixels;
            }

            public void Restore()
            {
                _settings.targetTexture = _previous;
                _settings.clearColor = _clearColor;
                _settings.colorClearValue = _clearValue;
                _texture.Release();
                Object.Destroy(_texture);
            }
        }
    }
}
