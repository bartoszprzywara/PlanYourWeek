using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class PlannedWeekItems
    {
        public ObservableCollection<PlannedActivity>[] FinalPlannedWeek { get; set; }
        public int[] TotalHours { get; set; }
        public int[] UsedHours { get; set; }
        public int[] TotalWorkingHours { get; set; }
        public int[] UsedWorkingHours { get; set; }
    }
}
