using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class Estimation : ComplexProperty, IComplexProperty
    {
        public Estimation()
        {

        }

        public Estimation(string name)
        {
            Name = name;
            Duration = ParseName(name);
        }

        public double Duration { get; set; }

        private double ParseName(string word)
        {
            string[] words = word.Split(' ');
            double result = 0;
            if (words[1] == "min")
                result = float.Parse(words[0]) / 60;
            if (words[1] == "godz")
                result = float.Parse(words[0]);
            return result;
        }
    }
}
