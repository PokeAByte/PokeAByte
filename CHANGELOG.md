# Future

## Breaking

* Replaced the previous frontend (UI) with a new one written in Preact.
* Removed the internal performance measurements and the `SHOW_READ_LOOP_STATISTICS` setting.
* Removed the intermediate `PropertyModel` type that is returned from some REST endpoints. The data given to clients may slightly differ now.
* Removed the following REST endpoints:
  * `files/mapper/refresh_archived_list`
* Moved the following APIs from `IPokeAByteProperty` to `IPokeAByteInstance`:
	* `FreezeProperty()` 
	* `UnfreezeProperty()` 
	* `WriteValue()` 
	* `WriteBytes()` 

## Features

* Added linux support for the Bizhawk integration tool.
* Added linux support for the Bizhawk driver.

## Performance

* Improved the performance of the bizhawk driver by being smarter with the work done per read and reducing the memory overhead.
* Some minor performance improvements by not repeatedly parsing the `Bits` string.
* Reduced memory footprint by getting rid of blazor/mudblazor.
  * This also improved performance somewhat because the update mechanism for the blazor UI no longer exists.
* Minor memory usage improvement by only instantiating the Bizhawk and Retroarch drivers when a connection to the respective
  emulator can be established.
* Fixed some minor memory leaks that occur when repeatedly loading and unloading mappers.
* Serialize `IPokeAByteProperty` directly instead of using an intermediary anonymous object for the `PropertiesChanged` websocket event.
* Use one contigous byte array for storing game memory, this skips a bunch of work to find the proper `ByteArray` based on address and length.
* Reduced memory use and memory-churn.

Overall, using the Pokemon Emerald mapper and Bizhawk on Linux, I see a reduction in CPU usage of ~30% and a reduction 
of memory utilization of ~34%. 

# 0.8.1 (2024-11-09)
## Bug Fix

* Fixed a race condition in MapperClientService.cs in function HandlePropertyChangedEvent where _pendingUpdates was being modified while it was being read

# 0.8.0 (2024-11-09)

## Features

* Added a new Websocket message Hello that provides the current mapper metadata and glossary.
* Added a new API for mapper javascripts: __mapper.copy_properties(sourcePath, destinationPath) can be used to
* copy properties in bulk from C#, as the JavaScript implementation may be slow for some mappers.
* Added new REST APIs:
	* /mapper-service/get-mappers
	* /mapper-service/is-connected
	* /mapper-service/change-mapper
	* /mapper-service/get-properties
	* /mapper-service/get-glossary
	* /mapper-service/unload-mapper

## Bugfixes

* Fixed the "Open Mapper Folder" button on Linux (and hopefully macOS)
* Fixed downloading of mappers on linux caused by case mismatch of the target download folder.
* Fixed a failure to reconnect to RetroArch after restarting the emulator. Bugreport
* Fixed some values not updating in the Poke-A-Byte web UI properly (caused by failing to length-validate a glossary string).

## Performance

* Drastically improved the performance of the RetroArch driver (also used for SuperShuckie).
* Improved performance of various mappers by avoiding the silent throwing and catching of exceptions when
    re-calculating memory addresses. Any mapper that sets variables.reload_addresses = true; will likely benefit.
* Reduced memory use and in property processing.
* Various other general performance improvements in property processing.
* The Poke-A-Byte web-UI no longer creates excessive CPU load:
    * Now only updates every 15 milliseconds, to reduce the amount of work needed.
    * The UI now only updates properties that have actually changed.
    * Fixed an issue with lingering unused memory after browser tab has been closed.

## Other

* Poke-A-Byte now logs the version and commit hash on startup.
