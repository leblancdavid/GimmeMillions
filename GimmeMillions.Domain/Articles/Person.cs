using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Articles
{
    public class Person
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Qualifier { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public int Rank { get; set; }
    }
}
