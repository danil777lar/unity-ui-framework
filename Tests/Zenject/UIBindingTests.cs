using System.IO;
using System.Reflection;
using Larje.Core.Economy;
using Larje.Core.Saves;
using Larje.Core.Zenject;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Larje.UI.Zenject.Tests
{
    public sealed class UIBindingTests
    {
        private string _root;
        private DiContainer _container;
        private UIConfig _config;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "larje_ui_tests_" + System.Guid.NewGuid().ToString("N"));
            _config = ScriptableObject.CreateInstance<UIConfig>();
            _container = new DiContainer();
            _container.BindLarjeInfrastructure();
            _container.BindLarjeSaves(ScriptableObject.CreateInstance<SaveConfig>(), new FileSaveStorage(_root));
            _container.BindLarjeEconomy(ScriptableObject.CreateInstance<CurrencyConfig>(), null);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }

            Object.DestroyImmediate(_config);
        }

        [Test]
        public void UIServiceAndDebugCommandsAreBound()
        {
            _container.BindLarjeUI(_config);

            Assert.IsTrue(_container.HasBinding<IUIService>());
            Assert.AreSame(_config, _container.Resolve<UIConfig>());
        }

        [Test]
        public void ComponentsReceiveDependenciesThroughConstruct()
        {
            _container.Resolve<LarjeInitializer>().Initialize();
            GameObject target = new GameObject("Counter", typeof(RectTransform));
            target.AddComponent<TMPro.TextMeshProUGUI>();
            CurrencyCounter counter = target.AddComponent<CurrencyCounter>();
            try
            {
                _container.InjectGameObject(target);
                _container.Resolve<ICurrencyService>().Set(counter.Currency, 0);
                Assert.AreEqual(0, counter.GetCurrentAmount());
                Assert.IsTrue(InjectedField(counter));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        private bool InjectedField(CurrencyCounter counter)
        {
            FieldInfo field = typeof(CurrencyCounter).GetField("_currency", BindingFlags.Instance | BindingFlags.NonPublic);
            return field.GetValue(counter) != null;
        }
    }
}
