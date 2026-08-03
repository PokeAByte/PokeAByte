import { mapperFilesSignal } from "@/Contexts/mapperFilesSignal";
import { uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { useAPI } from "@/hooks/useAPI";
import { changeMapper } from "@/api/fetch";
import { Panel } from "../../../components/Panel";
import { onMapperLoaded } from "./onMapperLoaded";
import { CSSProperties } from "preact";
import { getMapperColors } from "@/utility/getMapperColors";
import { useComputed } from "@preact/signals";
import { beautifyMapperName } from "@/utility/mapperName";

export function RecentPanel() {
	const isEnabled = useComputed(() => uiSettingsSignal.value.recentlyUsedEnabled).value;
	const recentMappers = useComputed(() => uiSettingsSignal.value.recentMappers).value;
	const mapperFiles = mapperFilesSignal.value;
	const changeMapperApi = useAPI(changeMapper, onMapperLoaded);
	const mappers = recentMappers?.map(path => mapperFiles.availableMappers?.find(mapper => mapper.path == path))
		.filter(x => !!x);

	if (!isEnabled || !mappers?.length) {
		return null;
	}

	return (
		<Panel id="mapper-recent" title="Recently used mappers" defaultOpen>
			<div class="favorites flexy-panel">
				{mappers?.map((favorite) => {
					const mapperName = beautifyMapperName(favorite);
					const buttonColors = getMapperColors(mapperName);
					const style: CSSProperties = {};
					if (buttonColors) {
						style.borderColor = buttonColors.border;
						style.backgroundColor = buttonColors.bg;
						style.color = buttonColors.text;
					}
					return (
						<button
							onClick={() => changeMapperApi.call(favorite.path)}
							style={style}
						>
							{mapperName}
						</button>
					);
				})}
			</div>
		</Panel>
	);
}