using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public sealed class UIStates
    {
        public const string SHOW_PREFIX = "state-show--";
        public const string HIDE_PREFIX = "state-hide--";
        public const string CLASS_PREFIX = "state-class--";

        private readonly VisualElement _root;
        private readonly HashSet<string> _active = new HashSet<string>();
        private readonly List<Rule> _rules = new List<Rule>();

        public IReadOnlyCollection<string> Active => _active;

        public UIStates(VisualElement root)
        {
            _root = root;
            if (root != null)
            {
                root.Query<VisualElement>().ForEach(Collect);
            }

            Apply();
        }

        public bool Is(string state)
        {
            return _active.Contains(state);
        }

        public void Set(string state, bool active)
        {
            bool changed = active ? _active.Add(state) : _active.Remove(state);
            if (changed)
            {
                Apply();
            }
        }

        public void SetOnly(IEnumerable<string> states)
        {
            _active.Clear();
            foreach (string state in states)
            {
                if (!string.IsNullOrEmpty(state) && state != "default")
                {
                    _active.Add(state);
                }
            }

            Apply();
        }

        private void Collect(VisualElement element)
        {
            Rule rule = null;
            foreach (string className in element.GetClasses())
            {
                if (className.StartsWith(SHOW_PREFIX) || className.StartsWith(HIDE_PREFIX) || className.StartsWith(CLASS_PREFIX))
                {
                    rule ??= new Rule(element);
                    rule.Add(className);
                }
            }

            if (rule != null)
            {
                _rules.Add(rule);
            }
        }

        private void Apply()
        {
            foreach (Rule rule in _rules)
            {
                rule.Apply(_active);
            }

            if (_root != null && _rules.Count > 0)
            {
                UIGaps.Apply(_root);
            }
        }

        private sealed class Rule
        {
            private readonly VisualElement _element;
            private readonly List<string> _show = new List<string>();
            private readonly List<string> _hide = new List<string>();
            private readonly List<KeyValuePair<string, string>> _classes = new List<KeyValuePair<string, string>>();

            public Rule(VisualElement element)
            {
                _element = element;
            }

            public void Add(string className)
            {
                if (className.StartsWith(SHOW_PREFIX))
                {
                    _show.Add(className.Substring(SHOW_PREFIX.Length));
                }
                else if (className.StartsWith(HIDE_PREFIX))
                {
                    _hide.Add(className.Substring(HIDE_PREFIX.Length));
                }
                else
                {
                    string[] parts = className.Substring(CLASS_PREFIX.Length).Split(new[] { "--" }, 2, System.StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        _classes.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
                    }
                }
            }

            public void Apply(HashSet<string> active)
            {
                bool visible = _show.Count == 0 || _show.Exists(active.Contains);
                if (_hide.Exists(active.Contains))
                {
                    visible = false;
                }

                if (_show.Count > 0 || _hide.Count > 0)
                {
                    _element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                }

                foreach (KeyValuePair<string, string> pair in _classes)
                {
                    _element.EnableInClassList(pair.Value, active.Contains(pair.Key));
                }
            }
        }
    }
}
