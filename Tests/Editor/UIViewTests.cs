using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Larje.UI.Tests
{
    public sealed class UIViewTests
    {
        [Test]
        public void StatesShowHideAndToggleClasses()
        {
            VisualElement root = Build(out VisualElement shown, out VisualElement hidden, out Button button);
            UIStates states = new UIStates(root);

            Assert.AreEqual(DisplayStyle.None, shown.style.display.value);
            Assert.AreEqual(DisplayStyle.Flex, hidden.style.display.value);
            Assert.IsFalse(button.ClassListContains("is-disabled"));

            states.Set("no_ads", true);

            Assert.AreEqual(DisplayStyle.Flex, shown.style.display.value);
            Assert.AreEqual(DisplayStyle.None, hidden.style.display.value);
            Assert.IsTrue(button.ClassListContains("is-disabled"));
        }

        [Test]
        public void QueryFollowsNamePaths()
        {
            VisualElement root = Build(out VisualElement shown, out VisualElement hidden, out Button button);

            Assert.AreSame(button, UIQuery.Path<Button>(root, "Screen/Bottom/PlayButton"));
            Assert.IsNull(UIQuery.Path<Button>(root, "Screen/Missing"));
        }

        [Test]
        public void ViewBindsGeneratedProperties()
        {
            VisualElement root = Build(out VisualElement shown, out VisualElement hidden, out Button button);
            TestView view = new TestView();
            view.Bind(root);

            Assert.AreSame(button, view.PlayButton);
        }

        [Test]
        public void MotionFindsPresetsByClass()
        {
            VisualElement root = Build(out VisualElement shown, out VisualElement hidden, out Button button);
            List<KeyValuePair<VisualElement, UIMotionPreset>> press = UIMotion.Find(root, UIMotionKind.Press);
            List<KeyValuePair<VisualElement, UIMotionPreset>> idle = UIMotion.Find(root, UIMotionKind.Idle);

            Assert.AreEqual(1, press.Count);
            Assert.AreSame(button, press[0].Key);
            Assert.AreEqual(1, idle.Count);
        }

        private VisualElement Build(out VisualElement shown, out VisualElement hidden, out Button button)
        {
            VisualElement root = new VisualElement();
            VisualElement screen = new VisualElement { name = "Screen" };
            VisualElement bottom = new VisualElement { name = "Bottom" };
            shown = new VisualElement { name = "NoAdsBadge" };
            shown.AddToClassList("state-show--no_ads");
            hidden = new VisualElement { name = "NoAdsButton" };
            hidden.AddToClassList("state-hide--no_ads");
            button = new Button { name = "PlayButton" };
            button.AddToClassList("anim--press");
            button.AddToClassList("anim--idle-pulse");
            button.AddToClassList("state-class--no_ads--is-disabled");
            root.Add(screen);
            screen.Add(bottom);
            screen.Add(shown);
            screen.Add(hidden);
            bottom.Add(button);
            return root;
        }

        private sealed class TestView : UIView
        {
            public Button PlayButton { get; private set; }

            protected override void OnBind()
            {
                PlayButton = Find<Button>("Screen/Bottom/PlayButton");
            }
        }
    }
}
