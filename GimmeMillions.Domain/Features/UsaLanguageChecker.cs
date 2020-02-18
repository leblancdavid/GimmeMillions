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
        public HashSet<string> LanguageSet { get; set; }

        public UsaLanguageChecker()
        {
            LanguageSet = new HashSet<string>();
        }

        public void Load(StreamReader reader)
        {
            LanguageSet = new HashSet<string>();
            string word;
            while ((word = reader.ReadLine()) != null)
            {
                LanguageSet.Add(word);
            }
        }

        public void Add(string f)
        {
            LanguageSet.Add(f);
        }

        public void Remove(string f)
        {
            LanguageSet.Remove(f);
        }

        public bool IsValid(string feature)
        {
            if (LanguageSet.Contains(feature))
                return true;
            return false;
        }

    }
}
