using Pre.Railway.Core.Entities;
using Pre.Railway.Core.Event_Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Services
{
    public class NmbsService
    {
        public InfrabelService InfrabelService { get; }

        public Train AffectedTrain { get; internal set; }

        public List<Train> CurrentLiveBoard { get; private set; }

        public List<Train> Delays { get; private set; } = new List<Train>();

        public List<Train> DepartedTrains { get; private set; } = new List<Train>();

        public List<string> LogAnnouncements { get; } = new List<string>();

        public List<string> LiveBoardAnnouncements { get; } = new List<string>();

        public string LogFilePath { get; }


        public NmbsService()
        {
            LogFilePath = CreateLogFileOnStartup();
        }
       
        public void UpdateAnnouncements()
        {
            LiveBoardAnnouncements.Clear();

            foreach(Train train in Delays)
            {
                LiveBoardAnnouncements.Add(FormatTrainEventInfo(train));
            }

            foreach(Train train in DepartedTrains)
            {
                LiveBoardAnnouncements.Add(FormatTrainEventInfo(train));
            }
        }

        public string FormatTrainEventInfo(Train affectedTrain)
        {
            int delayInMinutes = int.Parse(String.Concat(affectedTrain.Delay.Skip(3).Take(2)));
            if (delayInMinutes == 0) delayInMinutes = 60;
            string min = delayInMinutes == 1 ? "minuut" : "minuten";

            return $"spoor {affectedTrain.Platform} De trein naar {affectedTrain.Destination} heeft {delayInMinutes} {min} vertraging";
        }

        private string CreateLogFileOnStartup()
        {
            string date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss");
            string fileName = $"trainlog-{date}.txt";
            string programDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            return Path.Combine(programDirectory, fileName).Replace("bin\\Debug\\net6.0-windows", "LogFiles");
        }
        public void LogAnnouncement(string announcement)
        {
            string date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(date);
            sb.AppendLine();
            sb.AppendLine(announcement);        
            sb.AppendLine("=================================================================");
            
           LogAnnouncements.Add(sb.ToString());
        }

        public void WriteToLogFile()
        {           
            WriteService.WriteToFile(LogFilePath, LogAnnouncements);           
        }


    }
}
