import classNames from "classnames";
import { useLocation } from "wouter";
import { Mapper } from "pokeaclient";
import { Advanced } from "../components/Advanced";

export function HeaderNavigation({ mapper }: { mapper: Mapper | null }) {
	const [location, setLocation] = useLocation();
	const activeClass = mapper ? "text-green" : "text-red";
	const mapperActive = location === "/" || location.startsWith("/mapper");
	const propertiesActive = location.startsWith("/properties");
	const settingsActive = location.startsWith("/settings");
	return (
		<>
			<button
				onClick={() => setLocation("/mapper/")}
				role="link"
				type="button"
				className={classNames({"active": mapperActive })}
			>
				<i
					className={classNames("material-icons", { [activeClass]: mapperActive })}
					aria-hidden="true"
				> 
					catching_pokemon 
				</i>
				MAPPERS
			</button>
			<button
				role="link"
				type="button"
				onClick={() => setLocation("/properties")}
				disabled={!mapper}
				className={classNames({"active": propertiesActive })}
				>
				<i 
					className={classNames("material-icons", { [activeClass]: propertiesActive })}
					aria-hidden="true"
				> 
					api 
				</i>
				PROPERTIES
			</button>
			<Advanced>
				<button
					role="link"
					type="button"
					className={classNames({"active": settingsActive })}
					onClick={() => setLocation("/settings/")}
				>
					<i 
						className={classNames("material-icons", { [activeClass]: settingsActive })}
						aria-hidden="true"
					>
						settings
					</i>
					SETTINGS
				</button>
			</Advanced>
		</>
	)
}