using Remonty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Remonty.Helpers
{
    public class ToastNotificationHelper
    {
        public static void AddNotification(Activity act)
        {
            if (act.List != "Zaplanowane") return;

            // creating notification message
            string when = (act.StartHour == null) ? "dzisiaj" : "od " + act.StartHourUI;
            when += string.IsNullOrEmpty(act.EstimationUI) ? "" : " przez " + act.EstimationUI;
            string contentString =
            "<toast scenario=\"reminder\" duration=\"long\">" +
                "<visual>" +
                    "<binding template=\"ToastGeneric\">" +
                        "<text id=\"1\">" + act.Title + "</text>" +
                        "<text id=\"2\">" + when + "</text>" +
                    "</binding>" +
                "</visual>" +
                "<commands>" +
                    "<command id=\"snooze\"/>" +
                    "<command id=\"dismiss\"/>" +
                "</commands>" +
            "</toast>";
            Windows.Data.Xml.Dom.XmlDocument content = new Windows.Data.Xml.Dom.XmlDocument();
            content.LoadXml(contentString);

            // setting notification delivery time
            DateTimeOffset scheduledTime = ((DateTimeOffset)act.StartDate).LocalDateTime;
            if (act.StartHour != null)
                scheduledTime = scheduledTime.Add((TimeSpan)act.StartHour);
            else
                scheduledTime = scheduledTime.Add(new TimeSpan(9, 0, 0));
            //scheduledTime = DateTime.Now.AddSeconds(5); // debug setting

            // creating new notification
            var newToast = new Windows.UI.Notifications.ScheduledToastNotification(content, scheduledTime, TimeSpan.FromMinutes(5), 0);
            newToast.Id = act.Id.ToString();

            // removing old scheduled notification if exists
            RemoveNotification(act);

            // adding new notification to schedule
            Windows.UI.Notifications.ToastNotifier toastNotifier = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            toastNotifier.AddToSchedule(newToast);
        }

        public static void RemoveNotification(Activity act)
        {
            if (act.List != "Zaplanowane") return;

            Windows.UI.Notifications.ToastNotifier toastNotifier = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();

            foreach (var scheduledToast in toastNotifier.GetScheduledToastNotifications())
                if (scheduledToast.Id == act.Id.ToString())
                    toastNotifier.RemoveFromSchedule(scheduledToast);
        }

        private async void AddToCalendar(Activity tempActivity)
        {
            Appointment appointment = new Appointment();
            //Activity tempActivity = LoadActivityValuesFromControls();

            appointment.Subject = tempActivity.Title;
            appointment.Details = tempActivity.Description;
            appointment.Location = tempActivity.ContextUI;
            appointment.Reminder = TimeSpan.FromHours(1);
            appointment.StartTime = ((DateTimeOffset)tempActivity.StartDate).Date;
            string activityAppointmentId = string.Empty; // to powinno być pole aktywności activity.AppointmentId

            if (tempActivity.StartHour != null)
                appointment.StartTime += (TimeSpan)tempActivity.StartHour;

            if (tempActivity.EstimationId != null)
                appointment.Duration = TimeSpan.FromHours(LocalDatabaseHelper.ReadItem<Estimation>((int)tempActivity.EstimationId).Duration);

            var rect = new Rect(new Point(Window.Current.Bounds.Width / 2, Window.Current.Bounds.Height / 2), new Size());

            string newAppointmentId;

            if (string.IsNullOrEmpty(activityAppointmentId))
            {
                newAppointmentId = await AppointmentManager.ShowReplaceAppointmentAsync(activityAppointmentId, appointment, rect, Placement.Default);

                if (string.IsNullOrEmpty(newAppointmentId))
                    newAppointmentId = activityAppointmentId;
            }
            else
                newAppointmentId = await AppointmentManager.ShowAddAppointmentAsync(appointment, rect, Placement.Default);

            LocalDatabaseHelper.ExecuteQuery("UPDATE Activity SET AppointmentId = '" + newAppointmentId + "' WHERE Id = " + tempActivity.Id);
            App.PlannedWeekNeedsToBeReloaded = true;
        }

        async private Task CreateCalenderEntry()
        {
            // 1. get access to appointmentstore 
            var appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);

            // 2. get calendar 
            var appCustomApptCalendar = await appointmentStore.CreateAppointmentCalendarAsync("MyCalendar");

            // 3. create new Appointment 
            var appointment = new Appointment();
            appointment.AllDay = true;
            appointment.Subject = "Mein Termin";
            appointment.StartTime = DateTime.Now;

            //  4. add 
            await appCustomApptCalendar.SaveAppointmentAsync(appointment);
        }
    }
}
