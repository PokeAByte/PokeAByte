import { useContext } from "preact/hooks";
import { Link } from "wouter";
import { MapperFilesContext } from "@/Contexts/availableMapperContext";
import { useUISetting } from "@/Contexts/UISettingsContext";
import { useAPI } from "@/hooks/useAPI";
import { changeMapper } from "@/utility/fetch";
import { Panel } from "../../../components/Panel";
import { createMapperLoadToast } from "./createMapperLoadToast";
import { CSSProperties } from "preact";
import { getMapperColors } from "@/utility/getMapperColors";

export function FavoritePanel() {
	const [favoriteIds] = useUISetting("favoriteMappers");
	const mapperFileContext = useContext(MapperFilesContext);
	const changeMapperApi = useAPI(changeMapper, createMapperLoadToast);
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
				You can manage your favorites <Link href="~/ui/settings/#settings_ui">here</Link>.
			</span>
		</Panel>
	);
}