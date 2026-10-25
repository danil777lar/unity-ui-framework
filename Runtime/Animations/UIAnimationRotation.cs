using Larje.Core.Tweening;
using UnityEngine;

namespace Larje.UI.Animations
{
    public sealed class UIAnimationRotation : UIAnimationBase
    {
        [Header("Values")]
        [SerializeField] private Vector3 offsetFrom = new Vector3(0f, 0f, 90f);
        [SerializeField] private Vector3 offsetTo = Vector3.zero;
        [SerializeField] private bool setFromOnAwake = true;

        [Header("Timing")]
        [SerializeField] private DirectionalTween timing = new DirectionalTween();

        private Vector3 _from;
        private Vector3 _to;

        protected override float OnEvent(bool forward)
        {
            TweenSettings settings = timing.Get(forward);
            Vector3 start = transform.localEulerAngles;
            Vector3 target = forward ? _to : _from;
            return Play(settings, token => Tween.Progress(settings, t => transform.localRotation = Quaternion.Euler(LerpAngles(start, target, t)), token));
        }

        private void Awake()
        {
            _from = transform.localEulerAngles + offsetFrom;
            _to = transform.localEulerAngles + offsetTo;
            if (setFromOnAwake)
            {
                transform.localRotation = Quaternion.Euler(_from);
            }
        }

        private Vector3 LerpAngles(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(Mathf.LerpAngle(a.x, b.x, t), Mathf.LerpAngle(a.y, b.y, t), Mathf.LerpAngle(a.z, b.z, t));
        }
    }
}
