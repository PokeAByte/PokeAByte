# GameHook
Explore and modify emulated games in real-time via a built-in dashboard and WebAPI.

### Summary
GameHook is designed for research and investigation into system memory
of populator video game systems. This information is made available
in an easy-to-use JSON format that is human-readable. \
Live updates to this data are provided through websockets.
 \
 \
Join our Discord Server \
https://gamehook.io/discord

### Documentation
https://gamehook.io/quickstart

https://gamehook.io/scripting

### VSCode Installation
1) Clone the repo: `git clone https://github.com/Scotts-Thoughts/gamehook.git`
2) Open VSCode and click `File > Open Folder`, find where you cloned the repo and select the `src` folder
3) Open a new terminal in VSCode: this can be done by clicking `Terminal` on the top of the window and then `New Terminal`
4) Click into the terminal and type `dotnet publish`. This will publish all projects into their respective directories
 - BizHawk External Tool: `\src\GameHook.Integrations.BizHawk\bin\Release\net48\publish`
 - GameHook WebAPI: `\src\GameHook.WebAPI\bin\Release\net8.0\win-x64\publish`
 - Mapper Tree Builder: `\src\GameHook.Utility.BuildMapperTree\bin\Release\net8.0\publish`
5) To run GameHook go to the published folder and run `GameHook.WebAPI.exe`
