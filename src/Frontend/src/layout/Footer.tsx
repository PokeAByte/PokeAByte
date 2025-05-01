import { Link } from "wouter";

export function Footer() {
	return (
		<footer>
			<span>
				Poke-A-Byte 2025 (AGPL)
			</span>
			<span>
				<Link to="/license/"> License </Link>
			</span>
		</footer>
	)
}