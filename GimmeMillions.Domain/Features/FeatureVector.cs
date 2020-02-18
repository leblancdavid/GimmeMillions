using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.Domain.Features
{
    public class FeatureVector
    {
        public double[] Data { get; set; }
        public DateTime Date { get; set; }
        public int Length
        {
            get { return Data.Length; }
        }

        public double this[int i]
        {
            get { return Data[i]; }
            set { Data[i] = value; }
        }

        public FeatureVector(int length)
        {
            Data = new double[length];
            Date = DateTime.Today;
        }

        public FeatureVector(int length, DateTime date)
        {
            Data = new double[length];
            Date = date;
        }

    }
}
