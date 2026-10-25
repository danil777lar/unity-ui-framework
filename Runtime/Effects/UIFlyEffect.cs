using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public static class UIFlyEffect
    {
        public static async UniTask Fly(VisualElement layer, VisualElement from, VisualElement to, int count, IEnumerable<string> itemClasses,
            Action<int> onArrive = null, float size = 84f, CancellationToken token = default)
        {
            if (layer == null || from == null || to == null || count <= 0)
            {
                return;
            }

            Vector2 start = layer.WorldToLocal(from.worldBound.center);
            Vector2 end = layer.WorldToLocal(to.worldBound.center);
            List<UniTask> flights = new List<UniTask>();
            for (int i = 0; i < count; i++)
            {
                VisualElement item = new VisualElement();
                item.pickingMode = PickingMode.Ignore;
                foreach (string className in itemClasses)
                {
                    item.AddToClassList(className);
                }

                item.style.position = Position.Absolute;
                item.style.width = size;
                item.style.height = size;
                item.style.left = start.x - size * 0.5f;
                item.style.top = start.y - size * 0.5f;
                layer.Add(item);
                Vector2 burst = start + UnityEngine.Random.insideUnitCircle * size * 1.6f;
                flights.Add(FlyOne(item, start, burst, end, size, i, count, onArrive, to, token));
            }

            await UniTask.WhenAll(flights);
        }

        private static async UniTask FlyOne(VisualElement item, Vector2 start, Vector2 burst, Vector2 end, float size, int index, int count,
            Action<int> onArrive, VisualElement target, CancellationToken token)
        {
            TweenSettings burstSettings = new TweenSettings(0.25f, Ease.OutQuad, 0f, true);
            await Tween.Vector2(start, burst, burstSettings, x => Place(item, x, size, 1f), token).SuppressCancellationThrow();
            Vector2 control = (burst + end) * 0.5f + Vector2.up * -200f;
            TweenSettings flySettings = new TweenSettings(0.55f, Ease.InQuad, index * 0.05f, true);
            bool cancelled = await Tween.Float(0f, 1f, flySettings, t =>
            {
                Vector2 a = Vector2.Lerp(burst, control, t);
                Vector2 b = Vector2.Lerp(control, end, t);
                Place(item, Vector2.Lerp(a, b, t), size, Mathf.Lerp(1f, 0.6f, t));
            }, token).SuppressCancellationThrow();
            item.RemoveFromHierarchy();
            if (!cancelled)
            {
                onArrive?.Invoke(index);
                Punch(target, token).Forget();
            }
        }

        private static void Place(VisualElement item, Vector2 center, float size, float scale)
        {
            item.style.left = center.x - size * 0.5f;
            item.style.top = center.y - size * 0.5f;
            item.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
        }

        private static async UniTaskVoid Punch(VisualElement target, CancellationToken token)
        {
            TweenSettings settings = new TweenSettings(0.18f, Ease.OutBack, 0f, true);
            await Tween.Float(1.15f, 1f, settings, x => target.style.scale = new StyleScale(new Scale(new Vector3(x, x, 1f))), token).SuppressCancellationThrow();
            target.style.scale = StyleKeyword.Null;
        }
    }
}
