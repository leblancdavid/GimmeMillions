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
        public string Encoding { get; set; }
        public int Length
        {
            get { return Data.Length; }
        }

        public double this[int i]
        {
            get { return Data[i]; }
            set { Data[i] = value; }
        }

        public FeatureVector(int length, string encoding)
        {
            Data = new double[length];
            Date = DateTime.Today;
            Encoding = encoding;
        }

        public FeatureVector(int length, DateTime date, string encoding)
        {
            Data = new double[length];
            Date = date;
            Encoding = encoding;
        }

        public FeatureVector(double[] data, DateTime date, string encoding)
        {
            Data = data;
            Date = date;
            Encoding = encoding;
        }

        public FeatureVector()
        {
        }

        public FeatureVector GetMinMaxNormalized()
        {
            var normalizedVector = new FeatureVector(Data.Length, Date, Encoding);

            double min = Data.Min();
            double max = Data.Max();
            for(int i = 0; i < Data.Length; ++i)
            {
                normalizedVector.Data[i] = (Data[i] - min) / (max - min);
            }

            return normalizedVector;
        }

    }
}
