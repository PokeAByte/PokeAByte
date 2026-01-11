
import { Store } from "../../../utility/propertyStore";
import { PropertyValue } from "./PropertyValue";
import { useStorageRecordState } from "../../../hooks/useStorageState";
import { hiddenOverrideSignal, hiddenProperties } from "../../../Contexts/hiddenPropertySignal";
import { IfNotHidden } from "../../../components/IfNotHidden";
import { GameProperty } from "pokeaclient";
import { propertySearchSignal, PropertyTreeNode } from "../PropertyEditor";
import { mapperSignal } from "@/Contexts/mapperSignal";
import { PropertyTreeHeader } from "./PropertyTreeHeader";

function matchProperty(property: GameProperty<any>|undefined|null, query: string) {
	if (!property) {
		return false;
	}
	return property.path.toLowerCase().includes(query)
		|| property.address?.toString(16) === query
}

export function PropertyTree({ node }: { node: PropertyTreeNode }) {
	const mapperId = mapperSignal.peek()!.id;
	const [isOpen, setIsOpen] = useStorageRecordState(mapperId, node.path, false);
	const override = hiddenOverrideSignal.value;
	const search = propertySearchSignal.value;
	const hiddenItemCount = node.children
		?.filter(x => !override && hiddenProperties.peek().includes(x.path))
		.length

	let immediateChildren: PropertyTreeNode[];
	if (search) {
		immediateChildren = node.children
			?.filter(leaf => 
				leaf.allChildren
					? leaf.allChildren.some(x => matchProperty(Store.getProperty(x), search))
					: matchProperty(Store.getProperty(leaf.path), search)
			)
			?? [];
	} else {
		immediateChildren = node.children
			?.filter(x => override || !hiddenProperties.value.includes(x.path))
			?? []
	}
		
	if (!node.children) {
		return (
			<IfNotHidden path={node.path}>
				<PropertyValue mapperId= {mapperId} path={node.path} />
			</IfNotHidden>
		);
	}
	if (propertySearchSignal.value && immediateChildren.length == 0) {
		return null;
	}
	const openOverride = propertySearchSignal.value && (immediateChildren.length <= 10);
	return (
		<>
			<PropertyTreeHeader 
				isOpen={isOpen}
				onToggleOpen={() => setIsOpen(!isOpen)}
				node={node}
				entryCount={immediateChildren.length}
				hiddenEntryCount={hiddenItemCount ?? 0}
			/>
			<tr class={(isOpen || openOverride) ? "" : "hidden"}>
				<td colSpan={2}>
					{(isOpen || openOverride) &&
						<table class="property-table">
							<tbody>
								{immediateChildren.map(x => {
									return <PropertyTree key={x.key} node={x} />;
								})}
							</tbody>
						</table>
					}
				</td>
			</tr>
		</>
	)
}
