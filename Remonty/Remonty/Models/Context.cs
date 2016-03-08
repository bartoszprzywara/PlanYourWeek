using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty
{
    public class Context
    {
        public string ContextName;
    }

    public class ContextManager
    {
        public static List<string> getContexts()
        {
            var contexts = new List<string>();
            contexts.Add("Zakupy");
            contexts.Add("Spotkanie");
            contexts.Add("Telefon");
            contexts.Add("Komputer");
            contexts.Add("Kuchnia");
            contexts.Add("Łazienka");
            contexts.Add("Przedpokój");
            contexts.Add("Salon");
            contexts.Add("Sypialnia");

            return contexts;
        }
    }
}
