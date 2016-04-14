using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class Priority : ComplexProperty, IComplexProperty
    {
        public Priority()
        {

        }

        public Priority(string name)
        {
            Name = name;
        }
    }
}
