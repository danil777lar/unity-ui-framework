using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core.Economy;
using Larje.Core.Tweening;
using TMPro;
using UnityEngine;

namespace Larje.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class CurrencyCounter : MonoBehaviour
    {
        [Header("Currency")]
        [SerializeField] private CurrencyId currency;
        [SerializeField] private List<CurrencyPlacementId> placements = new List<CurrencyPlacementId>();

        [Header("Updates")]
        [SerializeField] private bool updateOnAdd = true;
        [SerializeField] private bool updateOnSpend = true;

        [Header("Format")]
        [SerializeField] private bool shortFormat;
        [SerializeField] private string prefix;
        [SerializeField] private string suffix = "<sprite index=0>";

        private bool _subscribed;
        private TMP_Text _text;
        private TweenScope _punch;
        private ICurrencyService _currency;

        public CurrencyId Currency => currency;

        private TMP_Text Text
        {
            get
            {
                if (_text == null)
                {
                    _text = GetComponent<TMP_Text>();
                }

                return _text;
            }
        }

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ICurrencyService currencyService)
        {
            _currency = currencyService;
            Subscribe();
        }

        public int GetCurrentAmount()
        {
            if (_currency == null)
            {
                return 0;
            }

            if (placements.Count == 0)
            {
                return _currency.Get(currency);
            }

            int sum = 0;
            foreach (CurrencyPlacementId placement in placements)
            {
                sum += _currency.Get(currency, placement);
            }

            return sum;
        }

        public void SetDisplayed(int amount)
        {
            string formatted = shortFormat ? CurrencyFormatter.Format(amount) : amount.ToString();
            Text.text = prefix + formatted + suffix;
        }

        public void PlayPunch()
        {
            _punch ??= new TweenScope(this.GetCancellationTokenOnDestroy());
            PunchAsync(_punch.Restart()).SuppressCancellationThrow().Forget();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _currency.Added -= OnAdded;
                _currency.Spent -= OnSpent;
                _currency.Changed -= Refresh;
                _subscribed = false;
            }
        }

        private void Subscribe()
        {
            if (_subscribed || _currency == null || !isActiveAndEnabled)
            {
                return;
            }

            _subscribed = true;
            _currency.Added += OnAdded;
            _currency.Spent += OnSpent;
            if (updateOnAdd && updateOnSpend)
            {
                _currency.Changed += Refresh;
            }

            Refresh();
        }

        private void OnAdded(CurrencyOperation operation)
        {
            if (operation.Currency == currency && updateOnAdd && !updateOnSpend)
            {
                Refresh();
            }
        }

        private void OnSpent(CurrencyOperation operation)
        {
            if (operation.Currency == currency && updateOnSpend && !updateOnAdd)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            SetDisplayed(GetCurrentAmount());
        }

        private async UniTask PunchAsync(CancellationToken cancellationToken)
        {
            await Tween.Vector3(transform.localScale, Vector3.one * 0.5f, new TweenSettings(0.1f, Ease.OutQuad, 0f, true), x => transform.localScale = x, cancellationToken);
            await Tween.Vector3(transform.localScale, Vector3.one, new TweenSettings(0.2f, Ease.OutBack, 0f, true), x => transform.localScale = x, cancellationToken);
        }
    }
}
