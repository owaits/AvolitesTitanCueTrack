

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
    .AddJsonFile("appsettings.json")
    .AddCommandLine(Environment.GetCommandLineArgs())
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

while(!await master.IsConnected() || !await backup.IsConnected())
{
    Console.WriteLine("Waiting for connection to MASTER and BACKUP console...");
    await Task.Delay(30000);
}

Console.WriteLine($"Started syncing {master.ConnectedDevice.Legend} to backup {backup.ConnectedDevice.Legend}");

var trackers = new List<PlaybackTracker>();
var handles = await master.Handles.GetHandles("Playbacks");

foreach (var handle in handles.Where(item => item.Type == "cueListHandle"))
{
    trackers.Add(new PlaybackTracker()
    {
        TitanId = handle.TitanId,
        Legend = handle.Legend
    });
}

while (true)
{
    var handleUpdates = await master.Handles.GetHandles("Playbacks", verbose: true);
    foreach(var tracker in trackers)
    {
        var handle = handleUpdates.FirstOrDefault(item=> item.TitanId == tracker.TitanId);
        if(handle != null)
        {
            await tracker.Pulse(handle, backup);
        }        
    }
    await Task.Delay(1000);
}



