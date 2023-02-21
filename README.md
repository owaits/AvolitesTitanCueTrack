# Avolites Titan Cue Track

Cue Track is intended to be used with an Avolites Titan Lighting Console where a backup scenario synchronises the live cue between a master and backup console. It uses a WebAPI connection between the two consoles to read the current cue from the master and replicate that on the backup console running the same show. In addition to the live cue it will also sychronise the current set list track ensuring that master and backup consoles are always on the same page. You may run this program on a seperate computer on on the backup console.

# Getting Started

You first need to have a valid network between the master and backup consoles as well as any device you are running this software on. This software uses the WebAPI port 4430 or 4431 to communicate with the two consoles. You need to make a note of the master and backup IP addresses as these will be required.

- Load the show from the master console into the backup. Ensure that any DMX and Synergy output configuration is correct. You may also use the Backup mode of Titan Net.
- Run AvolitesTitanCueTrack.exe
- You will be asked for the master IP address, please enter it
- You will be asked for the backup IP address, please enter it
- If the connection is established it will indicate a successful connection, if its waiting for a connection then there is a problem connecting to one of the consoles and you should investigate the problem.
- Now when you change the live cue on the master it will be replicated on the backup. As the cues change this will be printed to the screen.

# Configuration

As well as entering the master and backup IP addresses when the application starts two additional configuration methods exist.

## Command Line Options

The following options can be used on the command line to set the tracking options and bypass the requirement to enter them within the application.

- `/master {IP Address}` - this sets the master IP address of the Titan console acting as the master.
- `/backup {IP Address}` - this sets the backup IP address of the Titan console acting as the backup.
- `/port {Port Number}` - the default WebAPI port is 4430 but if you are using a non standard port you may set it here.

e.g. `AvolitesTitanCueTrack.exe /master 10.0.0.1 /backup 10.0.0.2 /port 4431`

## Application Settings

Within the application directory is the appsettings.json file which allows you to specify options that will be used each time the application starts.

- `"master": "{IP Address}"` - this sets the master IP address of the Titan console acting as the master.
- `"backup": "{IP Address}"` - this sets the backup IP address of the Titan console acting as the backup.
- `"port": "{Port Number}"` - the default WebAPI port is 4430 but if you are using a non standard port you may set it here.

```
{
  "master": "10.0.0.1",
  "backup": "10.0.0.2",
  "port": 4431
}
```

# Set List Synchronisation

Due to the default behaviour of WebAPI running in its own user enviroment the Cue List sync will appear not to function. With this in mind its recomended you enable to GUI WebAPI by updating the app.config file of the console you are using and setting the wepapi.enabled option to true. You will also then need to change the port used to 4431.

