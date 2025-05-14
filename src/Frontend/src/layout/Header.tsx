import { useLocation } from "wouter";
import { HeaderNavigation } from "./HeaderNavigation";
import { Store } from "../utility/propertyStore";
import {  useSyncExternalStore } from "preact/compat";
import { AdvancedToggle } from "../Contexts/AdvancedToggle";

export function Header() {
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);
	const [, setLocation] = useLocation();
	const reloadMapper = () => {
		// @ts-expect-error The upstream type definition is incomplete, accessing fileId works just fine.
		Store.client.changeMapper(mapper.fileId).then(() => setLocation("/ui/properties"));
	};
	const textHighlightClass = mapper ? "text-green" : "text-red";
	return (
		<header className="layout-box">
			<h1 class={textHighlightClass}>
				Poke-A-Byte
			</h1>
			<nav className={`tab`}>
				<HeaderNavigation mapper={mapper} />
			</nav>
			<div class={"mapper-info"}>
					{mapper 
						?<>
							<span class={`margin-right ${textHighlightClass}`}  title={"Current mapper: " + mapper.gameName}>Connected</span>
							<i tabIndex={0} title="Unload Mapper" role={"button"} className={`icon-button-bare material-icons text-red`} onClick={Store.client.unloadMapper}> 
								clear 
							</i>
							<i tabIndex={0}  title="Reload Mapper" role={"button"} className={`icon-button-bare material-icons text-purple`} onClick={reloadMapper}> 
								refresh
							</i>
						</>
						: <>
							No Mapper loaded
						</>
					}
					<AdvancedToggle />
			</div>		
		</header>
	);
}