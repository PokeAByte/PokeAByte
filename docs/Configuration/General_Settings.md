# General settings

Can be changed in the [browser UI ](http://localhost:8085/ui/settings) and are stored in `$configFolder/PokeAByte/settings.json`. 

## DELAY_MS_BETWEEN_READS

How long Poke-A-Byte waits in between reading from the emulator. The default is 5 milliseconds or reading memory 200 times a second.

If you struggle with performance, you can try to increase this value to reduce the workload, as this also affects whatver software you are using that is listening to game property updates.

## PROTOCOL_FRAMESKIP

When using [SuperShuckie](https://github.com/SnowyMouse/supershuckie) or [BizHawk](https://tasvideos.org/BizHawk) (see [How To guide](../HowTo.md#bizhawk)), this will tell the emulator to skip frames in between updating the game memory for Poke-A-Byte. By default this is 0 except for Mappers targeting the Nintendo DS platform where the default is 15. 

This has a similar effect as `DELAY_MS_BETWEEN_READS`, but can also help improve performance on the emulator side. 

## RETROARCH_LISTEN_IP_ADDRESS

The IP address used to connect to RetroArch. Default is `127.0.0.1`.

## RETROARCH_LISTEN_PORT

The port used to connect to RetroArch. Default is `55355`.

## RETROARCH_READ_PACKET_TIMEOUT_MS

The maximum time to wait for a response from RetroArch when reading memory before giving up. The default is 64 milliseconds.
