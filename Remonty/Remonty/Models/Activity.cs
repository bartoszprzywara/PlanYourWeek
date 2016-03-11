using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class Activity
    {
        public Activity()
        {

        }

        public Activity(string title, string description, string priority, bool? isAllDay,
                        DateTimeOffset? startDate, TimeSpan? startHour, DateTimeOffset? endDate, TimeSpan? endHour,
                        string estimation, string context, string project)
        {
            Title = title;
            Description = description;
            Priority = priority;
            IsAllDay = isAllDay;
            if (startDate != null)
                StartDate = startDate;
            if (startHour != null && IsAllDay == false)
                StartHour = startHour;
            if (endDate != null)
                EndDate = endDate;
            if (endHour != null && IsAllDay == false)
                EndHour = endHour;
            Estimation = estimation;
            Context = context;
            Project = project;
        }

        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public bool? IsAllDay { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public TimeSpan? StartHour { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public TimeSpan? EndHour { get; set; }
        public string Estimation { get; set; }
        public string Context { get; set; }
        public string Project { get; set; }
        //public bool? IsDone { get; set; }
        public string StartHourUI
        {
            get
            {
                if (StartHour != null)
                    return ((TimeSpan)StartHour).ToString(@"hh\:mm");
                return "";
            }
        }
        public string StartDateUI
        {
            get
            {
                if (StartDate != null)
                    return ((DateTimeOffset)StartDate).ToString("dd.MM.yyyy");
                return "";
            }
        }
    }
}
