------ Server README -------

Enter geographical coordinates (in decimal format) into the console, or press Enter twice to load the default coordinates.
The server will then start.
By default, the server runs on port 42069. You need to forward this port in your router settings.
You can now connect through the app using the correct IP:Port combination.
If this port is being used by other devices in your current router, check on command prompt
netstat -a -n -o | findstr :42069
If you see a line with 42069, the port is already in use. Change the port in settings

The server automatically saves the game world after every player action to the savegame.hex file.
This file is loaded on every restart.
To start a new world, simply delete this file, and the server will create a new savegame.
