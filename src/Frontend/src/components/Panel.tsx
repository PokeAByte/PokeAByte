import { saveSetting, uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { useEffect } from "preact/hooks";
import { useComputed } from "@preact/signals";
import { ComponentChild } from "preact";

export type PanelProps = {
	title: string, 
	defaultOpen?: boolean, 
	children: ComponentChild,
	id: string
}

/** A collapsible panel with a title. */
export function Panel(props: PanelProps) {
	const openPanels = useComputed(() => uiSettingsSignal.value.openPanels).value;
	const setOpen = (isOpen: boolean) => {
		saveSetting("openPanels", {...openPanels, [props.id]: isOpen });
	}
	useEffect(() => {
		if (document.location.hash === '#' + props.id) {
			setOpen(true);
		}
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []);

	return (
		<details
			class="panel"
			open={openPanels[props.id] ?? props.defaultOpen}
			onToggle={event => setOpen(event.currentTarget.open)}
		>
			<summary>{props.title}</summary>
			<article>
				{(openPanels[props.id] ?? props.defaultOpen) ? props.children : null}
			</article>
		</details>
	);
}
