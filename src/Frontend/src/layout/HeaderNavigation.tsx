import { Icon } from "@/components/Icon";
import { useLocation } from "@/components/Route";
import { className } from "@/utility/className";
import { Mapper } from "pokeaclient";

export function HeaderNavigation({ mapper }: { mapper: Mapper | null }) {
	const [location, setLocation] = useLocation();
	const activeClass = mapper ? "text-green" : "text-red";
	const mapperActive = location === "/" || location.startsWith("/ui/mappers");
	const propertiesActive = location.startsWith("/ui/properties");
	const settingsActive = location.startsWith("/ui/settings");
	return (
		<>
			<button
				onClick={() => setLocation("/mappers")}
				role="link"
				type="button"
				class={className(mapperActive, "active")}
			>
				<Icon name="catching_pokemon" class={className(mapperActive, activeClass)}/>
				MAPPERS
			</button>
			<button
				role="link"
				type="button"
				onClick={() => setLocation("/properties")}
				disabled={!mapper}
				class={className(propertiesActive, "active")}
				>
				<Icon name="api" class={className(propertiesActive, activeClass)}/>
				PROPERTIES
			</button>
			<button
				role="link"
				type="button"
				className={className(settingsActive, "active")}
				onClick={() => setLocation("/settings")}
				>
				<Icon name="settings" class={className(settingsActive, activeClass)}/>
				SETTINGS
			</button>
		</>
	)
}