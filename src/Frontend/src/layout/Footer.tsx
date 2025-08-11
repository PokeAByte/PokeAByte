import { Link } from "wouter";

/** Renders the footer. */
export function Footer() {
	return (
		<footer>
			Poke-A-Byte {globalThis.__POKEABYTE_VERSION__} 2025 ( <Link to="/license/">AGPL</Link> )
		</footer>
	)
}