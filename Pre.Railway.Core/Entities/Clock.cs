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

        public Clock(int delay)
        {
            
        }


        public void StartClock()
        {
            ClockTick?.Invoke(this, EventArgs.Empty);
        }

        public void StopClock() 
        {
        
        }

    }
}
