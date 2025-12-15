import { HeaderNavigation } from "./HeaderNavigation";
import { Store } from "../utility/propertyStore";
import {  useSyncExternalStore } from "preact/compat";
import { AdvancedToggle } from "../components/AdvancedToggle";
import { Mapper } from "pokeaclient";
import { IconButton } from "@/components/IconButton";

async function performReload(mapper: Mapper) {
	await Store.client.changeMapper(mapper.fileId);
}

export function Header() {
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);
	const reloadMapper = async () => {
		if (mapper != null) {
			performReload(mapper);
		}
	};
	const textColor = mapper ? "text-green" : "text-red";

	return (
		<header>
			<h1 class={textColor}>
				Poke-A-Byte
			</h1>
			<nav class="tab">
				<HeaderNavigation mapper={mapper} />
			</nav>
			<div class="mapper-info">
				{mapper 
					? <>
						<span class={`margin-right ${textColor}`}  title={"Current mapper: " + mapper.gameName}>
							Connected
						</span>
						<IconButton
							noBorder
							title="Unload Mapper" 
							class="text-red" 
							onClick={Store.client.unloadMapper}
							icon="clear"
						/>
						<IconButton
							noBorder
							title="Reload Mapper" 
							class="text-purple" 
							onClick={reloadMapper}
							icon="refresh"
						/>
					</>
					: "No Mapper loaded"
				}
				<AdvancedToggle />
			</div>		
		</header>
	);
}