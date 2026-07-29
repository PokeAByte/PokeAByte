# Mappers

A mapper is an XML file, with an optional accompanying JavaScript file, that describes the game properties and how to read them from the game memory.  They are maintained in a [separate github repository](https://github.com/PokeAByte/mappers) and can be downloaded via the browser UI: Go to http://localhost:8085/ui/mappers and open the "Download mappers" panel.

Mappers are downloaded into your local configuration folder, depending on your operating system.

| OS      | Folder                                           | Example                                |
| ------- | ------------------------------------------------ | -------------------------------------- |
| Windows | %AppData%/PokeAByte/Mappers                      | C:\Users\Red\AppData\PokeAByte\Mappers |
| Linux   | $XDG_CONFIG_HOME/PokeAByte/Mappers               | /home/red/.config/PokeAByte/Mappers    |
| MacOS   | ~/Library/Application\ Support/PokeAByte/Mappers | -                                      |

Alternatively, you can open the folder in which mappers are stored by opening the browser UI and clicking the "Open mapper folder" button.

## Archiving a mapper

To archive a mapper, open the browser UI, then open the "Backup mappers" panel, select the mapper in question and click the "Archive selected" button.

This will move the mapper out of the `Mappers` folder and into a newly created folder in `MapperArchives`. For example
`/home/red/.config/PokeAByte/MapperArchives/Archive_2026-07-24_060000/`. The mapper will no longer be available in the "Load mapper" panel until you restore the archive.

The "backup selected mapper" functions in a similar, but instead creates a copy instead of moving the mapper(s).
