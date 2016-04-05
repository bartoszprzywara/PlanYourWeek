using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class PlannedActivity
    {
        public PlannedActivity(int i)
        {
            Id = i;
            StartHour = new TimeSpan(i, 00, 00);
            ProposedActivity = new Activity(true);
        }

        public PlannedActivity(int i, Activity act)
        {
            Id = i;
            StartHour = new TimeSpan(i, 00, 00);
            ProposedActivity = act;
        }

        public int Id { get; set; }
        public TimeSpan StartHour { get; set; }
        public string DateColor { get; set; } = "Black";
        public string HourColor { get; set; } = "Black";
        public int ItemHeight { get; set; } = 60;
        public Activity ProposedActivity { get; set; }

        public string StartHourUI { get { return StartHour.ToString(@"hh\:mm"); } }
    }
}
