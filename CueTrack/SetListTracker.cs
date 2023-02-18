using LXProtocols.AvolitesWebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CueTrack
{
    /// <summary>
    /// Tracks the active track of the running set list between the backup and main consoles.
    /// </summary>
    public class SetListTracker
    {
        /// <summary>
        /// Gets or sets the the active track within the cue list.
        /// </summary>
        public int ActiveTrackId { get; set; }

        /// <summary>
        /// Called periodically to perform the sync between backup and main consoles.
        /// </summary>
        /// <param name="master">The master connection to sync to.</param>
        /// <param name="backup">The backup connection to sync to.</param>
        /// <param name="timeStamp">The time stamp.</param>
        public async Task Pulse(Titan master, Titan backup, DateTime timeStamp)
        {
            try
            {
                var masterActiveTrack = await master.SetList.GetActiveTrack();
                var backupActiveTrack = await backup.SetList.GetActiveTrack();

                if(masterActiveTrack != null)
                {
                    if (masterActiveTrack.TitanId != ActiveTrackId || masterActiveTrack.TitanId != backupActiveTrack?.TitanId)
                    {
                        await backup.SetList.FireTrack(HandleReference.FromTitanId(masterActiveTrack.TitanId));
                        ActiveTrackId = masterActiveTrack.TitanId;

                        Console.WriteLine($"TRACK {masterActiveTrack.Legend} FIRED");
                    }
                }

            }
            catch (JsonException ex)
            {
                Console.WriteLine($"ERROR: " + ex.Message);
            }
        }
    }
}
