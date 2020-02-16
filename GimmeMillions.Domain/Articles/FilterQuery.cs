using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Articles
{
    public class FilterQuery
    {
        public string Field { get; set; }
        public List<string> Values { get; set; }
        public string ToQueryString()
        {
            return $"{Field}:({string.Join(",", Values.Select(x => $"\"{x}\""))})";
        }
    }
}
