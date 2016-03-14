using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class Priority : IHasName
    {
        public Priority()
        {

        }

        public Priority(string name)
        {
            Name = name;
        }

        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
