using LXProtocols.AvolitesWebAPI;
using LXProtocols.AvolitesWebAPI.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CueTrack
{
    /// <summary>
    /// Keeps track of an individual playback ensuring the load state and current cue stay in sync.
    /// </summary>
    public class PlaybackTracker
    {
        /// <summary>
        /// Gets or sets the ID of the cue list being synced between the main and backup consoles.
        /// </summary>
        public int TitanId { get; set; }

        /// <summary>
        /// Gets or sets the legend of the playback being tracked.
        /// </summary>
        public string? Legend { get; set; }

        /// <summary>
        /// Gets or sets the current load state for the playback.
        /// </summary>
        public bool Loaded { get; set; }

        /// <summary>
        /// Gets or sets the current cue ID for the cue list.
        /// </summary>
        public int CueId { get; set; }

        /// <summary>
        /// Gets or sets the last update to this tracked playback.
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Called periodically to carry out the sync and update the state.
        /// </summary>
        /// <param name="handle">The most recent handle information for the playback.</param>
        /// <param name="backup">The backup connection to sync to.</param>
        public async Task Pulse(HandleInformation handle, Titan backup, DateTime timeStamp)
        {
            bool loaded = handle.Active;
            if(Loaded != loaded)
            {
                if(loaded)
                {
                    await backup.Playbacks.Fire(HandleReference.FromTitanId(TitanId));
                    Console.WriteLine($"{Legend} LOADED");
                }
                else
                {
                    await backup.Playbacks.Kill (HandleReference.FromTitanId(TitanId));
                    Console.WriteLine($"{Legend} KILLED");
                }
                
                Loaded = loaded;
            }

            
                if(Loaded)
                {
                    int? cueId = handle.Information[1]["LiveCue"]?.GetValue<int>();
                    if(cueId != null && CueId != cueId)
                    {
                        try
                        {
                            var cueInformation = await backup.CueLists.GetCue(TitanId, (int) cueId);

                            await backup.CueLists.SetNextCue(HandleReference.FromTitanId(TitanId), cueInformation.CueNumber);
                            await backup.CueLists.Play(HandleReference.FromTitanId(TitanId));
                        
                            Console.WriteLine($"{Legend}:{cueInformation.Legend} CUE {cueInformation.CueNumber}");
                        }
                        catch (JsonException ex)
                        {
                            Console.Write(ex.Message);
                        }
                    
                        CueId = (int) cueId;                        
                    }
                }  

            LastUpdate = timeStamp;
        }
    }
}
