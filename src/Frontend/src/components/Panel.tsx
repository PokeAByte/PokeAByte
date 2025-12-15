import { useUISetting } from "@/Contexts/UISettingsContext";
import { PanelProps } from "../pages/mappers/MapperPage";
import { useEffect } from "preact/hooks";

export function Panel(props: PanelProps) {
	const [openPanels, setOpenPanels] = useUISetting("openPanels");
	const setOpen = (isOpen: boolean) => {
		setOpenPanels({...openPanels, [props.id]: isOpen });
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
