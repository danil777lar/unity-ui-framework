using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI.Testing
{
    public sealed class UIMockupSuite
    {
        public const string MANIFEST_PATH = "UIMockups/manifest.json";
        public const string OUTPUT_FOLDER = "Logs/UIParity";

        private bool _skipped;
        private readonly List<string> _failures = new List<string>();
        private readonly StringBuilder _report = new StringBuilder();

        public bool Skipped => _skipped;
        public IReadOnlyList<string> Failures => _failures;
        public string Report => _report.ToString();

        public IEnumerator Run(string onlyId = null)
        {
            if (!File.Exists(MANIFEST_PATH))
            {
                _skipped = true;
                _report.AppendLine($"{MANIFEST_PATH} not found: no exported mockups (design.py ui-export)");
                yield break;
            }

            MockupManifest manifest = JsonUtility.FromJson<MockupManifest>(File.ReadAllText(MANIFEST_PATH));
            int width = 1080;
            int height = 1920;
            foreach (MockupItem item in manifest.items)
            {
                if (onlyId == null || onlyId == item.id)
                {
                    foreach (string state in item.states)
                    {
                        string name = item.id + (state == "default" ? string.Empty : "--" + state);
                        string layoutPath = $"UIMockups/layout/{name}.json";
                        if (!File.Exists(layoutPath))
                        {
                            _failures.Add($"{name}: {layoutPath} not found");
                        }
                        else
                        {
                            MockupLayout layout = JsonUtility.FromJson<MockupLayout>(File.ReadAllText(layoutPath));
                            if (layout.referenceResolution != null && layout.referenceResolution.Length == 2)
                            {
                                width = layout.referenceResolution[0];
                                height = layout.referenceResolution[1];
                            }

                            yield return RunCase(item, state, name, layout, width, height, manifest.tolerance);
                        }
                    }
                }
            }

            Directory.CreateDirectory(OUTPUT_FOLDER);
            File.WriteAllText(Path.Combine(OUTPUT_FOLDER, "report.txt"), Report);
        }

        private IEnumerator RunCase(MockupItem item, string state, string name, MockupLayout layout, int width, int height, float tolerance)
        {
            UIMockupHarness harness = new UIMockupHarness(width, height);
            VisualElement view = null;
            string error = null;
            try
            {
                view = harness.Load(item.uxml, new[] { state });
            }
            catch (System.Exception exception)
            {
                error = exception.Message;
            }

            if (error == null)
            {
                yield return harness.WaitForLayout();
                List<string> details = new List<string>();
                List<string> problems = harness.Compare(view, layout, tolerance, details);
                Directory.CreateDirectory(OUTPUT_FOLDER);
                File.WriteAllLines(Path.Combine(OUTPUT_FOLDER, name + ".txt"), details);
                harness.Capture(Path.Combine(OUTPUT_FOLDER, name + ".png"), $"UIMockups/shots/{name}.png");
                _report.AppendLine($"{name}: {(problems.Count == 0 ? "OK" : problems.Count + " mismatches")}");
                foreach (string problem in problems)
                {
                    _report.AppendLine("  " + problem);
                    _failures.Add($"{name}: {problem}");
                }
            }
            else
            {
                _failures.Add($"{name}: {error}");
                _report.AppendLine($"{name}: {error}");
            }

            harness.Dispose();
        }
    }
}
