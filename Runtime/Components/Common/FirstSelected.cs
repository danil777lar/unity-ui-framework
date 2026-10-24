using UnityEngine;
using UnityEngine.EventSystems;

namespace Larje.Core.UI
{
    public sealed class FirstSelected : MonoBehaviour
    {
        private void OnEnable()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}
