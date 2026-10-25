using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Larje.UI.Testing
{
    public sealed class UIMockupHarness : IDisposable
    {
        private readonly int _width;
        private readonly int _height;
        private readonly GameObject _host;
        private readonly UIDocument _document;
        private readonly PanelSettings _settings;
        private readonly RenderTexture _texture;

        public VisualElement Root => _document.rootVisualElement;

        public UIMockupHarness(int width, int height)
        {
            _width = width;
            _height = height;
            _texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            _texture.Create();
            _settings = ScriptableObject.CreateInstance<PanelSettings>();
            _settings.targetTexture = _texture;
            _settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            _settings.scale = 1f;
            _settings.clearColor = true;
            _settings.colorClearValue = Color.black;
            _settings.themeStyleSheet = FindTheme();
            _host = new GameObject("[UIMockupHarness]");
            _host.SetActive(false);
            _document = _host.AddComponent<UIDocument>();
            _document.panelSettings = _settings;
            _host.SetActive(true);
        }

        public VisualElement Load(string uxmlPath, IEnumerable<string> states)
        {
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (asset == null)
            {
                throw new FileNotFoundException("UXML not found: " + uxmlPath);
            }

            Root.Clear();
            VisualElement view = asset.Instantiate();
            UIKit.Prepare(view);
            Root.Add(view);
            new UIStates(view).SetOnly(states ?? Array.Empty<string>());
            return view;
        }

        public IEnumerator WaitForLayout(int frames = 3)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
        }

        public List<string> Compare(VisualElement view, MockupLayout layout, float tolerance)
        {
            return Compare(view, layout, tolerance, null);
        }

        public List<string> Compare(VisualElement view, MockupLayout layout, float tolerance, List<string> details)
        {
            List<string> problems = new List<string>();
            foreach (MockupNode node in layout.nodes ?? Array.Empty<MockupNode>())
            {
                VisualElement element = UIQuery.Path<VisualElement>(view, node.path);
                if (element == null)
                {
                    problems.Add($"{node.path}: missing in UXML");
                }
                else if (!IsVisible(element))
                {
                    problems.Add($"{node.path}: hidden in Unity but visible in the mockup");
                }
                else
                {
                    CompareRect(node, element.worldBound, tolerance, problems);
                    AddDetail(node, element.worldBound, details);
                }
            }

            return problems;
        }

        public Texture2D Read()
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = _texture;
            Texture2D texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            return texture;
        }

        public void Capture(string path, string mockupPng = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Texture2D unity = Read();
            File.WriteAllBytes(path, unity.EncodeToPNG());
            if (mockupPng != null && File.Exists(mockupPng))
            {
                Texture2D mockup = new Texture2D(2, 2);
                mockup.LoadImage(File.ReadAllBytes(mockupPng));
                Texture2D pair = new Texture2D(_width * 2, _height, TextureFormat.RGBA32, false);
                pair.SetPixels(0, 0, Mathf.Min(_width, mockup.width), Mathf.Min(_height, mockup.height), mockup.GetPixels(0, 0, Mathf.Min(_width, mockup.width), Mathf.Min(_height, mockup.height)));
                pair.SetPixels(_width, 0, _width, _height, unity.GetPixels());
                pair.Apply();
                File.WriteAllBytes(Path.ChangeExtension(path, null) + "--compare.png", pair.EncodeToPNG());
                Object.DestroyImmediate(mockup);
                Object.DestroyImmediate(pair);
            }

            Object.DestroyImmediate(unity);
        }

        public void Dispose()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_settings);
            _texture.Release();
            Object.DestroyImmediate(_texture);
        }

        private void CompareRect(MockupNode node, Rect actual, float tolerance, List<string> problems)
        {
            Rect expected = new Rect(node.rect.x, node.rect.y, node.rect.w, node.rect.h);
            bool text = node.type == "Text";
            float sizeTolerance = text ? tolerance * 2f : tolerance;
            bool positionOk = text
                ? Vector2.Distance(expected.center, actual.center) <= tolerance * 1.5f
                : Mathf.Abs(expected.x - actual.x) <= tolerance && Mathf.Abs(expected.y - actual.y) <= tolerance;
            bool sizeOk = Mathf.Abs(expected.width - actual.width) <= sizeTolerance && Mathf.Abs(expected.height - actual.height) <= sizeTolerance;
            if (!positionOk || !sizeOk)
            {
                problems.Add($"{node.path}: expected {Format(expected)}, got {Format(actual)}");
            }
        }

        private void AddDetail(MockupNode node, Rect actual, List<string> details)
        {
            if (details == null)
            {
                return;
            }

            float dx = actual.x - node.rect.x;
            float dy = actual.y - node.rect.y;
            float dw = actual.width - node.rect.w;
            float dh = actual.height - node.rect.h;
            if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy), Mathf.Abs(dw), Mathf.Abs(dh)) > 2f)
            {
                details.Add($"{node.path} [{node.type}]: dx {dx:F0}, dy {dy:F0}, dw {dw:F0}, dh {dh:F0}");
            }
        }

        private string Format(Rect rect)
        {
            return $"({rect.x:F0}, {rect.y:F0}, {rect.width:F0}x{rect.height:F0})";
        }

        private bool IsVisible(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current.resolvedStyle.display == DisplayStyle.None || current.resolvedStyle.visibility == Visibility.Hidden)
                {
                    return false;
                }
            }

            return true;
        }

        private ThemeStyleSheet FindTheme()
        {
            string guid = AssetDatabase.FindAssets("t:ThemeStyleSheet").FirstOrDefault();
            return guid != null ? AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(AssetDatabase.GUIDToAssetPath(guid)) : null;
        }
    }
}
