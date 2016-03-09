using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class Activity
    {
        public Activity(string Title, string Description, string Priority, bool? IsAllDay,
                        TimeSpan? StartHour, DateTime? StartDate, DateTime? EndDate,
                        string Estimation, string Context, string Project)
        {
            this.ID = Activities.Instance.getProposedID();
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
            this.ID = Activities.Instance.getProposedID();
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

        public Activity(int ID, string Title, string Description, string Priority, bool? IsAllDay,
        TimeSpan? StartHour, DateTimeOffset? StartDate, DateTimeOffset? EndDate,
        string Estimation, string Context, string Project)
        {
            this.ID = ID;
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

        public int ID { get; set; }
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

    public class Activities
    {
        private static Activities m_oInstance = null;
        private int m_nCounter = 0;
        public List<Activity> activities = new List<Activity>();

        public static Activities Instance
        {
            get
            {
                if (m_oInstance == null)
                {
                    m_oInstance = new Activities();
                    ActivityManager.AddSomeActivities();
                }
                return m_oInstance;
            }
        }

        public void addActivity(Activity activity)
        {
            activities.Add(activity);
            m_nCounter++;
        }

        public List<Activity> getActivities()
        {
            return activities;
        }

        public int getProposedID()
        {
            return m_nCounter;
        }

        
    }


    /*
    public class ActivityManager
    {
        public static List<Activity> getActivities()
        {
            var activities = new List<Activity>();

            activities.Add(new Activity("Kupić farbę", "Biała 10l, Zielona 5l", "Wysoki", true, null, null, new DateTime(2016, 04, 21), "2 godz", "Zakupy", null));
            activities.Add(new Activity("Tytuł zadania 1", "Opis zadania 1", "Niski", false, new TimeSpan(17, 34, 56), new DateTime(2016, 03, 29), new DateTime(2016, 03, 30), "1 godz", "Spotkanie", "Położyć panele"));
            activities.Add(new Activity("Pomalować kuchnię", "Na zielono", "Normalny", false, new TimeSpan(16, 00, 00), new DateTime(2016, 04, 23), new DateTime(2016, 04, 23), "4 godz", "Kuchnia", "Pomalować mieszkanie"));
            //activities.Add(new Activity(null, null, null, null, null, null, null, null, null, null));

            return activities;
        }
    }
    */

    public class ActivityManager
    {
        public static void AddSomeActivities()
        {
            Activities.Instance.addActivity(new Activity("Kupić farbę", "Biała 10l, Zielona 5l", "Wysoki", true, null, null, new DateTime(2016, 04, 21), "2 godz", "Zakupy", null));
            Activities.Instance.addActivity(new Activity("Tytuł zadania 1", "Opis zadania 1", "Niski", false, new TimeSpan(17, 34, 56), new DateTime(2016, 03, 29), new DateTime(2016, 03, 30), "1 godz", "Spotkanie", "Położyć panele"));
            Activities.Instance.addActivity(new Activity("Pomalować kuchnię", "Na zielono", "Normalny", false, new TimeSpan(16, 00, 00), new DateTime(2016, 04, 23), new DateTime(2016, 04, 23), "4 godz", "Kuchnia", "Pomalować mieszkanie"));
            Activities.Instance.addActivity(new Activity(null, null, null, null, null, null, null, null, null, null));
        }
    }
}
