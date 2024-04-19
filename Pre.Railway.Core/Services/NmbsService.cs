using Pre.Railway.Core.Entities;
using Pre.Railway.Core.Event_Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Services
{
    public class NmbsService
    {
        public List<Train> Delays { get; private set; } = new List<Train>();

        public List<Train> DepartedTrains { get; private set; } = new List<Train>();

        public List<string> LogAnnouncements { get; } = new List<string>();

        public List<string> LiveBoardAnnouncements { get; } = new List<string>();

        public List<string> SpeechAnnouncements { get; set; } = new List<string>();


        public string LogFilePath { get; private set; }

        PromptBuilder promptBuilder = new PromptBuilder();
        PromptBuilder promptBuilderQueue = new PromptBuilder();

        public void ChangeLogPath(string station)
        {
            LogFilePath = CreateNewLogFile(station);
        }

        public void ClearPreviousStationInfo()
        {
            Delays.Clear();
            DepartedTrains.Clear();
        }

        public List<string> FilterAnnouncements()
        {
            List<string> allAnnouncements = new List<string>();
            List<string> output = new List<string>();

            foreach (Train train in Delays)
            {
                allAnnouncements.Add(FormatSpeechinfoDelay(train));
            }

            foreach (Train train in DepartedTrains)
            {
                allAnnouncements.Add(FormatSpeechinfoDeparted(train));
            }

            foreach(string announcement in allAnnouncements)
            {
                if (!SpeechAnnouncements.Contains(announcement))
                {
                    output.Add(announcement);
                }
            }

            return output;

        }

        public void UpdateLiveBoardAnnouncements()
        {
            LiveBoardAnnouncements.Clear();


            foreach (Train train in Delays)
            {
                LiveBoardAnnouncements.Add(FormatTrainDelayEventInfo(train));
            }

            foreach (Train train in DepartedTrains)
            {
                LiveBoardAnnouncements.Add(FormatTrainDepartedEventInfo(train));
            }

        }

        public void UpdateSpeechAnnouncements()
        {
            SpeechAnnouncements.Clear();
            promptBuilder.ClearContent();

            foreach (Train train in Delays)
            {
                SpeechAnnouncements.Add(FormatSpeechinfoDelay(train));
            }

            foreach (Train train in DepartedTrains)
            {
                SpeechAnnouncements.Add(FormatSpeechinfoDeparted(train));
            }
        }

        public void UpdateLogFileAnnouncements()
        {
            LogAnnouncements.Clear();

            foreach (Train train in Delays)
            {
                LogAnnouncement(FormatTrainDelayEventInfo(train));
            }

            foreach (Train train in DepartedTrains)
            {
                LogAnnouncement(FormatTrainDepartedEventInfo(train));
            }

            WriteToLogFile();
        }

        public string CreateNewLogFile(string station)
        {
            //    string stationName = CurrentStation;
            string date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss");
            string fileName = $"trainlog-{date}-{station}.txt";
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

        public string FormatSpeechinfoDelay(Train affectedTrain)
        {
            int delayInMinutes = int.Parse(String.Concat(affectedTrain.Delay.Skip(3).Take(2)));
            if (delayInMinutes == 0) delayInMinutes = 60;

            return $"Platform {affectedTrain.Platform}. Train with destination {affectedTrain.Destination} has a {delayInMinutes} minute delay";
        }

        public string FormatSpeechinfoDeparted(Train affectedTrain)
        {
            return $"Platform {affectedTrain.Platform}. Train with destination {affectedTrain.Destination} has departed at {affectedTrain.DepartureTime}";
        }

        public string FormatTrainDelayEventInfo(Train affectedTrain)
        {
            int delayInMinutes = int.Parse(String.Concat(affectedTrain.Delay.Skip(3).Take(2)));
            if (delayInMinutes == 0) delayInMinutes = 60;
            string min = delayInMinutes == 1 ? "minuut" : "minuten";

            return $"spoor {affectedTrain.Platform} De trein naar {affectedTrain.Destination} heeft {delayInMinutes} {min} vertraging";
        }

        public string FormatTrainDepartedEventInfo(Train affectedTrain)
        {
            return $"De trein naar {affectedTrain.Destination} is vertrokken om {affectedTrain.DepartureTime}";
        }

        public void WriteToLogFile()
        {
            WriteService.WriteToFile(LogFilePath, LogAnnouncements);
        }

        public async Task ReadText()
        {
            UpdateSpeechAnnouncements();

            List<string> announcementsCopy = new();

            var result = SpeechAnnouncements.Any(a => a.Equals(announcementsCopy));

            foreach (string announcement in SpeechAnnouncements)
            {
                promptBuilder.AppendText(announcement);
                promptBuilder.AppendBreak(PromptBreak.Medium);
            }

            announcementsCopy = SpeechAnnouncements;

            SpeechSynthesizer synth = new SpeechSynthesizer();

            synth.SpeakAsync(promptBuilder);

            synth.SpeakCompleted += Synth_SpeakCompleted;

            //synth.Dispose();

        }

        public async Task ReadQueueAsync()
        {

            //List<string> announcementsQueue = new();

            //foreach (string announcement in SpeechAnnouncements)
            //{

            //    if (!SpeechAnnouncements.Contains(announcement))
            //    {
            //        announcementsQueue.Add(announcement);
            //    }

            //}

            //if(announcementsQueue.Count == 0)
            //{
            //    return;
            //}

            var restult = FilterAnnouncements();

            foreach (string announcement in restult)
            {
                promptBuilderQueue.AppendText(announcement);
                promptBuilderQueue.AppendBreak(PromptBreak.Medium);
            }

            if(FilterAnnouncements().Count == 0)
            {
                return;
            }

            SpeechSynthesizer synth = new SpeechSynthesizer();
 
            synth.SpeakAsync(promptBuilderQueue);
        }

        private void Synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            ReadQueueAsync();
        }
    }
}
