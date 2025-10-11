import { defineConfig } from 'vite'
import preact from "@preact/preset-vite";
import { readFileSync } from 'fs';

const match = readFileSync("../PokeAByte.Web/PokeAByte.Web.csproj", "utf-8")
	.match(/(?:\<AssemblyVersion\>)((\d+\.)+\d)/);
const version = match ? match[1] : "";

export default defineConfig({
	plugins: [preact()],
	define: {
        '__POKEABYTE_VERSION__': JSON.stringify(version)
    }
})
