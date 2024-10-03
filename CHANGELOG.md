# FUTURE

## Features

- The BizHawk integration / driver now also works on linux.
- Added a new Websocket message `Hello` that provides the current mapper metadata and glossary.
- Added a new API for mapper javascripts: `__mapper.copy_properties(sourcePath, destinationPath)` can be used to
  copy properties in bulk from C#, as the JavaScript-native way may be slow for some mappers.

## Bugfixes

- Fixed a failure to reconnect to RetroArch after restarting the emulator. [Bugreport](https://github.com/PokeAByte/PokeAByte/issues/9)
- 

## Performance

- Poke-A-Byte now reuses the memory accessor used to talk to BizHawk, instead of re-recreating the connection on every
  read.
- Drastically improved the performance of the RetroArch driver (also used for SuperShuckie).
- Improved performance of various mappers by avoiding the silent throwing and catching of exceptions when
  re-calculating memory addresses. Any mapper that sets `variables.reload_addresses = true;` will likely benefit.
- Various other general performance improvements in property processing.
- Improved memory use in property processing.
- The Poke-A-Byte web-UI no creates excessive CPU work:
  - Now only updates every 15 milliseconds, to reduce the amount of work needed.
  - The UI now only updates properties that actually had changes.
  - Fixed an issue with lingering unused memory after browser tab has been closed.
