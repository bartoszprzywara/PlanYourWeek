using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Helpers
{
    class InitializeLocalDatabase
    {
        public static void InitializeLocalDB()
        {
            LocalDatabaseHelper initDB = new LocalDatabaseHelper();
            initDB.CreateDatabase();

#if DEBUG
            initDB.DeleteAllContact();

            initDB.Insert(new Activity("Kupić farbę", "Biała 10l, Zielona 5l", "Wysoki", true, null, null, new DateTime(2016, 04, 21), "2 godz", "Zakupy", null));
            initDB.Insert(new Activity("Tytuł zadania 1", "Opis zadania 1", "Niski", false, new TimeSpan(17, 34, 56), new DateTime(2016, 03, 29), new DateTime(2016, 03, 30), "1 godz", "Spotkanie", "Położyć panele"));
            initDB.Insert(new Activity("Pomalować kuchnię", "Na zielono", "Normalny", false, new TimeSpan(16, 00, 00), new DateTime(2016, 04, 23), new DateTime(2016, 04, 23), "4 godz", "Kuchnia", "Pomalować mieszkanie"));
            //initDB.Insert(new Activity(null, null, null, null, null, null, null, null, null, null));
#endif
        }
    }
}
