using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Larje.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class FontSizeGroup : MonoBehaviour
    {
        private static readonly Dictionary<string, float> s_minSizes = new Dictionary<string, float>();

        [SerializeField] private string group;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            FitAsync(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow().Forget();
        }

        private async UniTask FitAsync(CancellationToken cancellationToken)
        {
            _text.enableAutoSizing = true;
            _text.ForceMeshUpdate();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);

            float size = _text.fontSize;
            s_minSizes[group] = s_minSizes.TryGetValue(group, out float current) ? Mathf.Min(current, size) : size;
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken);

            _text.enableAutoSizing = false;
            _text.fontSize = s_minSizes[group];
        }
    }
}
