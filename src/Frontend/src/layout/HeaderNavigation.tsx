import classNames from "classnames";
import { useLocation } from "wouter";
import { Mapper } from "pokeaclient";

export function HeaderNavigation({ mapper }: { mapper: Mapper | null }) {
	const [location, setLocation] = useLocation();
	return (
		<>
			<button
				onClick={() => setLocation("/mapper/")}
				role="link"
				type="button"
				className={classNames({ "active": location === "/" || location.startsWith("/mapper") })}
			>
				<i className="material-icons"> catching_pokemon </i>
				MAPPERS
			</button>
			<button
				role="link"
				type="button"
				onClick={() => setLocation("/properties/")}
				disabled={!mapper}
				className={classNames({ "active": location === "/properties/" })}
			>
				<i className="material-icons"> api </i>
				PROPERTIES
			</button>
			<button
				role="link"
				type="button"
				onClick={() => setLocation("/settings/")}
				className={classNames({ "active": location.startsWith("/settings") })}
			>
				<i className="material-icons">api</i>
				SETTINGS
			</button>
		</>
	)
}