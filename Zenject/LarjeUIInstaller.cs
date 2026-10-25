using UnityEngine;
using Zenject;

namespace Larje.UI.Zenject
{
    [CreateAssetMenu(fileName = "LarjeUIInstaller", menuName = "Larje/UI/Zenject/UI Installer")]
    public sealed class LarjeUIInstaller : ScriptableObjectInstaller<LarjeUIInstaller>
    {
        [SerializeField] private UIConfig ui;

        public override void InstallBindings()
        {
            if (ui != null)
            {
                Container.BindLarjeUI(ui);
            }
        }
    }
}
