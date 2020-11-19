using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public interface ITextProcessor
    {
        IEnumerable<string> Process(string f);
    }
}
