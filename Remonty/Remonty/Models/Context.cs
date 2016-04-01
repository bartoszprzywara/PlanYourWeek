using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class Context : IHasName
    {
        public Context()
        {

        }

        public Context(string name)
        {
            Name = name;
        }

        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ActivitiesCounter { get { return Helpers.LocalDatabaseHelper.CountItems<Activity>("SELECT * FROM Activity WHERE ContextID = " + Id); } }
    }
}
