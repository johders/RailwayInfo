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
        public string LiveBoardMessage 
        { 
            get
            {
                int delayInMinutes = int.Parse(String.Concat(Train.Delay.Skip(3).Take(2)));
                string min = delayInMinutes == 1 ? "minuut" : "minuten";

                return $"📢 Opgelet: spoor {Train.Platform} De trein naar {Train.Destination} heeft {delayInMinutes} {min} vertraging";
            }
                
        }

        public DelayEventArgs(Train delayedTrain)
        {
            Train = delayedTrain;
        }
    }
}
