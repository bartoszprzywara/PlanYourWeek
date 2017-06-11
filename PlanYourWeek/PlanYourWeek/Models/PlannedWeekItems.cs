using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanYourWeek.Models
{
    public class PlannedWeekItems
    {
        public ObservableCollection<PlannedActivity>[] PlannedWeek { get; set; }
        public double[] TotalHours { get; set; }
        public double[] UsedHours { get; set; }
        public double[] TotalWorkingHours { get; set; }
        public double[] UsedWorkingHours { get; set; }
    }
}
