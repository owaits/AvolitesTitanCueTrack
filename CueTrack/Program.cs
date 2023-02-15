

using CueTrack;
using LXProtocols.AvolitesWebAPI;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Net;

Console.WriteLine("AVOLITES TITAN CUE TRACK");
Console.WriteLine("Synchronises the live cue between all running cue lists allowing a backup console to track the master console.");
Console.WriteLine("Please ensure that you have loaded the same show on the master and backup consoles!");
Console.WriteLine();

var config = new ConfigurationBuilder()
    .AddCommandLine(Environment.GetCommandLineArgs())
    .AddJsonFile("appsettings.json")
    .Build();

string? masterIP = config["master"];
if (string.IsNullOrEmpty(masterIP))
{
    Console.WriteLine("Please enter the IP address of the MASTER console:");
    masterIP = Console.ReadLine();
    Console.WriteLine();
}


if (!IPAddress.TryParse(masterIP, out IPAddress? masterAddress))
{
    Console.WriteLine("Invalid MASTER IP address!");
    Console.ReadLine();
    return;
}

string? backupIP = config["backup"];
if (string.IsNullOrEmpty(backupIP))
{
    Console.WriteLine("Please enter the IP address of the BACKUP console:");
    backupIP = Console.ReadLine();
    Console.WriteLine();
}


if (!IPAddress.TryParse(backupIP, out IPAddress? backupAddress))
{
    Console.WriteLine("Invalid BACKUP IP address!");
    Console.ReadLine();
    return;
}

int port = int.Parse(config["port"] ?? "4430");

Titan master = new Titan(masterAddress, port);
Titan backup = new Titan(backupAddress, port);

while (true)
{    
    //Try to connect to the master consoles.
    while (!await master.IsConnected())
    {
        Console.WriteLine("Waiting for connection to MASTER console...");
        await Task.Delay(30000);
    }

    //Try to connect to the backup consoles.
    while (!await backup.IsConnected())
    {
        Console.WriteLine("Waiting for connection to BACKUP console...");
        await Task.Delay(30000);
    }

    Console.WriteLine($"Started syncing {master.ConnectedDevice.Legend} to backup {backup.ConnectedDevice.Legend}");

    var trackers = new Dictionary<int, PlaybackTracker>();
    var setListTracker = new SetListTracker();

    try
    {
        while (true)
        {
            var timeStamp = DateTime.UtcNow;

            await setListTracker.Pulse(master, backup, timeStamp);

            var handleUpdates = (await master.Handles.GetHandles("Playbacks", verbose: true)).Where(item => item.Type == "cueListHandle");

            var orphanedTracker = new HashSet<int>(trackers.Keys);

            foreach (var cueListHandle in handleUpdates)
            {
                PlaybackTracker tracker;
                if (!trackers.TryGetValue(cueListHandle.TitanId, out tracker))
                {
                    tracker = new PlaybackTracker()
                    {
                        TitanId = cueListHandle.TitanId,
                        Legend = cueListHandle.Legend
                    };

                    trackers.Add(tracker.TitanId, tracker);
                }

                orphanedTracker.Remove(tracker.TitanId);
                await tracker.Pulse(cueListHandle, backup, timeStamp);
            }

            foreach (var orphanKey in orphanedTracker)
                trackers.Remove(orphanKey);

            await Task.Delay(1000);
        }
    }
    catch (HttpRequestException)
    {
        Console.WriteLine("LOST CONNECTION!!!");
    }    
}



