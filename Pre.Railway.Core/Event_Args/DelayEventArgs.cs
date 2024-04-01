using Pre.Railway.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Event_Args
{
    public class DelayEventArgs : EventArgs
    {
        public Train Train { get; set; }

        public DelayEventArgs(Train delayedTrain)
        {
            Train = delayedTrain;
        }
    }
}
