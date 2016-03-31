using Remonty.Helpers;
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

        public Activity(bool isPlaceholderActivity)
        {
            IsPlaceholder = isPlaceholderActivity;
            PriorityId = 1;
        }

        public Activity(string title, string description, int priorityId, bool isAllDay, string list,
                        DateTimeOffset? startDate, TimeSpan? startHour, DateTimeOffset? endDate, TimeSpan? endHour,
                        int? estimationId, int? contextId, int? projectId)
        {
            Title = title;
            Description = description;
            PriorityId = priorityId;
            IsAllDay = isAllDay;
            List = list;
            if (startDate != null && List == "Zaplanowane")
                StartDate = startDate;
            if (startHour != null && List == "Zaplanowane" && IsAllDay == false)
                StartHour = startHour;
            if (endDate != null)
                EndDate = endDate;
            if (endHour != null && IsAllDay == false && EndDate != null)
                EndHour = endHour;
            EstimationId = estimationId;
            ContextId = contextId;
            ProjectId = projectId;
            IsDone = false;
            IsPlaceholder = false;
        }

        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PriorityId { get; set; }
        public bool IsAllDay { get; set; }
        public string List { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public TimeSpan? StartHour { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public TimeSpan? EndHour { get; set; }
        public int? EstimationId { get; set; }
        public int? ContextId { get; set; }
        public int? ProjectId { get; set; }
        public bool IsDone { get; set; }
        public bool IsPlaceholder { get; set; }

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
                    return ((DateTimeOffset)StartDate).LocalDateTime.ToString("dd.MM.yyyy");
                return List;
            }
        }
        public string ContextUI
        {
            get
            {
                if (ContextId != null)
                    return "@" + LocalDatabaseHelper.ReadItem<Context>((int)ContextId).Name;
                return "";
            }
        }
        public string ProjectUI
        {
            get
            {
                if (ProjectId != null)
                    return LocalDatabaseHelper.ReadItem<Project>((int)ProjectId).Name;
                return "";
            }
        }
        public string EstimationUI
        {
            get
            {
                if (EstimationId != null)
                    return LocalDatabaseHelper.ReadItem<Estimation>((int)EstimationId).Name;
                return "";
            }
        }
        public string PriorityUI
        {
            get
            {
                if (PriorityId == 1)
                    return "Gray";
                else if (PriorityId == 2)
                    return "Green";
                else
                    return "Red";
            }
        }
    }
}
