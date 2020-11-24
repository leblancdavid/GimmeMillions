using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoLive.Notification
{
    public class MultiCryptoEventNotifier : ICryptoEventNotifier
    {
        private IEnumerable<ICryptoEventNotifier> _notifiers;
        public MultiCryptoEventNotifier(IEnumerable<ICryptoEventNotifier> notifiers)
        {
            _notifiers = notifiers;
        }

        public void Notify(CryptoEventNotification notification)
        {
            foreach(var notifier in _notifiers)
            {
                notifier.Notify(notification);
            }
        }
    }
}
