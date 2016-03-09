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

        // TODO: wywalic ktorys konstruktor
        public Activity(string Title, string Description, string Priority, bool? IsAllDay,
                        TimeSpan? StartHour, DateTime? StartDate, DateTime? EndDate,
                        string Estimation, string Context, string Project)
        {
            this.Title = Title;
            this.Description = Description;
            this.Priority = Priority;
            this.IsAllDay = IsAllDay;
            if (StartHour != null && this.IsAllDay == false)
                this.StartHour = (TimeSpan)StartHour;
            if (StartDate != null)
                this.StartDate = new DateTimeOffset((DateTime)StartDate);
            if (EndDate != null)
                this.EndDate = new DateTimeOffset((DateTime)EndDate); ;
            this.Estimation = Estimation;
            this.Context = Context;
            this.Project = Project;
        }

        public Activity(string Title, string Description, string Priority, bool? IsAllDay,
                        TimeSpan? StartHour, DateTimeOffset? StartDate, DateTimeOffset? EndDate,
                        string Estimation, string Context, string Project)
        {
            this.Title = Title;
            this.Description = Description;
            this.Priority = Priority;
            this.IsAllDay = IsAllDay;
            if (StartHour != null && this.IsAllDay == false)
                this.StartHour = (TimeSpan)StartHour;
            if (StartDate != null)
                this.StartDate = (DateTimeOffset)StartDate;
            if (EndDate != null)
                this.EndDate = (DateTimeOffset)EndDate;
            this.Estimation = Estimation;
            this.Context = Context;
            this.Project = Project;
        }

        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public bool? IsAllDay { get; set; }
        public TimeSpan? StartHour { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string Estimation { get; set; }
        public string Context { get; set; }
        public string Project { get; set; }
        public string StartDateUI
        {
            get
            {
                if (StartDate != null)
                    return ((DateTimeOffset)StartDate).ToString("dd.MM.yyyy");
                return "";
            }
        }
        public string StartHourUI
        {
            get
            {
                if (StartHour != null)
                    return ((TimeSpan)StartHour).ToString(@"hh\:mm");
                return "";
            }
        }
    }
}
