using System.IO;

namespace CryptoLive.Notification
{
    public class LoggingCryptoEventNotifier : ICryptoEventNotifier
    {
        private string _logFile;
        public LoggingCryptoEventNotifier(string logFile)
        {
            _logFile = logFile;
        }
        public void Notify(CryptoEventNotification notification)
        {
            string notificationLog;
            if(notification.IsBuySignal())
            {
                notificationLog = $"BUY {notification.CryptoSymbol} ({notification.Signal}%): ${notification.LastBar.Close} - {notification.LastBar.Date}";
            }
            else
            {
                notificationLog = $"SELL {notification.CryptoSymbol} ({notification.Signal}%): ${notification.LastBar.Close} - {notification.LastBar.Date}";
            }
            using (StreamWriter w = File.AppendText(_logFile))
            {
                w.WriteLine(notificationLog);
            }

        }
    }
}
