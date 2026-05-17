using TMPro;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Currencies
{
    public class CurrencyBalanceView : MonoBehaviour
    {
        [Inject] private ICurrencyService _currencyService;

        [SerializeField] private TextMeshProUGUI _isotopesLabel;
        [SerializeField] private TextMeshProUGUI _nucleotidesLabel;

        private void Awake()
        {
            _currencyService.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_currencyService != null)
                _currencyService.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (_currencyService == null)
                return;

            _nucleotidesLabel.text = _currencyService.BalanceOf(CurrencyId.Nucleotides).ToString();
            _isotopesLabel.text = _currencyService.BalanceOf(CurrencyId.Isotopes).ToString();
        }
    }
}