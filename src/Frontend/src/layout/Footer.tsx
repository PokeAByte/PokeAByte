/** Renders the footer. */
export function Footer() {
	// @ts-expect-error "__POKEABYTE_VERSION__" is injected via vite.
	const version = __POKEABYTE_VERSION__;
	return (
		<footer>
			Poke-A-Byte {version} © 2026 ( <a href="/ui/license">AGPL</a> )
		</footer>
	)
}