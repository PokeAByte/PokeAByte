import { defineConfig } from 'vite'
import preact from "@preact/preset-vite";

import { readFileSync } from 'fs';
const match = readFileSync("../PokeAByte.Web/PokeAByte.Web.csproj", "utf-8").match(/(?:\<AssemblyVersion\>)((\d+\.)+\d)/);
let version = "";
if (match) {
	version = match[1];
}


// https://vitejs.dev/config/
export default defineConfig({
	plugins: [preact()],
	build: {
		target: "safari17"
	},
	define: {
        '__POKEABYTE_VERSION__': JSON.stringify(version)
    }
})
