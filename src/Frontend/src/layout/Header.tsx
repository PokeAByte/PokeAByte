import { HeaderNavigation } from "./HeaderNavigation";
import { Store } from "../utility/propertyStore";
import { AdvancedToggle } from "../components/AdvancedToggle";
import { GameProperty } from "pokeaclient";
import { IconButton } from "@/components/IconButton";
import { uiSettingsSignal } from "../Contexts/uiSettingsSignal";
import { Toasts } from "../notifications/ToastStore";
import { mapperSignal } from "@/Contexts/mapperSignal";
import { useComputed } from "@preact/signals";

async function performReload( preserveFreeze: boolean ) {
	if (!preserveFreeze) {
		await Store.reloadMapper();
		return;
	}

	let frozenProperties: GameProperty<any>[] = [];
	if (preserveFreeze) {
		const propertyMap = Store.getAllProperties();
		frozenProperties = Object.getOwnPropertyNames(propertyMap)
			.map(x => propertyMap[x])
			.filter(x => x.isFrozen);
	}
	const success = await Store.reloadMapper();
	if (success && frozenProperties.length > 0) {
		frozenProperties.forEach(
			property => {
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
	const mapper = mapperSignal.value;
	const isSticky = useComputed(() => uiSettingsSignal.value.stickyHeader ?? false).value;
	const preserveFreeze = useComputed(() => uiSettingsSignal.value.preserveFreeze ?? false).value;
	const reloadMapper = async () => {
		if (mapper != null) {
			performReload(!!preserveFreeze);
		}
	};
	const textColor = mapper ? "text-green" : "text-red";

	return (
		<header class={isSticky ? "sticky" : ""}>
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