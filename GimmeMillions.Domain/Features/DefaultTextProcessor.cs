﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class DefaultTextProcessor : ITextProcessor
    {
        protected IFeatureChecker _featureChecker;
        public DefaultTextProcessor(IFeatureChecker featureChecker)
        {
            _featureChecker = featureChecker;
        }
        public IEnumerable<string> Process(string f)
        {
            if(string.IsNullOrEmpty(f))
            {
                return new List<string>();
            }

            var onlyLetters = Regex.Replace(f.ToLower(), @"[^a-z]+", " ");
            var words = onlyLetters.Split(' ');

            return words.Where(x => _featureChecker.IsValid(x));
        }
    }
}
