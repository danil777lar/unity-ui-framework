using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Larje.UI
{
    public class UIToast : UIObject, IPointerDownHandler
    {
        [Header("Toast")]
        [SerializeField] private UIToastId id;
        [SerializeField] private float closeDelay = 2f;
        [SerializeField] private int maxWidth = 800;

        [Header("Links")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private ContentSizeFitter sizeFitter;

        public UIToastId Id => id;
        public override string IdName => id != null ? id.name : name;

        public void OnPointerDown(PointerEventData eventData)
        {
            Close();
        }

        protected override void OnBeforeOpen(UIObjectArgs args)
        {
            UIToastArgs toastArgs = args as UIToastArgs;
            if (toastArgs != null && text != null)
            {
                text.text = toastArgs.Text;
            }

            RunAsync(toastArgs != null ? toastArgs.KeepOpen : null, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid RunAsync(Func<bool> keepOpen, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(2, PlayerLoopTiming.Update, cancellationToken);
            ClampWidth();

            if (keepOpen == null)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(closeDelay), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, cancellationToken);
            }
            else
            {
                await UniTask.WaitWhile(keepOpen, PlayerLoopTiming.Update, cancellationToken);
            }

            Close();
        }

        private void ClampWidth()
        {
            if (text == null || sizeFitter == null)
            {
                return;
            }

            RectTransform rect = (RectTransform)text.transform;
            if (rect.sizeDelta.x > maxWidth)
            {
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                rect.sizeDelta = new Vector2(maxWidth, rect.sizeDelta.y);
            }
        }
    }
}
