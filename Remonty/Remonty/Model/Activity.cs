using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Model
{
    class Activity
    {
        public Activity()
        {
            Title = "Tytuł zadania 1";
            Description = "Opis zadania 1";
            Priority = 0;
            IsAllDay = false;
            Start = new DateTimeOffset(new DateTime(2016,03,29));
            End = new DateTimeOffset(new DateTime(2016,03,30)); ;
            Estimation = 3;
            Context = 1;
            Project = 2;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public int? Priority { get; set; }
        public bool IsAllDay { get; set; }
        public DateTimeOffset? Start { get; set; }
        public DateTimeOffset? End { get; set; }
        public int? Estimation { get; set; }
        public int? Context { get; set; }
        public int? Project { get; set; }
    }
}
