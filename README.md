# Poke-A-Byte
Explore and modify emulated games in real-time via a built-in dashboard and WebAPI.

### Summary
Poke-A-Byte is designed for research and investigation into system memory
of populator video game systems. This information is made available
in an easy-to-use JSON format that is human-readable. \
Live updates to this data are provided through websockets.

### How to use

[Brief overview available here](./docs/HowTo.md).

### Troubleshooting

See the [troubleshooting guide](./docs/Troubleshooting.md).

### VSCode Installation
1) Clone the repo: `git clone https://github.com/PokeAByte/PokeAByte.git`
2) Open VSCode and click `File > Open Folder`, find where you cloned the repo and select the `src` folder
3) Open a new terminal in VSCode: this can be done by clicking `Terminal` on the top of the window and then `New Terminal`
4) Click into the terminal and type `dotnet publish`. This will publish all projects into their respective directories
 - BizHawk External Tool: `src/artifacts/publish/PokeAByte.Integrations.BizHawk/release`
 - PokeAByte WebAPI/Frontend: `src/artifacts/publish/PokeAByte.Web/release/`
 - Mapper Tree Builder: `src/artifacts/publish/PokeAByte.Utility.BuildMapperTree/release`
5) To run Poke-A-Byte go to the published folder and run `PokeAByte.Web.exe`
