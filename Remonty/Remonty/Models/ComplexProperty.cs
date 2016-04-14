using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class ComplexProperty : IComplexProperty
    {
        public ComplexProperty()
        {

        }

        public ComplexProperty(string name)
        {
            Name = name;
        }

        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        [SQLite.Net.Attributes.Ignore]
        public int ActivitiesCounter { get; set; }
    }
}
