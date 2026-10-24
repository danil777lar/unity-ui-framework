using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core.Economy;
using Larje.Core.Tweening;
using TMPro;
using UnityEngine;

namespace Larje.Core.UI
{
    public sealed class CurrencyFlyAnimation : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private CurrencyCounter counter;
        [SerializeField] private Transform target;
        [SerializeField] private GameObject prefab;

        [Header("Text")]
        [SerializeField] private string prefix;
        [SerializeField] private string suffix;

        [Header("Spawn")]
        [SerializeField] private float spawnSecondsPerPixel = 0.005f;
        [SerializeField] private Ease spawnEase = Ease.OutBack;
        [SerializeField] private Vector2 spawnRadius = new Vector2(100f, 200f);
        [SerializeField] private float spawnScale = 1.5f;

        [Header("Move")]
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private Ease moveEase = Ease.InQuad;
        [SerializeField] private float midScale = 0.5f;
        [SerializeField] private float endScale = 1f;

        private ICurrencyService _currency;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ICurrencyService currency)
        {
            _currency = currency;
            _currency.Added += OnAdded;
        }

        private void Awake()
        {
            prefab.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_currency != null)
            {
                _currency.Added -= OnAdded;
            }
        }

        private void OnAdded(CurrencyOperation operation)
        {
            if (operation.Currency == counter.Currency && operation.UsePosition && Camera.main != null)
            {
                int finalAmount = counter.GetCurrentAmount();
                counter.SetDisplayed(finalAmount - operation.Amount);
                FlyAsync(operation, finalAmount, this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow().Forget();
            }
        }

        private async UniTask FlyAsync(CurrencyOperation operation, int finalAmount, CancellationToken cancellationToken)
        {
            Vector3 start = Camera.main.WorldToScreenPoint(operation.WorldPosition);
            GameObject instance = Instantiate(prefab, transform);
            instance.SetActive(true);
            instance.transform.position = start;
            instance.transform.localScale = Vector3.zero;

            TMP_Text text = instance.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = prefix + operation.Amount + suffix;
            }

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(spawnRadius.x, spawnRadius.y);
            Vector3 mid = start + offset;
            TweenSettings spawn = new TweenSettings(offset.magnitude * spawnSecondsPerPixel, spawnEase, 0f, true);
            TweenSettings move = new TweenSettings(moveDuration, moveEase, 0f, true);

            try
            {
                await Tween.Progress(spawn, t =>
                {
                    instance.transform.position = Vector3.LerpUnclamped(start, mid, t);
                    instance.transform.localScale = Vector3.one * Mathf.LerpUnclamped(0f, spawnScale, t);
                }, cancellationToken);

                await Tween.Progress(move, t =>
                {
                    instance.transform.position = Bezier(mid, start, target.position, t);
                    instance.transform.localScale = Vector3.one * Bezier(spawnScale, midScale, endScale, t);
                }, cancellationToken);

                counter.SetDisplayed(finalAmount);
                counter.PlayPunch();
            }
            finally
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
        }

        private Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
        }

        private float Bezier(float a, float b, float c, float t)
        {
            return Mathf.Lerp(Mathf.Lerp(a, b, t), Mathf.Lerp(b, c, t), t);
        }
    }
}
