[Version 0.9.0](#090)  
[Version 0.8.1](#081-2024-11-09)  
  
# 0.9.0
- [Features](#features)
- [UI Features](#ui-features)
- [UI Features](#ui-features)
- [Bug fixes](#bug-fixes)
- [Performance](#performance)
- [Other changes](#other-changes)

## Features

* Added Linux support for the BizHawk integration tool.
* Added Linux support for the BizHawk driver.
* Added a "Reload mapper" button to the property editor.
* Reduced the download size from roughly 40mb to 10mb (zipped, exact numbers depend on platform).

## UI features
## Notable UI Changes

### Header
- Status information is displayed in a more direct fashion.
- Removed the "power" button that was just a link to the mapper tab.
- Added a button to unload the current mapper.
- Added a button to reload the current mapper.
- Added a button to toggle advanced mode.

### Advanced mode

Some UI elements are now only shown when the "advanced mode" is active. This mode can be toggled by clicking on the rocket icon in the header.

- Disabled by default
- If disabled, the following features are hidden by default (compared to the old version).
  - "Open mapper folder" and "open archive folder" buttons in the mapper management tab.
  - The "Settings" tab.
  - The property details table (the fold-out that shows type, length, path, ...).

### "Mapper" tab
- Now organized in panels underneath each other, rather than tabs. 
- Poke-A-Byte will remember which panels you opened and closed. This information is stored in your browser.
- "Open Mapper Folder" now creates a toast notification.
- Added a filter dropdown to narrow the mapper selection by category (GEN1, GEN2, ...)
- When restoring or deleting a mapper from the backups, the dialog will inform you about the files that are going to be restored / deleted.

### "Properties" tab

- The save icon button is only shown when a change was made (but not yet saved).
- An undo button has been added to discard pending changes.
- The "freeze" icon button was moved to the left side of the input.
- When clicking on the freeze icon with a pending edit, the property will be saved as frozen with the new value.
  Same as if you saved first and then froze.
- Advanced mode: The address of the property is shown to the right of the input.
- Advanced mode: Properties can be hidden. To unhide the properties, click the eye-icon at the top of the properties
  page to show all hidden properties, then toggle the ones you want to display again.
- The property details table only shows entries that have data. e.g. if a property does not have a `size`, then the size
  row is omitted.
- Edits made in the "bytes" row can now be saved. A contextual undo button was also added.
- When clicking on the "copy" button of the bytes row: The string copied to clipboard has a different format.
  Previous: "00 00 00 00"
  New: "0x00 0x00 0x00 0x00"
- Removed the warning text about "deprecated" mappers.
- Removed "unload mapper" button (see header).

### Settings:
- When clearing your github settings, a confirmation dialog will open first.


### Other:
- Toast notifications now appear on the right.
- Errors encountered by Poke-A-Byte while processing the mapper are now shown as toast notifications
  - These notifications do not automatically disappear.
  - Repeat notifications with the same text are merged.
- If more than 5 notifications are active at the same time, a "Delete all" button is shown.
- Added a link to a newly added license page in the footer. Click on the underlined "AGPL" to get there.
- Various tweaks to the wording of confirmation notifications.

## Bug fixes
* The Poke-A-Byte Integration Tool for BizHawk no longer crashes if it is closed and then re-opened in the emulator.
* Fixed compatibility of the RetroArch driver with macOS (with a lot of help from MVF).

## Performance

* The UI should be more responsive. Having the UI open should also be less of a strain on Poke-A-Byte.
* Improved the performance of the BizHawk driver by being smarter with the work done per read and reducing memory overhead.
* Various performance improvements in property processing.
* Various performance improvements for reading the memory from the individual emulator drivers.
* Dropping the old UI reduced the base CPU and memory load.
* Avoid some work and memory use by only initializing drivers if and when they are used.

### Performance comparison
Mapper: "(GEN3) pokemon emerald deprecated ne:"
- CPU: 1 core @ **7 %** -> **3,38 %**
- RAM: **220 MB**  -> **196 MB**

Mapper: "(GEN3) pokemon emerald":
- CPU: 1 core @ **19.55 %** -> **5,88 %**
- RAM: **280 MB** -> **180 MB**

(Performance gains may vary by platform. They definitely vary by mapper and target emulator)

### EDPS Bizhawk integration performance

CPU load reported for `EmuHawk.exe`  (as percent of a single CPU core):  
**Pokemon Emerald**  
`PokeAByte.Integrations.BizHawk.dll`: **80 %**, 240 fps  
`EDPS.Bizhawk.dll`: **53 %**, 240 fps

**Pokemon Platin**  
`PokeAByte.Integrations.BizHawk.dll`: 120 %, **185 fps**  
`EDPS.Bizhawk.dll`: 125%, **240 fps**

## Other changes
- The frontend no longer references public CDNs for fonts or images. This should mean Poke-A-Byte can be used offline with less problems.
- Some calls to the GitHub API are now being cached for 60 seconds.

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
* Fixed downloading of mappers on Linux caused by case mismatch of the target download folder.
* Fixed a failure to reconnect to RetroArch after restarting the emulator. 
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
