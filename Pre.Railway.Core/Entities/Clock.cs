using Pre.Railway.Core.Entities.Api.Departures;
using Pre.Railway.Core.Event_Args;
using Pre.Railway.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Entities
{
    public class Clock
    {
        public event EventHandler ClockTick;

        public DateTime CurrentTime { get; private set; }
        public int Delay { get; }
        public string TimeString { get { return CurrentTime.ToString("T"); } }

        public InfrabelService InfrabelService { get; set; }

        public Clock(int delay)
        {
            Delay = delay;
        }

        public async void StartClock()
        {
            while (true)
            {
                CurrentTime = DateTime.Now;
                ClockTick?.Invoke(this, EventArgs.Empty);
                await Task.Delay(Delay);
            }            
        }

        public async void DetectDepartures()
        {
            while (true)
            {
                CurrentTime = DateTime.Now;
                InfrabelService.CompareCurrentWithDepartureTime(this);
                await Task.Delay(Delay * 30);
            }
        }

        public async void SilentLiveBoardUpdate()
        {
            while (true)
            {
                try
                {
					await InfrabelService.GetDeparturesAsync();
					InfrabelService.LiveBoardUpdated();
					await Task.Delay(Delay * 30);
				}
               catch (Exception ex)
                {
                    throw new Exception($"oh no: {ex.Message}");
                }
            }
        }

    }
}
