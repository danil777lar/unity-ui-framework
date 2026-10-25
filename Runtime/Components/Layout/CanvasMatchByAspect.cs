using UnityEngine;
using UnityEngine.UI;

namespace Larje.UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class CanvasMatchByAspect : MonoBehaviour
    {
        [SerializeField] private AnimationCurve matchByAspect = AnimationCurve.Linear(0.5f, 1f, 2f, 0f);

        private float _aspect;
        private CanvasScaler _scaler;

        private void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
        }

        private void Update()
        {
            float aspect = (float)Screen.width / Screen.height;
            if (!Mathf.Approximately(aspect, _aspect))
            {
                _aspect = aspect;
                _scaler.matchWidthOrHeight = matchByAspect.Evaluate(aspect);
            }
        }
    }
}
