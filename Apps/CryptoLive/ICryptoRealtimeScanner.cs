using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoLive
{
    public interface ICryptoRealtimeScanner
    {
        ICryptoEventNotifier Notifier { get; }
        IEnumerable<CryptoEventNotification> Scan();
    }
}
