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

        public Activity(string title, string description, int? priorityId, bool? isAllDay,
                        DateTimeOffset? startDate, TimeSpan? startHour, DateTimeOffset? endDate, TimeSpan? endHour,
                        int? estimationId, int? contextId, int? projectId)
        {
            Title = title;
            Description = description;
            PriorityId = priorityId;
            IsAllDay = isAllDay;
            if (startDate != null)
                StartDate = startDate;
            if (startHour != null && IsAllDay == false)
                StartHour = startHour;
            if (endDate != null)
                EndDate = endDate;
            if (endHour != null && IsAllDay == false)
                EndHour = endHour;
            EstimationId = estimationId;
            ContextId = contextId;
            ProjectId = projectId;
            IsDone = false;
        }

        [SQLite.Net.Attributes.PrimaryKey, SQLite.Net.Attributes.AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? PriorityId { get; set; }
        public bool? IsAllDay { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public TimeSpan? StartHour { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public TimeSpan? EndHour { get; set; }
        public int? EstimationId { get; set; }
        public int? ContextId { get; set; }
        public int? ProjectId { get; set; }
        public bool IsDone { get; set; }
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
        public string ContextUI
        {
            get
            {
                if (ContextId != null)
                    return LocalDatabaseHelper.ReadItem<Context>((int)ContextId).Name;
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
                if (PriorityId != null)
                    return LocalDatabaseHelper.ReadItem<Priority>((int)PriorityId).Name;
                return "";
            }
        }
    }
}
