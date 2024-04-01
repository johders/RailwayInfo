using Pre.Railway.Core.Entities;
using Pre.Railway.Core.Event_Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Services
{
    public class NmbsService
    {
        public InfrabelService InfrabelService { get; }

        public List<Train> CurrentLiveBoard { get; private set; }

        public Task<List<Train>> Delays { get; private set; }

        public List<Train> DepartedTrains { get; set; }

        public List<string> Announcements { get; set; }

        public NmbsService(InfrabelService infrabelService)
        {
            InfrabelService = infrabelService;
        }

        public void UpdateLiveBoard(List<Train> currentLiveBoard)
        {
            CurrentLiveBoard = currentLiveBoard;
            Delays = InfrabelService.DetectDelays(CurrentLiveBoard);
        }

        public static void AddAnnouncement()
        {
            
        } 
    }
}
