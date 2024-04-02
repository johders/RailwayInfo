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

        public Train AffectedTrain { get; set; }

        public List<Train> CurrentLiveBoard { get; private set; }

        public Task<List<Train>> Delays { get; private set; }

        public List<Train> DepartedTrains { get; set; }

        public List<string> Announcements { get; set; } = new List<string>();


        //public NmbsService(InfrabelService infrabelService)
        //{
        //    InfrabelService = infrabelService;
        //}
        public void UpdateLiveBoard(List<Train> currentLiveBoard)
        {
            CurrentLiveBoard = currentLiveBoard;
            Delays = InfrabelService.DetectDelays(CurrentLiveBoard);
        }

        public void LogAnnouncement(string announcement)
        {
            string date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");

            StringBuilder sb = new StringBuilder();
            sb.Append(announcement);
            sb.AppendLine(date);
            sb.AppendLine();
            sb.AppendLine("---------------------------------------------------------------------");
            sb.AppendLine();

           Announcements.Add(sb.ToString());
        }

        public void CreateLogFile()
        {
            string date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss");
            string fileName = $"trainlog-{date}.txt";
            string programDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string path = Path.Combine(programDirectory, fileName).Replace("bin\\Debug\\net6.0-windows", "LogFiles");

            WriteService.WriteToFile(path, Announcements);
            
        }

        public string FormatTrainEventInfo()
        {
            int delayInMinutes = int.Parse(String.Concat(AffectedTrain.Delay.Skip(3).Take(2)));
            string min = delayInMinutes == 1 ? "minuut" : "minuten";

             //$"📢 Opgelet: "
            return $"spoor {AffectedTrain.Platform} De trein naar {AffectedTrain.Destination} heeft {delayInMinutes} {min} vertraging";
        }
    }
}
