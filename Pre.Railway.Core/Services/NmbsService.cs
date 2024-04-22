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
        public List<Train> AnnouncedDelay { get; set; } = new List<Train>();

        public List<Train> AnnouncedDeparture { get; set; } = new List<Train>();

        public List<string> LiveBoardAnnouncements { get; } = new List<string>();

        public List<string> SpeechAnnouncements { get; set; } = new List<string>();
        public bool Speaking { get; set; }
        private List<string> allreadyRead = new List<string>();
        private int speechHelper = 0;
        public string LogFilePath { get; private set; }

        PromptBuilder promptBuilderQueue = new PromptBuilder();

        public void ChangeLogPath(string station)
        {
            LogFilePath = CreateNewLogFile(station);
        }

        public void ClearPreviousStationInfo()
        {
            Delays.Clear();
            DepartedTrains.Clear();
            AnnouncedDelay.Clear();
            AnnouncedDeparture.Clear();
            LiveBoardAnnouncements.Clear();
            SpeechAnnouncements.Clear();
            speechHelper = 0;
        }

        public void UpdateLogFileAndLiveBoardAnnouncements()
        {
            LogAnnouncements.Clear();

            foreach (Train train in Delays)
            {
                if (!AnnouncedDelay.Contains(train))
                {
                    LogAnnouncement(FormatTrainDelayEventInfo(train));
                    LiveBoardAnnouncements.Add(FormatTrainDelayEventInfo(train));
                    SpeechAnnouncements.Add(FormatSpeechinfoDelay(train));
                    AnnouncedDelay.Add(train);
                }
            }

            foreach (Train train in DepartedTrains)
            {
                if (!AnnouncedDeparture.Contains(train))
                {
                    LogAnnouncement(FormatTrainDepartedEventInfo(train));
                    LiveBoardAnnouncements.Add(FormatTrainDepartedEventInfo(train));
                    SpeechAnnouncements.Add(FormatSpeechinfoDeparted(train));
                    AnnouncedDeparture.Add(train);
                }
            }

            WriteToLogFile();
        }

        public string CreateNewLogFile(string station)
        {
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

        public async Task TestSpeechAsync(string text)
        {
            SpeechSynthesizer testSynth = new SpeechSynthesizer();
            testSynth.SpeakAsync(text);
        }

        
        public async Task ReadQueueAsync()
        {

            var toBeRead = SpeechAnnouncements;

            if (toBeRead.Count == 0)
            {
                return;
            }

            promptBuilderQueue.ClearContent();

            if(speechHelper == 1)
            {
                toBeRead.RemoveAt(0);
            }

            foreach (string announcement in toBeRead)
            {
                if(speechHelper == 1)
                {
                    promptBuilderQueue.AppendText(announcement);
                    promptBuilderQueue.AppendBreak(PromptBreak.Medium);
                }
                if (!allreadyRead.Contains(announcement))
                {
                    promptBuilderQueue.AppendText(announcement);
                    promptBuilderQueue.AppendBreak(PromptBreak.Medium);
                    allreadyRead.Add(announcement);
                }
            }

            if (!Speaking)
            {
                SpeechSynthesizer queueSynth = new SpeechSynthesizer();
                Speaking = true;
                queueSynth.SpeakAsync(promptBuilderQueue);
                queueSynth.SpeakCompleted += QueueSynth_SpeakCompleted;
            }

        }
        

        private void QueueSynth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            Speaking = false;
            speechHelper++;
            if (speechHelper == 1)
            {
                ReadQueueAsync();
                speechHelper++;
            }
        }

    }
}
