using CryptoLive.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptoLive.Accounts
{
    public class SimulationCryptoAccountManager : ICryptoAccountManager
    {
        private SimulatedCryptoAccount _account;
        public ICryptoAccount Account => _account;

        private string _accountFile;
        private decimal _maxPositionRatio;
        private decimal _fees = 0.01m;
        public SimulationCryptoAccountManager(decimal maxPositionRatio = 0.20m, decimal fees = 0.01m)
        {
            _account = new SimulatedCryptoAccount();
            _accountFile = "SimulatedAccount.json";
            _maxPositionRatio = maxPositionRatio;
            _fees = fees;
        }

        public void SaveAccount(string accountFile)
        {
            _accountFile = accountFile;
            File.WriteAllText(_accountFile, JsonConvert.SerializeObject(_account, Formatting.Indented));
        }

        public void LoadAccount(string accountFile)
        {
            _accountFile = accountFile;
            _account = JsonConvert.DeserializeObject<SimulatedCryptoAccount>(File.ReadAllText(_accountFile));
        }

        public void Notify(CryptoEventNotification notification)
        {
            if(notification.IsBuySignal())
            {
                var amount = DetermineShareBuyCount(notification);
                if(amount > 0.0m)
                {
                    _account.Buy(notification.CryptoSymbol, notification.LastBar.Close, amount);
                    _account.AvailableFunds -= _fees * (notification.LastBar.Close * amount);
                }
            }
            else if(notification.IsSellSignal())
            {
                //Always sell the whole package
                _account.Sell(notification.CryptoSymbol, notification.LastBar.Close, decimal.MaxValue);
            }

            //Save progress
            SaveAccount(_accountFile);
        }

        private decimal DetermineShareBuyCount(CryptoEventNotification notification)
        {
            var maxAmount = _maxPositionRatio * _account.CurrentValue;
            decimal buyAmount;
            var existingPosition = _account.CurrentPositions.FirstOrDefault(x => x.Symbol == notification.CryptoSymbol);
            if (existingPosition != null)
            {
                if(existingPosition.PositionValue < maxAmount)
                {
                    buyAmount = maxAmount - existingPosition.PositionValue;
                    if(buyAmount > _account.AvailableFunds)
                    {
                        buyAmount = _account.AvailableFunds;
                    }

                    return buyAmount * (1.0m - _fees) / notification.LastBar.Close;
                }
                return 0.0m;
            }

            buyAmount = maxAmount;
            if (buyAmount > _account.AvailableFunds)
            {
                buyAmount = _account.AvailableFunds;
            }

            return buyAmount * (1.0m - _fees) / notification.LastBar.Close;
        }

    }
}
