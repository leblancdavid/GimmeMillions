using GimmeMillions.Domain.Stocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoLive.Notification
{
    public interface ICryptoEventNotifier
    {
        void Notify(CryptoEventNotification notification);
    }
}
