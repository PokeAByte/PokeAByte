import { HeaderNavigation } from "./HeaderNavigation";
import { Store } from "../utility/propertyStore";
import {  useSyncExternalStore } from "preact/compat";
import { AdvancedToggle } from "../components/AdvancedToggle";

export function Header() {
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);
	const reloadMapper = () => {
		if (mapper != null) {
			Store.client.changeMapper(mapper.fileId)
		}
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
						<i
							tabIndex={0} 
							title="Unload Mapper" 
							role={"button"} 
							className={`icon-button-bare material-icons text-red`} 
							onClick={Store.client.unloadMapper}
						> 
							clear 
						</i>
						<i tabIndex={0} 
							title="Reload Mapper" 
							role={"button"} 
							className={`icon-button-bare material-icons text-purple`} 
							onClick={reloadMapper}
						> 
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