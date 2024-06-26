﻿using Pre.Railway.Core.Entities;
using Pre.Railway.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Event_Args
{
    public class ReportDelayEventArgs : EventArgs
    {
        public NmbsService NmbsService { get; }
        public Train DelayedTrain { get; set; }
        public ReportDelayEventArgs(NmbsService nmbsService, Train delayedTrain)
        {
            NmbsService = nmbsService;
            DelayedTrain = delayedTrain;

        }
    }
}
