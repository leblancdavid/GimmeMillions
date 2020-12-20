using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Stocks
{
    public class StockSymbolsFile : IStockSymbolsRepository
    {
        private string _symbolsFile;
        public StockSymbolsFile(string file)
        {
            _symbolsFile = file;
        }

        public IEnumerable<string> GetStockSymbols()
        {
            var symbols = new List<string>();
            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(_symbolsFile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var ticker = line.Split(',');
                        if (!ticker[0].Any(x => !char.IsLetter(x) || !char.IsUpper(x)))
                            symbols.Add(ticker[0]);
                    }
                }
            }
            catch(Exception)
            {
            }

            return symbols;
        }
    }
}
