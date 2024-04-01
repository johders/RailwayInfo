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

        public void StopClock(Clock clock)
        {
            clock.ClockTick -= ClockTick;
        }

    }
}
