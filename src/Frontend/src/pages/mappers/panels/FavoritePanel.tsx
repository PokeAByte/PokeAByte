import { mapperFilesSignal } from "@/Contexts/mapperFilesSignal";
import { getFavoriteId, uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { useAPI } from "@/hooks/useAPI";
import { changeMapper } from "@/utility/fetch";
import { Panel } from "../../../components/Panel";
import { onMapperLoaded } from "./createMapperLoadToast";
import { CSSProperties } from "preact";
import { getMapperColors } from "@/utility/getMapperColors";
import { useComputed } from "@preact/signals";
import { LoadProgress } from "@/components/LoadProgress";
import { beautifyMapperName } from "@/utility/mapperName";

export function FavoritePanel() {
	const favoriteIds = useComputed(() => uiSettingsSignal.value.favoriteMappers).value;
	const mapperFileContext = mapperFilesSignal.value;
	const changeMapperApi = useAPI(changeMapper, onMapperLoaded);
	const favorites = favoriteIds?.map(path => mapperFileContext.availableMappers?.find(mapper => getFavoriteId(mapper) == path))
		.filter(x => !!x);

	if (!favorites?.length) {
		return <pre>{JSON.stringify(favoriteIds, null, "")}</pre>;
	}
	if (changeMapperApi.isLoading) {
		return (
			<Panel id="mapper-favorites" title="Favorite mappers" defaultOpen>
				<LoadProgress label="Loading mapper" />
			</Panel>
		);
	}

	return (
		<Panel id="mapper-favorites" title="Favorite mappers" defaultOpen>
			<div class="favorites flexy-panel">
				{favorites?.map((favorite) => {
					const buttonColors = getMapperColors(beautifyMapperName(favorite));
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
							{beautifyMapperName(favorite)}
						</button>
					);
				})}
			</div>
			<br />
			<span>
				You can manage your favorites <a href="/ui/settings/#settings_ui">here</a>.
			</span>
		</Panel>
	);
}