import { VisibilityToggle } from "@/components/VisibilityToggle";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";
import { Show } from "@preact/signals/utils";
import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { PropertyTreeNode } from "../PropertyEditor";
import { Icon } from "@/components/Icon";

type PropertyTreeHeaderProps = {
	onToggleOpen: () => void,
	isOpen: boolean,
	node: PropertyTreeNode,
	entryCount: number,
	hiddenEntryCount: number,
}

export function PropertyTreeHeader(props: PropertyTreeHeaderProps) {
	const secondaryName = useGamePropertyField(props.node.secondaryNamePath ?? "", "value");

	return (
		<tr class="leaf interactive" onClick={props.onToggleOpen}>
			<th>
				<Icon name={props.isOpen ? "folder" : "folder_open"}/>
				<span class="margin-left">
					{props.node.key}
					{secondaryName &&
						<span> - {secondaryName?.toString()} </span>}
				</span>
			</th>
			<td>
				<span class="margin-left color-darker">
					{props.entryCount} Entries
					{props.hiddenEntryCount > 0 && ` (+${props.hiddenEntryCount} hidden)`}
				</span>
				<Show when={advancedModeSignal}>
					<VisibilityToggle path={props.node.path} />
				</Show>
			</td>
		</tr>
	);
}
