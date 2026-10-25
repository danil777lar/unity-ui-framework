using Larje.Core.Zenject;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Larje.UI.Zenject.Tests
{
    public sealed class UIBindingTests
    {
        private DiContainer _container;
        private UIConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<UIConfig>();
            _container = new DiContainer();
            _container.BindLarjeInfrastructure();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void UIServiceAndDebugCommandsAreBound()
        {
            _container.BindLarjeUI(_config);

            Assert.IsTrue(_container.HasBinding<IUIService>());
            Assert.IsTrue(_container.HasBinding<IUIControllerFactory>());
            Assert.AreSame(_config, _container.Resolve<UIConfig>());
        }

        [Test]
        public void FactoryCreatesBoundControllers()
        {
            _container.BindLarjeUI(_config);
            _container.BindUIController<TestScreen>("Test");
            IUIControllerFactory factory = _container.Resolve<IUIControllerFactory>();

            Assert.IsInstanceOf<TestScreen>(factory.Create("Test"));
            Assert.IsNull(factory.Create("Missing"));
        }

        public sealed class TestScreen : UIScreen
        {
        }
    }
}
