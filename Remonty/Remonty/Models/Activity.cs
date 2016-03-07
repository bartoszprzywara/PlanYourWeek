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
            Title = "Tytuł zadania 1";
            Description = "Opis zadania 1";
            Priority = 0;
            IsAllDay = false;
            StartHour = new TimeSpan(17, 34, 56);
            StartDate = new DateTimeOffset(new DateTime(2016,03,29));
            EndDate = new DateTimeOffset(new DateTime(2016,03,30)); ;
            Estimation = 3;
            Context = 1;
            Project = 2;
        }

        public Activity(string Title, string Description, int? Priority, bool? IsAllDay,
                        TimeSpan? StartHour, DateTime? StartDate, DateTime? EndDate,
                        int? Estimation, int? Context, int? Project)
        {
            this.Title = Title;
            this.Description = Description;
            this.Priority = Priority;
            this.IsAllDay = IsAllDay;
            if (StartHour != null)
                this.StartHour = (TimeSpan)StartHour;
            if (StartDate != null)
                this.StartDate = new DateTimeOffset((DateTime)StartDate);
            if (EndDate != null)
                this.EndDate = new DateTimeOffset((DateTime)EndDate); ;
            this.Estimation = Estimation;
            this.Context = Context;
            this.Project = Project;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public int? Priority { get; set; }
        public bool? IsAllDay { get; set; }
        public TimeSpan StartHour { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int? Estimation { get; set; }
        public int? Context { get; set; }
        public int? Project { get; set; }
    }

    public class ActivityManager
    {
        public static List<Activity> getActivities()
        {
            var activities = new List<Activity>();

            activities.Add(new Activity("Kupić farbę", "Biała 10l, Zielona 5l", 2, true, null, null, new DateTime(2016, 04, 21), 4, 0, null));
            activities.Add(new Activity());
            //activities.Add(new Activity(null, null, null, null, null, null, null, null, null, null));
            activities.Add(new Activity("Pomalować kuchnię", "Na zielono", 1, false, new TimeSpan(16, 00, 00), new DateTime(2016, 04, 23), new DateTime(2016, 04, 23), 6, 4, 1));

            return activities;
        }
    }
}
