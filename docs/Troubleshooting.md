# Troubleshooting guide

## Update Poke-A-Byte

Please make sure that you are on the latest version of Poke-A-Byte. You can check the [releases page](https://github.com/PokeAByte/PokeAByte/releases) for the newest version and the terminal output of Poke-A-Byte. The first two lines of the terminal output of Poke-A-Byte tell you which version you are on:

```
14:00 [Information] (PokeAByte.Web.Program) Poke-A-Byte version: 0.8.2
14:00 [Information] (PokeAByte.Web.Program) Poke-A-Byte commit: 5dee723384d37f99053b33e6644ecd7cd8d707db
14:00 [Information] (PokeAByte.Domain.Models.AppSettings) AppSettings initialized: RETROARCH_DELAY_MS_BETWEEN_READS: 5 and BIZHAWK_DELAY_MS_BETWEEN_READS: 5
14:00 [Information] (Microsoft.Hosting.Lifetime) Now listening on: "http://localhost:8085"
14:00 [Information] (Microsoft.Hosting.Lifetime) Application started. Press Ctrl+C to shut down.
```

If these two lines are missing, you are using a version older than `0.8.0` and should update before trying any other trouble shooting step.

## "Driver was not found" / "No driver could connect to an emulator"

If you are getting one of the following error messages, please check your emulator settings.

- `Failed to load mapper. Message: Driver was not found.` (old message)
- `Failed to load mapper. Message: No driver could connect to an emulator`

For RetroArch, make sure that "Network Comamnds" are enabled and that the "Network Command Port" is `55355`.
The two settings are under `Settings` -> `Network`, near the bottom of the list.

SuperShuckie does not need to be configured and should work out of the box.

BizHawk required the "BizHawk Integration Tool", a build of which can be downloaded in the Discord. As of right now, the BizHawk integration only works on Windows.

## "Failed to load mapper: Max attempts reached."

This indicates that the driver could connect to the emulator, but that something went wrong reading data. 

1. Make sure you are using the correct mapper for the game, this includes game language.
2. Make sure the mapper you are using is up to date. 
   1. Go to "Mappers" -> "UPDATE"
   2. Find your mapper in the list and check the box to select it.
   3. Click the "Update Selected" button above the list.
3. If you are using RetroArch or SuperShuckie, make sure that your operating system is not interfering with the network traffic between the emulator and Poke-A-Byte via a firewall. 

Sometimes it also helps if you close both Poke-A-Byte and the emulator completely and restart both. 