using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace AppCenter_WPF_Sample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            AppCenterStart();

            base.OnStartup(e);
        }

        private void AppCenterStart()
        {            
            AppCenter.LogLevel = LogLevel.Verbose;

            AppCenter.Start("ee7111d8-fdb8-4888-a98d-82702842f3b0",
                   typeof(Analytics), typeof(Crashes));

            Crashes.SendingErrorReport += Crashes_SendingErrorReport;
            Crashes.SentErrorReport += Crashes_SentErrorReport;
            Crashes.FailedToSendErrorReport += Crashes_FailedToSendErrorReport;

            Crashes.ShouldProcessErrorReport = (ErrorReport report) =>
            {
                bool processErrorReport = true;

                IDictionary<String, String> dictionary = report.GetType()
                  .GetProperties()
                  .Where(p => p.CanRead && p.PropertyType == typeof(String))
                  .ToDictionary(p => p.Name, p => (String)p.GetValue(report, null));
                                
                Analytics.TrackEvent(Events.ShouldProcessErrorReport.ToString(), dictionary);

                return processErrorReport;
            };

            Crashes.HasCrashedInLastSessionAsync().ContinueWith((resultTask) =>
            {
                if (resultTask.Result)
                {
                    Analytics.TrackEvent(Events.CrashedLastSession.ToString());
                }                
            });
        }

        private void Crashes_FailedToSendErrorReport(object sender, FailedToSendErrorReportEventArgs e)
        {
            Analytics.TrackEvent(Events.FailedToSendErrorReport.ToString());
        }

        private void Crashes_SentErrorReport(object sender, SentErrorReportEventArgs e)
        {
            Analytics.TrackEvent(Events.SentErrorReport.ToString());
        }

        private void Crashes_SendingErrorReport(object sender, SendingErrorReportEventArgs e)
        {
            Analytics.TrackEvent(Events.SendingErrorReport.ToString());
        }

        public enum Events
        {
            CrashedLastSession,
            SendingErrorReport,
            SentErrorReport,
            FailedToSendErrorReport,
            ShouldProcessErrorReport
        }
    }
}
