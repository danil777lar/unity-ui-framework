using Larje.UI.Debugging;
using Zenject;

namespace Larje.UI.Zenject
{
    public static class LarjeUIContainerExtensions
    {
        public static void BindLarjeUI(this DiContainer container, UIConfig config)
        {
            container.BindInstance(config);
            container.Bind<IUIControllerFactory>().To<ZenjectUIControllerFactory>().AsSingle().IfNotBound();
            container.BindInterfacesAndSelfTo<UIService>().AsSingle();
            container.BindInterfacesTo<UIDebugCommands>().AsSingle();
        }

        public static void BindUIController<T>(this DiContainer container, string key) where T : UIObject
        {
            container.Bind<UIControllerBinding>().FromInstance(new UIControllerBinding(key, typeof(T)));
        }
    }
}
