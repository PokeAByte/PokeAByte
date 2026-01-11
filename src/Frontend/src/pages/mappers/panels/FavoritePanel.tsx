import { mapperFilesSignal } from "@/Contexts/mapperFilesSignal";
import { uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { useAPI } from "@/hooks/useAPI";
import { changeMapper } from "@/utility/fetch";
import { Panel } from "../../../components/Panel";
import { onMapperLoaded } from "./createMapperLoadToast";
import { CSSProperties } from "preact";
import { getMapperColors } from "@/utility/getMapperColors";
import { useComputed } from "@preact/signals";

export function FavoritePanel() {
	const favoriteIds = useComputed(() => uiSettingsSignal.value.favoriteMappers).value;
	const mapperFileContext = mapperFilesSignal.value;
	const changeMapperApi = useAPI(changeMapper, onMapperLoaded);
	const favorites = favoriteIds?.map(id => mapperFileContext.availableMappers?.find(mapper => mapper.id == id))
		.filter(x => !!x);

	if (!favorites?.length) {
		return null;
	}

	return (
		<Panel id="mapper-favorites" title="Favorite mappers" defaultOpen>
			<div class="favorites">
				{favorites?.map((favorite) => {
					const buttonColors = getMapperColors(favorite.displayName);
					const style: CSSProperties = {};
					if (buttonColors) {
						style.borderColor = buttonColors.border;
						style.backgroundColor = buttonColors.bg;
						style.color = buttonColors.text;
					}
					return (
						<button
							class="margin-right"
							onClick={() => changeMapperApi.call(favorite.id)}
							style={style}
						>
							{favorite.displayName}
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