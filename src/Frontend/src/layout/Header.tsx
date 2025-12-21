import { HeaderNavigation } from "./HeaderNavigation";
import { Store } from "../utility/propertyStore";
import {  useSyncExternalStore } from "preact/compat";
import { AdvancedToggle } from "../components/AdvancedToggle";
import { GameProperty, Mapper } from "pokeaclient";
import { IconButton } from "@/components/IconButton";
import { useUISetting } from "../Contexts/UISettingsContext";
import { Toasts } from "../notifications/ToastStore";

async function performReload(mapper: Mapper, preserveFreeze: boolean ) {
	if (!preserveFreeze) {
		await Store.client.changeMapper(mapper.fileId);
		return;
	}

	let frozenProperties: GameProperty<any>[] = [];
	if (preserveFreeze) {
		const propertyMap = Store.getAllProperties();
		frozenProperties = Object.getOwnPropertyNames(propertyMap)
			.map(x => propertyMap[x])
			.filter(x => x.isFrozen);
	}
	const success = await Store.client.changeMapper(mapper.fileId);
	if (success && frozenProperties.length > 0) {
		frozenProperties.forEach(
			property => {
				console.log(property.path + " " + property.value);
				Store.client.updatePropertyBytes(property.path, property.bytes, true)
			}
		);
		Toasts.push("Reloaded mapper with "+ frozenProperties.length + " frozen values reapplied", "", "green");
	} else {
		if (frozenProperties.length > 0) {
			Toasts.push("Failed to reapply frozen values. Frozen values are unfortunately lost. :/", "", "red");
		}
	}
}

export function Header() {
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);
	const [preserveFreeze] = useUISetting("preserveFreeze");
	const reloadMapper = async () => {
		if (mapper != null) {
			performReload(mapper, !!preserveFreeze);
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