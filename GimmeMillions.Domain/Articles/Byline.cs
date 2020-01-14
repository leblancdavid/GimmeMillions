using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Articles
{
    public class Byline
    {
        public string Original { get; set; }
        public string Organization { get; set; }
        public List<Person> Person { get; set; }
    }
}
