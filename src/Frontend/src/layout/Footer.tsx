import { Link } from "wouter";

/** Renders the footer. */
export function Footer() {
	// @ts-expect-error "__POKEABYTE_VERSION__" is injected via vite.
	const version = globalThis.__POKEABYTE_VERSION__;
	return (
		<footer>
			Poke-A-Byte {version} © 2025 ( <Link to="/license/">AGPL</Link> )
		</footer>
	)
}