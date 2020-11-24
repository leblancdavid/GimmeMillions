using CryptoLive.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoLive.Accounts
{
    public interface ICryptoAccountManager : ICryptoEventNotifier
    {
        ICryptoAccount Account { get; }
    }
}
