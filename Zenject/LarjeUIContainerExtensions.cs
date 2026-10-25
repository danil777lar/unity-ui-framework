using Larje.UI.Debugging;
using Zenject;

namespace Larje.UI.Zenject
{
    public static class LarjeUIContainerExtensions
    {
        public static void BindLarjeUI(this DiContainer container, UIConfig config)
        {
            container.BindInstance(config);
            container.BindInterfacesAndSelfTo<UIService>().AsSingle();
            container.BindInterfacesTo<UIDebugCommands>().AsSingle();
        }
    }
}
