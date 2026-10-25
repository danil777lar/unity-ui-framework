using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Larje.Core.Economy;
using Larje.Core.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI
{
    public sealed class UICurrencyLabel : IDisposable
    {
        private int _shown;
        private readonly Label _label;
        private readonly CurrencyId _currency;
        private readonly ICurrencyService _service;
        private readonly TweenScope _scope;
        private readonly Func<int, string> _format;

        public UICurrencyLabel(Label label, CurrencyId currency, ICurrencyService service, Func<int, string> format = null, CancellationToken owner = default)
        {
            _label = label;
            _currency = currency;
            _service = service;
            _format = format ?? Format;
            _scope = new TweenScope(owner);
            _shown = service.Get(currency);
            _label.text = _format(_shown);
            _service.Changed += OnChanged;
        }

        public void Dispose()
        {
            _service.Changed -= OnChanged;
            _scope.Dispose();
        }

        private void OnChanged()
        {
            int target = _service.Get(_currency);
            if (target == _shown)
            {
                return;
            }

            TweenSettings settings = new TweenSettings(0.4f, Ease.OutQuad, 0f, true);
            int from = _shown;
            Tween.Float(from, target, settings, x => Show(Mathf.RoundToInt(x)), _scope.Restart()).SuppressCancellationThrow().Forget();
        }

        private void Show(int amount)
        {
            _shown = amount;
            _label.text = _format(amount);
        }

        private string Format(int amount)
        {
            return amount.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", " ");
        }
    }
}
