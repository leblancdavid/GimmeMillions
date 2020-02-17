using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class UsaLanguageChecker : IFeatureChecker
    {
        private HashSet<string> _dictionary = new HashSet<string>();

        public void Load(StreamReader reader)
        {
            _dictionary = new HashSet<string>();
            string word;
            while ((word = reader.ReadLine()) != null)
            {
                _dictionary.Add(word);
            }
        }

        public void Add(string f)
        {
            _dictionary.Add(f);
        }

        public void Remove(string f)
        {
            _dictionary.Remove(f);
        }

        public bool IsValid(string feature)
        {
            if (_dictionary.Contains(feature))
                return true;
            return false;
        }

    }
}
