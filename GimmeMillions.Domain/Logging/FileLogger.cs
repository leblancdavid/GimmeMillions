using System;
using System.IO;

namespace GimmeMillions.Domain.Logging
{
    public class FileLogger : ILogger
    {
        private string _fileName;
        public FileLogger(string fileName)
        {
            _fileName = fileName;
        }

        public void Log(string message)
        {
            var logFile = $"{_fileName}-{DateTime.Today.ToString("yyyy-MM-dd")}";
            using (StreamWriter streamWriter = new StreamWriter(logFile, true))
            {
                streamWriter.WriteLine(message);
                streamWriter.Close();
            }
        }
    }
}
